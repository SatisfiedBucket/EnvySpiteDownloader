using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnvyAndSpiteLoader
{

    public partial class Form1 : Form
    {
        Point lastPoint;
        public Form1()
        {
            InitializeComponent();
        }

        public void StartDownload(string url, string name, string version)
        {
            progressBar1.Value = 0;
            WebClient webcl = new WebClient();
            webcl.DownloadFileCompleted += webcl_DownloadFileCompleted;
            webcl.DownloadProgressChanged += webcl_DownloadProgressChanged;
            byte[] contents = Encoding.ASCII.GetBytes(version);
            File.WriteAllBytes(Path.Combine(Properties.Settings.Default.EnvyPath + "/" + name + ".downloader"), contents);
            string path = Path.Combine(Properties.Settings.Default.EnvyPath + "/" + name + ".doomah");
            webcl.DownloadFileAsync(new Uri(url), path);
        }

        void webcl_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            label2.Text = "Download Progress: " + e.ProgressPercentage.ToString() + "%";
            progressBar1.Value = e.ProgressPercentage;
        }
        void webcl_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                this.Hide();
            }
            else
            {
                MessageBox.Show("Epic Fail:" + e.Error.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
