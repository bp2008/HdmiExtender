using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HdmiExtenderLib
{
	public partial class MainForm : Form
	{
		System.Windows.Forms.Timer timer;
		HdmiExtenderReceiver receiver;

		byte[] previousImage;
		byte[] currentImage;
		int fpsCalc = 0;
		DateTime nextFpsSecond = DateTime.Now;

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			timer = new System.Windows.Forms.Timer();
			timer.Interval = 1;
			timer.Tick += timer_Tick;
			timer.Start();

			receiver = new HdmiExtenderReceiver("192.168.168.55", 1);
			receiver.Start();
		}

		void timer_Tick(object sender, EventArgs e)
		{
			currentImage = receiver.LatestImage;
			if(currentImage != previousImage)
			{
				fpsCalc++;
				previousImage = currentImage;
				if (pb.Image != null)
					pb.Image.Dispose();
				MemoryStream ms = new MemoryStream(currentImage);
				Image img = Bitmap.FromStream(ms);
				lblVideoData.Text = img.Width + "x" + img.Height;
				pb.Image = img;
			}
			if (nextFpsSecond < DateTime.Now)
			{
				lblFrameRate.Text = fpsCalc.ToString();
				fpsCalc = 0;
				nextFpsSecond = DateTime.Now.AddSeconds(1);
			}
			// Unfortunately, I could not get audio to play using the NAudio library, so this test application only plays back video.
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			receiver.Stop();
		}
	}
}
