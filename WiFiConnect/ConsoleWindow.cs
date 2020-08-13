using System;
using System.Runtime.InteropServices;

namespace WiFiConnect
{
    public static class ConsoleWindow
    {
        public enum WindowState
        {
            Hide = 0,
            Show = 5,
            Min = 2,
            Max = 3,
            Norm = 4,
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void SetWindowState(WindowState state)
        {
            var window = GetConsoleWindow();
            ShowWindow(window, (int)state);
        }
    }
}
