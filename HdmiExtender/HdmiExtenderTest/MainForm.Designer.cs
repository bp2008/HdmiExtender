namespace HdmiExtenderLib
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pb = new System.Windows.Forms.PictureBox();
			this.lblVideoData = new System.Windows.Forms.Label();
			this.lblFrameRate = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pb)).BeginInit();
			this.SuspendLayout();
			// 
			// pb
			// 
			this.pb.Location = new System.Drawing.Point(12, 44);
			this.pb.Name = "pb";
			this.pb.Size = new System.Drawing.Size(640, 360);
			this.pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pb.TabIndex = 0;
			this.pb.TabStop = false;
			// 
			// lblVideoData
			// 
			this.lblVideoData.AutoSize = true;
			this.lblVideoData.Location = new System.Drawing.Point(12, 9);
			this.lblVideoData.Name = "lblVideoData";
			this.lblVideoData.Size = new System.Drawing.Size(24, 13);
			this.lblVideoData.TabIndex = 2;
			this.lblVideoData.Text = "0x0";
			// 
			// lblFrameRate
			// 
			this.lblFrameRate.AutoSize = true;
			this.lblFrameRate.Location = new System.Drawing.Point(158, 9);
			this.lblFrameRate.Name = "lblFrameRate";
			this.lblFrameRate.Size = new System.Drawing.Size(13, 13);
			this.lblFrameRate.TabIndex = 3;
			this.lblFrameRate.Text = "0";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(122, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(30, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "FPS:";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(664, 412);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblFrameRate);
			this.Controls.Add(this.lblVideoData);
			this.Controls.Add(this.pb);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "MainForm";
			this.Text = "HDMI Extender Test";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.pb)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pb;
		private System.Windows.Forms.Label lblVideoData;
		private System.Windows.Forms.Label lblFrameRate;
		private System.Windows.Forms.Label label1;
	}
}

