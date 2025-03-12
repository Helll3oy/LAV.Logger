using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace LAV.Logger
{
    public static class Colorize
    {
        private const string ColorEnd = "\x1b[0m";

        public static string DarkGray(string text) => "\x1b[90m" + text + ColorEnd;
        public static string Red(string text) => "\x1b[91m" + text + ColorEnd;
        public static string Green(string text) => "\x1b[92m" + text + ColorEnd;
        public static string Yellow(string text) => "\x1b[93m" + text + ColorEnd;
        public static string Blue(string text) => "\x1b[94m" + text + ColorEnd;
        public static string Magenta(string text) => "\x1b[95m" + text + ColorEnd;
        public static string Cyan(string text) => "\x1b[96m" + text + ColorEnd;
        public static string White(string text) => "\x1b[97m" + text + ColorEnd;

        public static string Black(string text) => "\x1b[30m" + text + ColorEnd;
        public static string DarkRed(string text) => "\x1b[31m" + text + ColorEnd;
        public static string DarkGreen(string text) => "\x1b[32m" + text + ColorEnd;
        //public static string DarkYellow(string text) => "\x1b[33m" + text + ColorEnd;
        public static string DarkYellow(string text) => "\x1b[38;2;193;156;0m" + text + ColorEnd;
        public static string DarkBlue(string text) => "\x1b[34m" + text + ColorEnd;
        //public static string DarkMagenta(string text) => "\x1b[35m" + text + ColorEnd;
        public static string DarkMagenta(string text) => "\x1b[38;2;136;23;152m" + text + ColorEnd;
        public static string DarkCyan(string text) => "\x1b[36m" + text + ColorEnd;
        public static string Gray(string text) => "\x1b[37m" + text + ColorEnd;
    }

   internal static class AnsiCodeEnabler
    {
        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        public static void Enable()
        {
            const int STD_OUTPUT_HANDLE = -11;
            const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
            const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
                SetConsoleMode(iStdOut, outConsoleMode);
            }
        }
    }
}
