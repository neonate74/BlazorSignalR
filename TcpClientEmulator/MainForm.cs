using System.Net.NetworkInformation;
using System.Net.Sockets;
using TcpClientEmulator.Network;

namespace TcpClientEmulator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            this.Icon = Resource.small;
            this.Text = "Tcp Emulator";

            this.trayIcon.Visible = true;

            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
            this.Disposed += MainForm_Disposed;
        }

        private void MainForm_Disposed(object? sender, EventArgs e)
        {
            try
            {
                this.trayIcon.Dispose();

                Program.WebServerProcessKill();
            }
            catch { }
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            this.Visible = false;

            Thread th = new Thread(() =>
            {
                for (int i = Program.portRange[0]; i <= Program.portRange[1]; i++)
                {
                    if (CheckAvailableTcpPort(i))
                        break;
                }
            });
            th.Start();
        }

        private bool CheckAvailableTcpPort(int port)
        {
            bool isAvailable = true;
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }

            if (isAvailable)
            {
                Program.MainPort = port;
                TcpClientManager tcp = new TcpClientManager(port);
            }

            return isAvailable;
        }

        #region ShowMessage
        delegate void ctrlInvoke(string message);

        public void ShowMessage(string message)
        {
            if (this.txtLog.InvokeRequired)
            {
                this.txtLog.Invoke(new ctrlInvoke(ShowMessage), message);
            }
            else
            {
                this.txtLog.Text += "\r\n" + message;
            }
        }
        #endregion

        #region Basic Event

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true;

            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            this.Show();
            this.Activate();
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true;

            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            this.Show();
            this.Activate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.trayIcon.Visible = false;
            this.Dispose();
        }

        private void exitMainMenuItem_Click(object sender, EventArgs e)
        {
            this.trayIcon.Visible = false;
            this.Dispose();
        }
        #endregion
    }
}