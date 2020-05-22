using System;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace WiFiConnect
{
    public static class Network
    {
        private const string _wifi_RU = "Беспроводная сеть";

        public static bool CheckConnection(string host)
        {
            Ping ping = new Ping();
            var res = ping.Send(host, 1500);

            if (res.Status != IPStatus.Success)
                return false;

            return true;
        }

        public static async Task<bool> CheckConnectionAsync(string host)
        {
            using (var ping = new Ping())
            {
                try
                {
                    var maxDelay = TimeSpan.FromMilliseconds(1500);
                    var tokenSource = new CancellationTokenSource(maxDelay);
                    PingReply reply = await Task.Run(() => ping.SendPingAsync(host), tokenSource.Token);
                    if (reply.Status != IPStatus.Success)
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public static ManagementObjectCollection GetNetworkDevices()
        {
            var wmiQuery = new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId != NULL");
            var searchProcedure = new ManagementObjectSearcher(wmiQuery);
            return searchProcedure.Get();
        }

        public static void DisableWireless()
        {
            foreach (var o in GetNetworkDevices())
            {
                var item = (ManagementObject)o;
                if (((string)item["NetConnectionId"]) == _wifi_RU)
                    item.InvokeMethod("Disable", null);
            }
        }

        public static void EnableWireless()
        {
            foreach (var o in GetNetworkDevices())
            {
                var item = (ManagementObject)o;
                if (((string)item["NetConnectionId"]) == _wifi_RU)
                    item.InvokeMethod("Enable", null);
            }
        }
    }
}
