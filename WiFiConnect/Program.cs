using SimpleWifi;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiFiConnect
{
    class Program
    {
        static SimpleWifiFacade wifi = new SimpleWifiFacade();
        static AccessPoint accessPoint;
        static string host = "192.168.0.1";
        static NotifyIcon notifyIcon = new NotifyIcon();

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const int SW_Min = 2;
        const int SW_Max = 3;
        const int SW_Norm = 4;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main(string[] args)
        {
            accessPoint = wifi.GetKnownAccessPoint();
            Console.WriteLine($"Connect to {accessPoint.Name}");
            Worker worker = new Worker(async () => await CheckConnection());
            worker.Start();
            Console.WriteLine("Worker Started");
            InitializeNotifyIcon();

            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            Application.Run();
        }

        private static void InitializeNotifyIcon()
        {
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += TrayIcon_DoubleClick;
        }

        private static void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_SHOW);
        }

        private static async Task CheckConnection()
        {
            DateTime start = DateTime.Now;
            if (!await Network.CheckConnectionWithRetryAsync(host, 300))
            {
                wifi.Connect(accessPoint);
                notifyIcon.ShowBalloonTip(3000, "Reconnected", $"{DateTime.Now - start}", ToolTipIcon.Warning);
            }
        }
    }
}
