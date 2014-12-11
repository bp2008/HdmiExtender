using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using HdmiExtenderLib;

namespace HdmiExtenderService
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			// If the user starts this program with the argument "cmd", we will run as a console application.  Otherwise we will run as a Windows Service.
			if (args.Length == 1 && args[0] == "cmd")
			{
				ushort port = 18080;
				MainService svc = new MainService();
				VideoWebServer server = new VideoWebServer(port, -1, "192.168.168.55", 1);
				server.Start();
				Console.WriteLine("This service was run with the command line argument \"cmd\".");
				Console.WriteLine("When run without arguments, this application acts as a Windows Service.");
				Console.WriteLine();
				Console.WriteLine("Jpeg still image:");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("\thttp://localhost:" + port + "/image.jpg");
				Console.ResetColor();
				Console.WriteLine();
				Console.WriteLine("Motion JPEG:");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("\thttp://localhost:" + port + "/image.mjpg");
				Console.ResetColor();
				Console.WriteLine();
				Console.WriteLine("PCM 48kHz, Signed 32 bit, Big Endian");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("\thttp://localhost:" + port + "/audio.wav");
				Console.ResetColor();
				Console.WriteLine();
				Console.Write("When you see ");
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("netdrop1");
				Console.ResetColor();
				Console.Write(" or ");
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("netdrop2");
				Console.ResetColor();
				Console.WriteLine(" in the console, this means a frame was dropped due to data loss between the Sender device and this program.");
				Console.WriteLine();
				Console.WriteLine("Http server running on port " + port + ". Press ENTER to exit.");
				Console.ReadLine();
				Console.WriteLine("Shutting down...");
				server.Stop();
			}
			else
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] 
				{ 
					new MainService() 
				};
				ServiceBase.Run(ServicesToRun);
			}
		}
	}
}
