using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using HdmiExtenderLib;

namespace HdmiExtenderService
{
	public partial class MainService : ServiceBase
	{
		VideoWebServer server;

		public MainService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			server = new VideoWebServer(18080, -1, "192.168.168.55", 1);
			server.Start();
		}

		protected override void OnStop()
		{
			server.Stop();
		}
	}
}
