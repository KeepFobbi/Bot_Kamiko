using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net;
using System.IO;
using SimpleJSON;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Specialized;

namespace ip
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        WebClient webClient = new WebClient();
        DateTime StartSession;
        string Token = "541751696:AAHp-XMrbGqnuTdad1Hw4WbhnePDaWKI04s";
        bool flag = false;
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);

        private void Mute()
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
                (IntPtr)APPCOMMAND_VOLUME_MUTE);
        }

        private void VolDown()
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
                (IntPtr)APPCOMMAND_VOLUME_DOWN);
        }

        private void VolUp()
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
                (IntPtr)APPCOMMAND_VOLUME_UP);
        }

        public void SendMessage(string Message)
        {
            webClient.DownloadString("https://api.telegram.org/bot" + Token + "/sendMessage?chat_id=349870760&text=" + Message);
        }

        //https://api.telegram.org/bot541751696:AAHp-XMrbGqnuTdad1Hw4WbhnePDaWKI04s/getUpdates

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Visible = false;

                StartSession = DateTime.Now;

                Stream data = webClient.OpenRead("http://ip-address.ru/");
                StreamReader reader = new StreamReader(data);
                Regex regex = new Regex(@"([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})");
                MatchCollection match = regex.Matches(reader.ReadToEnd());

                SendMessage($" Мой Господин, ваш компьютер только что был запущен.\n " +
                    $"Ваш IP:  {match[0].Value}\n " +
                    $"Напоминаю:  3389\n" +
                    $"    Хорошего дня  =)\n\n" +
                    $"Сегодня:   {DateTime.Now.ToLongDateString()}");


                notifyIcon1.BalloonTipText = $"Ваш IP:  {match[0].Value}";
                notifyIcon1.ShowBalloonTip(1000);

                timer1.Start();

                //Application.Exit();

                response = webClient.DownloadString("https://api.telegram.org/bot" + Token + "/getUpdates?offset=" + (LastUpdateID));
                if (response.Length > 23)
                {
                    var N = JSON.Parse(response);
                    foreach (JSONNode r in N["result"].AsArray)
                    {
                        LastUpdateID = r["update_id"].AsInt + 1;
                        string UserMess = r["message"]["text"];
                    }
                }
            }
            catch(Exception er)
            {
                MessageBox.Show(er.Message);
            }
        }
        #region EndApp
        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            if (flag == false)
                Visible = false;
            richTextBox1.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            TimeSpan span = new TimeSpan();
            span = DateTime.Now - StartSession;
            string str;

            if (span.Minutes <= 1)
                str = $"{span.Seconds} c.";
            else if (span.Hours <= 1)
                str = $"{span.Minutes} м. {span.Seconds} c.";
            else if (span.Days <= 1)
                str = $"{span.Hours} ч. {span.Minutes} м. {span.Seconds} c.";
            else
                str = $"{span.Days} д. {span.Hours} ч. {span.Minutes} м. {span.Seconds} c.";


            webClient.OpenRead($"https://api.telegram.org/bot541751696:AAHp-XMrbGqnuTdad1Hw4WbhnePDaWKI04s/sendMessage?chat_id=349870760&text=" +
                $" Мой Господин, ваш компьютер только что был выключен.\n " +
                $"Ваш компьютер был в сети:   {str}");
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flag = true;
            Visible = true;
            SendMessage("Чат включен.");
        }
        #endregion

        int LastUpdateID = 0;
        string response;

        public static void HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            Console.WriteLine(string.Format("Uploading {0} to {1}", file, url));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                Console.WriteLine(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            response = webClient.DownloadString("https://api.telegram.org/bot" + Token + "/getUpdates?offset=" + (LastUpdateID));
            if (response.Length > 23)
            {
                var N = JSON.Parse(response);
                foreach (JSONNode r in N["result"].AsArray)
                {
                    LastUpdateID = r["update_id"].AsInt + 1;
                    string UserMess = r["message"]["text"];
                    int ID = r["message"]["from"]["id"].AsInt;

                    if (ID == 349870760)
                    {
                        if (UserMess == "/chatoff")
                        {
                            flag = false;
                            Visible = false;
                            SendMessage("Чат выключен.");
                        }
                        else if (UserMess == "/mute")
                        {
                            Mute();
                        }
                        else if (UserMess == "/screen")
                        {
                            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

                            Graphics graphics = Graphics.FromImage(printscreen as Image);

                            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);

                            printscreen.Save(@"D:\ip\Screen\printscreen.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                            NameValueCollection nvc = new NameValueCollection();
                            nvc.Add("chat_id", ID.ToString());
                            
                            HttpUploadFile("https://api.telegram.org/bot" + Token + "/sendPhoto",
                                 @"D:\ip\Screen\printscreen.jpg", "photo", "image/jpeg", nvc);

                        }
                        else if (UserMess == "/volup")
                        {
                            VolUp();
                        }
                        else if (UserMess == "/voldown")
                        {
                            VolDown();
                        }
                        else if (UserMess == "/off")
                        {
                            System.Diagnostics.Process process = new System.Diagnostics.Process();
                            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            startInfo.FileName = "powershell.exe";
                            startInfo.Arguments = "shutdown -s -t 00";
                            process.StartInfo = startInfo;
                            process.Start();
                        }
                        else if (UserMess == "/restart")
                        {
                            System.Diagnostics.Process process = new System.Diagnostics.Process();
                            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            startInfo.FileName = "powershell.exe";
                            startInfo.Arguments = "shutdown -r -t 00";
                            process.StartInfo = startInfo;
                            process.Start();
                        }
                        else if (UserMess == "/chat")
                        {
                            flag = true;
                            Visible = true;
                            SendMessage("Чат включен.");
                            richTextBox2.Focus();
                        }
                        else if (Visible == true)
                        {
                            richTextBox1.Text += "Client:\n\t" + UserMess + "\n";
                        }
                        else if (UserMess == "hi" || UserMess == "Hi" || UserMess == "Hello" && flag == false)
                            SendMessage("Hi");
                        else
                        {
                            MessageBox.Show(UserMess);
                        }
                    }
                }
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendMessage(richTextBox2.Text);
            richTextBox1.Text += "User:\n\t" + richTextBox2.Text + "\n";
            richTextBox2.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            flag = false;
            Visible = false;
            SendMessage("Чат выключен.");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            flag = false;
            Visible = false;
            notifyIcon1.BalloonTipText = $"Окно свернуто.\nНажмите на иконку два раза чтобы розвернуть.";
            notifyIcon1.ShowBalloonTip(1000);
            HideFlag = true;
        }

        bool HideFlag = false;

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (HideFlag == true)
            {
                flag = true;
                Visible = true;
                HideFlag = false;
            }
        }

        private Point mouseOffset;
        private bool isMouseDown = false;

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            int xOffset;
            int yOffset;

            if (e.Button == MouseButtons.Left)
            {
                xOffset = -e.X - SystemInformation.FrameBorderSize.Width;
                yOffset = -e.Y - SystemInformation.CaptionHeight -
                    SystemInformation.FrameBorderSize.Height;
                mouseOffset = new Point(xOffset, yOffset);
                isMouseDown = true;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                Location = mousePos;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }
    }
}
