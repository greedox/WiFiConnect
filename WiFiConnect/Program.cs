using Microsoft.Win32;
using SimpleWifi;
using System;
using System.Drawing;
using System.Linq;
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
        static Worker worker;
        
        static void Main(string[] args)
        {
            InitializeNotifyIcon();
            InitializeWorker();

            ConsoleWindow.SetWindowState(ConsoleWindow.WindowState.Hide);

            Application.Run();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        private static void InitializeWorker()
        {
            accessPoint = wifi.GetKnownAccessPoint();
            notifyIcon.ShowBalloonTip(3000, "", $"Connect to {accessPoint.Name}", ToolTipIcon.Info);
            worker = new Worker(async () => await Reconnect());
            worker.Start();
        }

        private static void InitializeNotifyIcon()
        {
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.Visible = true;
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] 
            {
                new MenuItem("Stop Worker", (s, e) => worker.Stop()),
                new MenuItem("Refresh Access Points"),
                new MenuItem("Select Access Point",
                             wifi.GetAccessPoints().Select(x => new MenuItem(x.Name, (s, e) => accessPoint = x)).ToArray()),
                new MenuItem("Autorun", new MenuItem[]
                {
                    new MenuItem("On", (s,e) => SetAutorun(true)),
                    new MenuItem("Off", (s,e) => SetAutorun(false)),
                }),
                new MenuItem("Exit", (s,e) => Application.Exit()),
            });
        }

        private static async Task Reconnect()
        {
            DateTime start = DateTime.Now;
            if (!await Network.CheckConnectionWithRetryAsync(host, 300))
            {
                wifi.Connect(accessPoint);
                await Task.Delay(1000);
                notifyIcon.ShowBalloonTip(3000, "Reconnected", $"Spent {(DateTime.Now - start).TotalSeconds} seconds", ToolTipIcon.Warning);
            }
        }

        private static void SetAutorun(bool autorun)
        {
            const string pathRegistryKeyStartup = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            using (RegistryKey registryKeyStartup = Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true))
            {
                try
                {
                    if (autorun)
                    {
                        registryKeyStartup.SetValue(Application.ProductName, $"\"{Application.ExecutablePath}\"");
                    }
                    else
                    {
                        registryKeyStartup.DeleteValue(Application.ProductName, false);
                    }
                }
                catch (Exception ex)
                {
                    notifyIcon.ShowBalloonTip(3000, "Autorun", ex.Message, ToolTipIcon.Error);
                }
            }
        }
    }
}
