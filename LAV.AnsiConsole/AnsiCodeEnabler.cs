using System;
using System.Runtime.InteropServices;

namespace LAV.AnsiConsole
{
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
            const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

            if (
#if NET8_0_OR_GREATER
                !OperatingSystem.IsWindows()
#elif NETSTANDARD1_3 || NETSTANDARD2_0_OR_GREATER || NET471_OR_GREATER || NETCOREAPP2_0_OR_GREATER
                !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
#else
                Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix
                || Environment.OSVersion.Platform == PlatformID.Xbox
#endif
            )
                return;

            //var stdout = Console.OpenStandardOutput();
            //var handle = Microsoft.Win32.SafeHandles.SafeFileHandle
            //    .DangerousCreate(stdout.SafeFileHandle.DangerousGetHandle(), ownsHandle: false);

            //if (!Microsoft.Win32.UnsafeNativeMethods.SetConsoleMode(handle, ENABLE_VIRTUAL_TERMINAL_PROCESSING))
            //{
            //    throw new System.ComponentModel.Win32Exception();
            //}

            const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
            const int STD_OUTPUT_HANDLE = -11;

            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
                SetConsoleMode(iStdOut, outConsoleMode);
            }
        }
    }
}
