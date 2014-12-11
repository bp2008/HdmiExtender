using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using SimpleHttp;
using System.Text.RegularExpressions;
using System.Web;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace HdmiExtenderLib
{
	public class VideoWebServer : HttpServer
	{
		private HdmiExtenderReceiver receiver;

		public VideoWebServer(int port, int port_https, string senderIPAddress, int networkInterfaceIndex)
			: base(port, port_https)
		{
			receiver = new HdmiExtenderReceiver(senderIPAddress, networkInterfaceIndex);
			receiver.Start();
		}
		public override void handleGETRequest(HttpProcessor p)
		{
			try
			{
				string requestedPage = Uri.UnescapeDataString(p.request_url.AbsolutePath.TrimStart('/'));
				if (requestedPage == "image.jpg")
				{
					byte[] latestImage = receiver.LatestImage;
					if (latestImage == null)
						latestImage = new byte[0];
					p.writeSuccess("image/jpeg", latestImage.Length);
					p.outputStream.Flush();
					p.rawOutputStream.Write(latestImage, 0, latestImage.Length);
				}
				else if (requestedPage.EndsWith("image.mjpg"))
				{
					p.tcpClient.ReceiveBufferSize = 4;
					p.tcpClient.SendBufferSize = 4;
					Console.WriteLine("Beginning mjpg stream");
					p.writeSuccess("multipart/x-mixed-replace;boundary=hdmiextender");
					byte[] previousImage = null;
					byte[] currentImage;
					while (!this.stopRequested)
					{
						try
						{
							currentImage = receiver.LatestImage;
							if(currentImage == previousImage)
								Thread.Sleep(1);
							else
							{
								previousImage = currentImage;

								p.outputStream.WriteLine("--hdmiextender");
								p.outputStream.WriteLine("Content-Type: image/jpeg");
								p.outputStream.WriteLine("Content-Length: " + currentImage.Length);
								p.outputStream.WriteLine();
								p.outputStream.Flush();
								p.rawOutputStream.Write(currentImage, 0, currentImage.Length);
								p.rawOutputStream.Flush();
								p.outputStream.WriteLine();
								p.outputStream.Flush();
							}
						}
						catch (Exception ex)
						{
							if (!p.isOrdinaryDisconnectException(ex))
								Logger.Debug(ex);
							break;
						}
					}

					Console.WriteLine("Ending mjpg stream");
				}
				else if (requestedPage == "audio.wav")
				{
					Console.WriteLine("Beginning audio stream");
					int? audioRegistrationId = null;
					try
					{
						ConcurrentQueue<byte[]> audioData = new ConcurrentQueue<byte[]>();
						audioRegistrationId = receiver.RegisterAudioListener(audioData);

						p.writeSuccess("audio/x-wav");
						p.outputStream.Flush();
						byte[] buffer;
						while (!this.stopRequested)
						{
							while (audioData.TryDequeue(out buffer))
								p.rawOutputStream.Write(buffer, 0, buffer.Length);
							Thread.Sleep(1);
						}
					}
					catch (Exception ex)
					{
						if (!p.isOrdinaryDisconnectException(ex))
							Logger.Debug(ex);
					}
					finally
					{
						Console.WriteLine("Ending audio stream");
						if(audioRegistrationId != null)
							receiver.UnregisterAudioListener(audioRegistrationId.Value);
					}
				}
				else if (requestedPage == "raw.html")
				{
					p.writeSuccess();
					p.outputStream.Write(@"<html>
<head>
	<title>Raw MJPEG view</title>
<style>
body
{
	background-color: Black;
}
</style>
</head>
<body>
<img src=""image.mjpg"" />
</body>
</html>");
				}
			}
			catch (Exception ex)
			{
				if (!p.isOrdinaryDisconnectException(ex))
					Logger.Debug(ex);
			}
		}

		public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
		{
			try
			{
				string requestedPage = p.request_url.AbsolutePath.TrimStart('/');

			}
			catch (Exception ex)
			{
				Logger.Debug(ex);
			}
		}

		public override void stopServer()
		{
			receiver.Stop();
		}
	}
}