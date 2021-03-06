﻿using SimpleWifi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WiFiConnect
{
    class SimpleWifiFacade
    {
        private Wifi _wifi;
        public SimpleWifiFacade()
        {
            _wifi = new Wifi();
        }

        public AccessPoint SelectAccessPoint()
        {
            var accessPoints = _wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength).ToArray();
            
            int i = 0;
            foreach (AccessPoint ap in accessPoints)
                Console.WriteLine("{0}. {1} {2}% Connected: {3}", i++, ap.Name, ap.SignalStrength, ap.IsConnected);

            Console.Write("\r\nEnter the index of the network you wish to connect to: ");

            int selectedIndex = int.Parse(Console.ReadLine());
            
            return accessPoints[selectedIndex];
        }

        public AccessPoint GetKnownAccessPoint()
        {
            var accesPoints = GetAccessPoints().OrderBy(x => x.SignalStrength).ToList();

            var connectedAP = accesPoints.FirstOrDefault(x => x.IsConnected);
            if (connectedAP != null)
                return connectedAP;
            else
                return accesPoints.FirstOrDefault(x => x.HasProfile);
        }

        public IEnumerable<AccessPoint> GetAccessPoints()
        {
            return _wifi.GetAccessPoints();
        }

        public bool Connect(AccessPoint accessPoint, bool overwrite = false)
        {
            // Auth
            AuthRequest authRequest = new AuthRequest(accessPoint);
            //bool overwrite = true;

            if (authRequest.IsPasswordRequired)
            {
                //if (selectedAP.HasProfile)
                //// If there already is a stored profile for the network, we can either use it or overwrite it with a new password.
                //{
                //    Console.Write("\r\nA network profile already exist, do you want to use it (y/n)? ");
                //    if (Console.ReadLine().ToLower() == "y")
                //    {
                //        overwrite = false;
                //    }
                //}

                if (overwrite)
                {
                    if (authRequest.IsUsernameRequired)
                    {
                        Console.Write("\r\nPlease enter a username: ");
                        authRequest.Username = Console.ReadLine();
                    }

                    authRequest.Password = PasswordPrompt(accessPoint);

                    if (authRequest.IsDomainSupported)
                    {
                        Console.Write("\r\nPlease enter a domain: ");
                        authRequest.Domain = Console.ReadLine();
                    }
                }
            }

            //selectedAP.ConnectAsync(authRequest, overwrite, OnConnectedComplete);
            bool connected = accessPoint.Connect(authRequest, overwrite);
            if (connected)
            {
                Console.WriteLine("Connection succefuly");
            }
            else
            {
                Console.WriteLine("Connection failed");
            }
            return connected;
        }

        public string PasswordPrompt(AccessPoint selectedAP)
        {
            string password = string.Empty;

            bool validPassFormat = false;

            while (!validPassFormat)
            {
                Console.Write("\r\nPlease enter the wifi password: ");
                password = Console.ReadLine();

                validPassFormat = selectedAP.IsValidPassword(password);

                if (!validPassFormat)
                    Console.Write("\r\nPassword is not valid for this network type.");
            }

            return password;
        }
    }
}
