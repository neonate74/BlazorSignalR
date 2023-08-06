using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace TcpClientEmulator
{
    internal static class Program
    {
        static Mutex? _mutex;
        public static bool IsRedundant = false;

        private static string WebSrvName = "BlazorApp.Server";

        public static MainForm? MainForm { get; set; }

        public static int MainPort { get; set; }
        public static int[] portRange { get; set; }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexName = "TcpClientEmulator";//AppDomain.CurrentDomain.FriendlyName;
            bool isCreatedNew = false;

            _mutex = new Mutex(true, mutexName, out isCreatedNew);
            if (!isCreatedNew)
            {
                IsRedundant = true;
                //MessageBox.Show("Application already started.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Application.Exit();
            }

            string strPortRange = ConfigurationManager.AppSettings.Get("PortRange") ?? "";
            if (strPortRange == "")
                MessageBox.Show("There is no definition for PortRange in App.config.\r\nPlease check it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                string[] temp = strPortRange.Split(',');
                portRange = new int[2];
                int.TryParse(temp[0], out portRange[0]);
                int.TryParse(temp[1], out portRange[1]);
            }

            WebServerProcessKill();

            WebServerProcessStart();

            ApplicationConfiguration.Initialize();

            Application.ThreadException += Application_ThreadException;
            Application.ApplicationExit += Application_ApplicationExit;

            MainForm = new MainForm();
            MainForm.WindowState = FormWindowState.Minimized;

            Application.Run(MainForm);

            Application.Exit();
        }

        public static void WebServerProcessKill()
        {
            Process[] processlist = Process.GetProcesses();
            foreach (Process theprocess in processlist)
            {
                // Console.WriteLine("Process: {0} ID: {1}", theprocess.ProcessName, theprocess.Id);
                if (theprocess.ProcessName.Contains(WebSrvName))
                    theprocess.Kill();
            }
        }

        private static void WebServerProcessStart()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = $"C:\\Users\\kbhan\\source\\repos\\BlazorApp\\publish\\{WebSrvName}.exe";
            startInfo.WorkingDirectory = "C:\\Users\\kbhan\\source\\repos\\BlazorApp\\publish\\";
            startInfo.Arguments = "";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process processTemp = new Process();
            processTemp.StartInfo = startInfo;
            processTemp.EnableRaisingEvents = true;
            try
            {
                processTemp.Start();
            }
            catch (Exception e)
            {
                throw;
            }
            //processTemp.WaitForExit();
        }

        private static void Application_ApplicationExit(object? sender, EventArgs e)
        {
            _mutex?.Dispose();
            Environment.Exit(0);
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"[Application_ThreadException]{e.Exception.Message}:{e.Exception.StackTrace}");
        }
    }
}