using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdmiExtenderLib
{
	//public class HdmiExtenderSender
	//{
	///// <summary>
	///// Broadcasts a Sender Control Packet, imitating an HDMI Extender Sender device.  This packet should be broadcast once per second.
	///// </summary>
	//public void BroadcastSenderControlPacket()
	//{
	//	try
	//	{
	//		using (Socket s = new Socket(SocketType.Dgram, ProtocolType.Udp))
	//		{
	//			string hexPacketNumber = (senderControlPacketCounter++).ToString("X").PadLeft(4, '0');
	//			// This number needs to be little-endian.
	//			hexPacketNumber = hexPacketNumber.Substring(2, 2) + hexPacketNumber.Substring(0, 2);

	//			string hexControlPacket = "5446367a63010000" + hexPacketNumber + "00030303002400000000000000000000001000000000000000000000007800d1a6300001";
	//			byte[] controlPacket = HexStringToByteArray(hexControlPacket);
	//			s.Bind(new IPEndPoint(IPAddress.Parse("192.168.168.57"), 48689));
	//			s.SendTo(controlPacket, new IPEndPoint(IPAddress.Parse("255.255.255.255"), 48689));
	//		}
	//	}
	//	catch (Exception ex)
	//	{
	//		Console.WriteLine(ex.ToString());
	//	}
	//}
	//}
}
