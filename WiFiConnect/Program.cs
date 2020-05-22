using SimpleWifi;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace WiFiConnect
{
    class Program
    {
        static CyclicList<string> hosts = new CyclicList<string>
            {
                "ya.ru",
                "yandex.ru",
                "rambler.ru",
                "google.com",
                "bing.com",
                "yahoo.com",
            };

        static SimpleWifiFacade wifi = new SimpleWifiFacade();
        static AccessPoint accessPoint;

        static async Task Main(string[] args)
        {
            Console.Write("Enter the delay to verify the connection (in seconds): ");
            int checkConnectionDelay = int.Parse(Console.ReadLine()) * 1000;
            accessPoint = wifi.SelectAccessPoint();

            Console.WriteLine("Sync (y/n)? Else Async");
            if (Console.ReadLine().ToLower() == "y")
            {
                Console.WriteLine("Started sync");
                Start(checkConnectionDelay);
            }
            else
            {
                Console.WriteLine("Started async");
                await StartAsync(checkConnectionDelay);
            }
        }

        private static void Start(int checkConnectionDelay)
        {
            string currentHost = string.Empty;
            TimeSpan delay = TimeSpan.FromMilliseconds(checkConnectionDelay);
            while (true)
            {
                try
                {
                    Retry.Do(() => 
                    { 
                        currentHost = hosts.GetNext();
                        if (!Network.CheckConnection(currentHost))
                            Thread.Sleep(delay);
                        else
                            throw new PingException($"[{DateTime.Now}] Ping is not success to {currentHost}");
                    }, delay);

                    Thread.Sleep(delay);
                }
                catch (AggregateException ex)
                {
                    foreach (var e in ex.InnerExceptions)
                    {
                        Console.WriteLine(
                            $"{e.Message}" +
                            $"\n{e.GetType()}");
                    }

                    Console.WriteLine("[Reconnect]");
                    //ReenableWireless();
                    wifi.Connect(accessPoint);
                }
            }
        }

        private static async Task StartAsync(int checkConnectionDelay)
        {
            string currentHost = string.Empty;
            TimeSpan delay = TimeSpan.FromMilliseconds(checkConnectionDelay);
            while (true)
            {
                try
                {
                    await AsyncRetry.Do(async () =>
                    {
                        currentHost = hosts.GetNext();

                        if (await Network.CheckConnectionAsync(currentHost))
                            await Task.Delay(delay);
                        else
                            throw new PingException($"[{DateTime.Now}] Ping is not success to {currentHost}");

                    }, TimeSpan.FromMilliseconds(1500));
                }
                catch (AggregateException ex)
                {
                    foreach (var e in ex.InnerExceptions)
                    {
                        Console.WriteLine(
                            $"{e.Message}" +
                            $"\n{e.GetType()}");
                    }

                    Console.WriteLine("[Reconnect]");
                    //ReenableWireless();
                    wifi.Connect(accessPoint);
                }
            }
        }

        private static void ReenableWireless()
        {
            Network.DisableWireless();
            Thread.Sleep(300);
            Network.EnableWireless();
            Thread.Sleep(5000);
        }
    }
}
