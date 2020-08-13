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

        public static async Task<bool> CheckConnectionWithRetryAsync(string host, int pingTimeout, int retryDelay)
        {
            try
            {
                return await AsyncRetry.Do(async () =>
                {
                    if (await CheckConnectionAsync(host, pingTimeout))
                        return true;
                    else
                        throw new Exception($"[{ DateTime.Now }] Ping is not success to {host}");
                }, TimeSpan.FromMilliseconds(retryDelay));
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine(
                        $"{e.Message}" +
                        $"\n{e.GetType()}");
                }
                return false;
            }
        }

        public static async Task<bool> CheckConnectionAsync(string host, int pingTimeout)
        {
            using (var ping = new Ping())
            {
                try
                {
                    var maxDelay = TimeSpan.FromMilliseconds(pingTimeout + 200);
                    var tokenSource = new CancellationTokenSource(maxDelay);
                    PingReply reply = await Task.Run(() => ping.SendPingAsync(host, pingTimeout), tokenSource.Token);
                    if (reply.Status != IPStatus.Success)
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception)
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

        public static void ReenableWireless()
        {
            DisableWireless();
            Thread.Sleep(300);
            EnableWireless();
            Thread.Sleep(5000);
        }
    }
}
