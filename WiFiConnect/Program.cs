using Microsoft.Win32;
using SimpleWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiFiConnect
{
    class Program
    {
        static SimpleWifiFacade wifi = new SimpleWifiFacade();
        static AccessPoint accessPoint;
        static string host = string.Empty;
        static NotifyIcon notifyIcon = new NotifyIcon();
        static Worker worker;
        
        static void Main(string[] args)
        {
            ConsoleWindow.SetWindowState(ConsoleWindow.WindowState.Hide);
            Application.ApplicationExit += Application_ApplicationExit;
            host = GetWifiGatewayIP().FirstOrDefault();
            InitializeNotifyIcon();
            InitializeWorker();
            Application.Run();
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            worker.Stop();
        }

        private static List<string> GetWifiGatewayIP()
        {
            var adapters = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 
                            && x.OperationalStatus == OperationalStatus.Up);

            var ips = new List<string>();


            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                GatewayIPAddressInformationCollection addresses = adapterProperties.GatewayAddresses;
                if (addresses.Count > 0)
                {
                    Console.WriteLine(adapter.Description);
                    foreach (GatewayIPAddressInformation address in addresses)
                    {
                        ips.Add(address.Address.ToString());
                    }
                }
            }

            return ips;
        }

        private static void InitializeWorker()
        {
            accessPoint = wifi.GetKnownAccessPoint();
            notifyIcon.ShowBalloonTip(3000, "", $"Connect to {accessPoint.Name} \n{host}", ToolTipIcon.Info);
            worker = new Worker(async () => await Reconnect());
            worker.Start();
        }

        private static void InitializeNotifyIcon()
        {
            notifyIcon.Icon = Resource.icon_32;
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
            if (!await Network.CheckConnectionWithRetryAsync(host, 1000, 300))
            {
                wifi.Connect(accessPoint);
                await Task.Delay(1500);
                notifyIcon.ShowBalloonTip(1000, "Reconnected", $"Spent {(DateTime.Now - start).TotalSeconds} seconds", ToolTipIcon.Warning);
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
                        registryKeyStartup.SetValue(Application.ProductName, $"\"{Application.ExecutablePath}\"");
                    else
                        registryKeyStartup.DeleteValue(Application.ProductName, false);
                }
                catch (Exception ex)
                {
                    notifyIcon.ShowBalloonTip(3000, "Autorun", ex.Message, ToolTipIcon.Error);
                }
            }
        }
    }
}
