using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PcapDotNet.Core;
using PcapDotNet.Core.Extensions;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;

namespace HdmiExtenderLib
{
	public class HdmiExtenderReceiver
	{
		private IPAddress addressSenderDevice;
		private int networkAdapterIndex = 1;

		private Thread thrKeepAlive;
		private Thread thrDataStream;

		private volatile bool abort = false;

		private byte[] latestImage = null;
		private int audioListenerIDCounter = 0;
		private ConcurrentDictionary<int, ConcurrentQueue<byte[]>> registeredAudioListeners = new ConcurrentDictionary<int, ConcurrentQueue<byte[]>>();

		/// <summary>
		/// Gets a the latest image data, encoded as image/jpeg.
		/// </summary>
		public byte[] LatestImage
		{
			get
			{
				return latestImage;
			}
		}

		public HdmiExtenderReceiver(string ipAddressOfSenderDevice, int networkAdapterIndex)
		{
			this.addressSenderDevice = IPAddress.Parse(ipAddressOfSenderDevice);
			this.networkAdapterIndex = networkAdapterIndex;
		}

		public void Start()
		{
			if (thrKeepAlive != null)
				Stop();

			thrDataStream = new Thread(doDataStream);
			thrDataStream.Name = "Video Streaming Thread";
			thrDataStream.Start();

			thrKeepAlive = new Thread(doKeepAlive);
			thrKeepAlive.Name = "Stream KeepAlive Thread";
			thrKeepAlive.Start();
		}

		public void Stop()
		{
			abort = true;
			try
			{
				thrKeepAlive.Join(300);
				thrDataStream.Join(300);

				thrKeepAlive.Abort();
				thrDataStream.Abort();

				thrKeepAlive = null;
				thrDataStream = null;
			}
			catch (Exception) { }
			abort = false;
		}

		public int RegisterAudioListener(ConcurrentQueue<byte[]> audioData)
		{
			int myKey = Interlocked.Increment(ref audioListenerIDCounter);
			registeredAudioListeners.AddOrUpdate(myKey, audioData, (key, existingValue) => { return audioData; });
			return myKey;
		}

		public void UnregisterAudioListener(int registrationId)
		{
			ConcurrentQueue<byte[]> tmp;
			registeredAudioListeners.TryRemove(registrationId, out tmp);
		}

		/// <summary>
		/// Processes incoming audio and video data.  This method should be run on a new thread.
		/// </summary>
		private void doDataStream()
		{
			try
			{
				JpegAssembler jpgAssembler = new JpegAssembler();

				PacketDevice selectedDevice = LivePacketDevice.AllLocalMachine[networkAdapterIndex];

				// Open the device
				using (PacketCommunicator communicator =
					selectedDevice.Open(65536,	// portion of the packet to capture
					// 65536 guarantees that the whole packet will be captured on all the link layers
										PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
										1000))                                  // read timeout
				{
					// Check the link layer. We support only Ethernet for simplicity.
					if (communicator.DataLink.Kind != DataLinkKind.Ethernet)
					{
						Console.WriteLine("This program works only on Ethernet networks.");
						return;
					}

					// Compile and set the filter
					communicator.SetFilter("ip and udp");

					// start the capture
					var query = from packet in communicator.ReceivePackets()
								//                            where !packet.IsValid
								select packet;


					foreach (Packet packet in query)
					{
						if (abort)
							return;
						if (packet.Ethernet.EtherType == EthernetType.IpV4 &&
							packet.Ethernet.IpV4.Protocol == IpV4Protocol.Udp)
						{
							if (packet.Ethernet.IpV4.Udp.DestinationPort == 2068)
							{
								// Video Data
								// Add lengths of all headers together: ethernet, ipv4, and udp
								int headerLength = packet.Ethernet.HeaderLength + packet.Ethernet.IpV4.HeaderLength + 8;
								byte[] data = new byte[packet.Length - headerLength];
								Array.Copy(packet.Buffer, headerLength, data, 0, data.Length);
								byte[] img = jpgAssembler.AddChunkAndGetNextImage(data);
								if (img != null)
									latestImage = img;
							}
							else if (packet.Ethernet.IpV4.Udp.DestinationPort == 2066)
							{
								// Audio Data
								// Add lengths of all headers together: ethernet, ipv4, and udp
								int headerLength = packet.Ethernet.HeaderLength + packet.Ethernet.IpV4.HeaderLength + 8;
								// These Sender devices put 16 bytes of garbage at the beginning of each audio packet.
								headerLength += 16;
								byte[] data = new byte[packet.Length - headerLength];
								Array.Copy(packet.Buffer, headerLength, data, 0, data.Length);
								foreach (ConcurrentQueue<byte[]> audioQueue in registeredAudioListeners.Values)
									audioQueue.Enqueue(data);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		/// <summary>
		/// Processes incoming control packets from a Sender device, and responds with control packets that keep the audio and video streaming active.  This method should be run on a new thread.
		/// </summary>
		private void doKeepAlive()
		{
			try
			{
				using (UdpClient udp = new UdpClient(new IPEndPoint(IPAddress.Any, 48689)))
				{
					IPEndPoint ep = null;
					while (!abort)
					{
						byte[] data = udp.Receive(ref ep);
						if (ep.Address.Equals(addressSenderDevice))
						{
							byte[] packetNumberBytes = data.Skip(8).Take(2).ToArray<byte>();
							UInt16 controlPacketNumber = BitConverter.ToUInt16(packetNumberBytes, 0);

							controlPacketNumber += 3;
							string hexPacketNumber = controlPacketNumber.ToString("X").PadLeft(4, '0');
							// This number needs to be little-endian.
							hexPacketNumber = hexPacketNumber.Substring(2, 2) + hexPacketNumber.Substring(0, 2);
							string hexControlPacket = "5446367a60020000" + hexPacketNumber + "000303010026000000000d2fd8";
							byte[] controlPacket = HexStringToByteArray(hexControlPacket);
							udp.Send(controlPacket, controlPacket.Length, new IPEndPoint(addressSenderDevice, 48689));
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		#region Util
		public static byte[] HexStringToByteArray(string hex)
		{
			return Enumerable.Range(0, hex.Length)
							 .Where(x => x % 2 == 0)
							 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
							 .ToArray();
		}
		#endregion

	}
}
