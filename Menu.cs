using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace EnvyAndSpiteLoader
{
    public partial class Menu : Form
    {
        Point lastPoint;

        public class Parameter
        {
            public string Name { get; set; }
            public string Author { get; set; }
            public string Img { get; set; }
            public string Url { get; set; }
            public string Id { get; set; }
            public string MoreInfo { get; set; }
            public string Version { get; set; }
        }

        private List<Parameter> parameters;

        private void OpenURL(string url)
        {
            string key = @"htmlfile\shell\open\command";
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(key, false);
            string Default_Browser_Path = ((string)registryKey.GetValue(null, null)).Split('"')[1];

            Process p = new Process();
            p.StartInfo.FileName = Default_Browser_Path;
            p.StartInfo.Arguments = url;
            p.Start();
        }

        private static string RetrieveData(string url)
        {

            // used to build entire input
            var sb = new StringBuilder();

            // used on each read operation
            var buf = new byte[8192];
            try
            {
                // prepare the web page we will be asking for
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                var request = (HttpWebRequest)
                WebRequest.Create(url);
#pragma warning restore SYSLIB0014 // Type or member is obsolete

                /* Using the proxy class to access the site
                 * Uri proxyURI = new Uri("http://proxy.com:80");
                 request.Proxy = new WebProxy(proxyURI);
                 request.Proxy.Credentials = new NetworkCredential("proxyuser", "proxypassword");*/

                // execute the request
                var response = (HttpWebResponse)
                                           request.GetResponse();

                // we will read data via the response stream
                Stream resStream = response.GetResponseStream();

                string? tempString = null;
                int count = 0;

                do
                {
                    // fill the buffer with data
                    count = resStream.Read(buf, 0, buf.Length);

                    // make sure we read some data
                    if (count != 0)
                    {
                        // translate from bytes to ASCII text
                        tempString = Encoding.ASCII.GetString(buf, 0, count);

                        // continue building the string
                        sb.Append(tempString);
                    }
                } while (count > 0); // any more data to read?

            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Failed to retrieve data from the network. Please check you internet connection: " +
                                exception);
            }
            return sb.ToString();
        }

        public Menu()
        {
            InitializeComponent();
            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            button1.Visible = false;
            button2.Visible = false;
            button4.Visible = false;
            pictureBox1.Visible = false;
            if (File.ReadAllText("./levels.json") != RetrieveData("https://raw.githubusercontent.com/SatisfiedBucket/EnvySpiteDownloader/main/levels.json"))
            {
                byte[] contents = Encoding.ASCII.GetBytes(RetrieveData("https://raw.githubusercontent.com/SatisfiedBucket/EnvySpiteDownloader/main/levels.json"));
                File.WriteAllBytes("./levels.json", contents);
                MessageBox.Show("Updated level list!");
            }
            LoadParameters();
            Process[] pname = Process.GetProcessesByName("ultrakill");
            if (pname.Length != 0)
                MessageBox.Show("ULTRAKILL is still running! Please close it before using this!");
        }

        private void LoadParameters()
        {
            // Read the JSON file
            string json = File.ReadAllText("./levels.json");
            parameters = JsonConvert.DeserializeObject<List<Parameter>>(json);

            // Populate the ListBox with the names of the parameters
            foreach (var parameter in parameters)
            {
                listBox1.Items.Add(parameter.Name);
            }
            if (Properties.Settings.Default.EnvyPath != "")
            {
                foreach (var parameter in parameters)
                {
                    if (File.Exists(Path.Combine(Properties.Settings.Default.EnvyPath + "/" + parameter.Name + ".downloader")) == true)
                    {
                        if (parameter.Version != File.ReadAllText(Path.Combine(Properties.Settings.Default.EnvyPath + "/" + parameter.Name + ".downloader")))
                        {
                            MessageBox.Show("There is a new version for " + parameter.Name + ". Downloading now!");
                            Form1 Form = new Form1();
                            Form.Show();
                            Form.StartDownload(parameter.Url, parameter.Name, parameter.Version);
                        }
                    }
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                label1.Visible = true;
                label2.Visible = true;
                label3.Visible = true;
                button1.Visible = true;
                button2.Visible = true;
                button4.Visible = true;
                pictureBox1.Visible = true;

                string selectedName = listBox1.SelectedItem.ToString();
                var selectedParameter = parameters.Find(p => p.Name == selectedName);

                if (selectedParameter != null)
                {
                    label1.Text = $"{selectedParameter.Name}";
                    label2.Text = $"{selectedParameter.Author}";
                    label3.Text = $"{selectedParameter.Version}";
                    pictureBox1.Load($"{selectedParameter.Img}");
                }
            }
            if (listBox1.SelectedItem == null)
            {
                label1.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
                button1.Visible = false;
                button2.Visible = false;
                button4.Visible = false;
                pictureBox1.Visible = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedName = listBox1.SelectedItem.ToString();
                var selectedParameter = parameters.Find(p => p.Name == selectedName);
                if (selectedParameter != null)
                {
                    System.Windows.Forms.Clipboard.SetText($"{selectedParameter.Id}");
                    MessageBox.Show("ID copied to clipboard.");
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void listBox1_TabIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.EnvyPath == "")
            {
                MessageBox.Show("You haven't selected your Envy installation path yet.");
                MessageBox.Show("IT IS VERY IMPORTANT THAT YOU SELECT THE RIGHT PATH HERE. Otherwise nothing will work.");
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        Properties.Settings.Default.EnvyPath = fbd.SelectedPath;
                        Properties.Settings.Default.Save();
                    }
                }
            }
            if (listBox1.SelectedItem != null)
            {
                string selectedName = listBox1.SelectedItem.ToString();
                var selectedParameter = parameters.Find(p => p.Name == selectedName);
                if (selectedParameter != null)
                {
                    if (File.Exists(Path.Combine(Properties.Settings.Default.EnvyPath + "/" + label1.Text + ".doomah")) == true)
                    {
                        MessageBox.Show("This level is already downloaded and will be overwritten.");
                    }
                    Form1 Form = new Form1();
                    Form.Show();
                    Form.StartDownload($"{selectedParameter.Url}", label1.Text, label3.Text);
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedName = listBox1.SelectedItem.ToString();
                var selectedParameter = parameters.Find(p => p.Name == selectedName);
                if (selectedParameter != null)
                {
                    MessageBox.Show($"{selectedParameter.MoreInfo}");
                }
            }
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("IT IS VERY IMPORTANT THAT YOU SELECT THE RIGHT PATH HERE. Otherwise nothing will work.");
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Properties.Settings.Default.EnvyPath = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenURL("https://github.com/SatisfiedBucket/EnvySpiteDownloader/tree/main");
        }
    }
}
