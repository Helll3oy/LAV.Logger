using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LAV.Logger
{
    public static class ColorExtensions
    {
        public static Color DecreaseBrightness(this Color originalColor, float factor)
        {
            if (factor < 0 || factor > 1)
                throw new ArgumentException("Factor must be between 0 and 1.");

            int r = (int)(originalColor.R * factor);
            int g = (int)(originalColor.G * factor);
            int b = (int)(originalColor.B * factor);

            return Color.FromArgb(originalColor.A, (byte)r, (byte)g, (byte)b);
        }

        public static Color DecreaseBrightnessUsingHsl(this Color originalColor, float factor)
        {
            ColorToHsl(originalColor, out double h, out double s, out double l);
            l *= factor; // Reduce lightness
            l = l.Clamp(0, 1);
            return HslToColor(h, s, l, originalColor.A);
        }

        public static Color ToColor(this ConsoleColor consoleColor)
        {
            // Map ConsoleColor to its RGB equivalent
            return consoleColor switch
            {
                ConsoleColor.Black => Color.FromArgb(0, 0, 0),
                ConsoleColor.DarkBlue => Color.FromArgb(0, 0, 128),
                ConsoleColor.DarkGreen => Color.FromArgb(0, 128, 0),
                ConsoleColor.DarkCyan => Color.FromArgb(0, 128, 128),
                ConsoleColor.DarkRed => Color.FromArgb(128, 0, 0),
                ConsoleColor.DarkMagenta => Color.FromArgb(128, 0, 128),
                ConsoleColor.DarkYellow => Color.FromArgb(128, 128, 0),
                ConsoleColor.Gray => Color.FromArgb(192, 192, 192), // Light gray
                ConsoleColor.DarkGray => Color.FromArgb(128, 128, 128), // Medium gray
                ConsoleColor.Blue => Color.FromArgb(0, 0, 255),
                ConsoleColor.Green => Color.FromArgb(0, 255, 0),
                ConsoleColor.Cyan => Color.FromArgb(0, 255, 255),
                ConsoleColor.Red => Color.FromArgb(255, 0, 0),
                ConsoleColor.Magenta => Color.FromArgb(255, 0, 255),
                ConsoleColor.Yellow => Color.FromArgb(255, 255, 0),
                ConsoleColor.White => Color.FromArgb(255, 255, 255),
                _ => throw new ArgumentOutOfRangeException(nameof(consoleColor), "Invalid ConsoleColor value.")
            };
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static void ColorToHsl(Color color, out double h, out double s, out double l)
        {
            const double epsilon = 0.0000000001f;
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            // Lightness
            l = (max + min) / 2.0;

            // Saturation
            if (delta >= 0 - epsilon || delta <= 0 + epsilon)
            {
                h = 0;
                s = 0;
            }
            else
            {
                s = l < 0.5 ? delta / (max + min) : delta / (2.0 - max - min);
                // Hue
                if (max >= r - epsilon || max <= r + epsilon)
                    h = (g - b) / delta + (g < b ? 6 : 0);
                else if (max >= g - epsilon || max <= g + epsilon)
                    h = (b - r) / delta + 2;
                else
                    h = (r - g) / delta + 4;
                h *= 60;
            }
        }

        public static Color HslToColor(double h, double s, double l, int alpha = 255)
        {
            h = h % 360;
            if (h < 0) h += 360;
            s = s.Clamp(0, 1);
            l = l.Clamp(0, 1);

            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = l - c / 2;

            double r, g, b;
            if (h < 60) (r, g, b) = (c, x, 0);
            else if (h < 120) (r, g, b) = (x, c, 0);
            else if (h < 180) (r, g, b) = (0, c, x);
            else if (h < 240) (r, g, b) = (0, x, c);
            else if (h < 300) (r, g, b) = (x, 0, c);
            else (r, g, b) = (c, 0, x);

            return Color.FromArgb(
                alpha,
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255)
            );
        }
    }

    public static class ConsoleColorHelper
    {
        // Windows API structs and methods
        [StructLayout(LayoutKind.Sequential)]
        private struct CONSOLE_SCREEN_BUFFER_INFO_EX
        {
            public int cbSize;
            public COORD dwSize;
            public COORD dwCursorPosition;
            public ushort wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
            public ushort wPopupAttributes;
            public bool bFullscreenSupported;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public COLORREF[] ColorTable; // 16 entries for ConsoleColor

            //public COLORREF black;
            //public COLORREF darkBlue;
            //public COLORREF darkGreen;
            //public COLORREF darkCyan;
            //public COLORREF darkRed;
            //public COLORREF darkMagenta;
            //public COLORREF darkYellow;
            //public COLORREF gray;
            //public COLORREF darkGray;
            //public COLORREF blue;
            //public COLORREF green;
            //public COLORREF cyan;
            //public COLORREF red;
            //public COLORREF magenta;
            //public COLORREF yellow;
            //public COLORREF white;

            //public COLORREF this[int index]
            //    => index switch
            //    {
            //        0 => black,
            //        1 => darkBlue,
            //        2 => darkGreen,
            //        3 => darkCyan,
            //        4 => darkRed,
            //        5 => darkMagenta,
            //        6 => darkYellow,
            //        7 => gray,
            //        8 => darkGray,
            //        9 => blue,
            //        10 => green,
            //        11 => cyan,
            //        12 => red,
            //        13 => magenta,
            //        14 => yellow,
            //        15 => white,
            //        _ => black
            //    };

        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            public short X;
            public short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COLORREF
        {
            public uint ColorDWORD;

            //public COLORREF(Color color)
            //{
            //    ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            //}

            //public COLORREF(uint r, uint g, uint b)
            //{
            //    ColorDWORD = r + (g << 8) + (b << 16);
            //}

            //public Color GetColor()
            //{
            //    return Color.FromArgb((int)(0x000000FFU & ColorDWORD),
            //                          (int)(0x0000FF00U & ColorDWORD) >> 8, (int)(0x00FF0000U & ColorDWORD) >> 16);
            //}

            //public void SetColor(Color color)
            //{
            //    ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            //}

            public Color ToColor()
            {
                // Convert 0x00BBGGRR to Color
                return Color.FromArgb(
                    (int)(ColorDWORD & 0xFF),         // Red
                    (int)((ColorDWORD >> 8) & 0xFF),  // Green
                    (int)((ColorDWORD >> 16) & 0xFF) // Blue
                );
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleScreenBufferInfoEx(
            IntPtr hConsoleOutput,
            ref CONSOLE_SCREEN_BUFFER_INFO_EX consoleInfo
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        // Standard output handle ID
        private const int STD_OUTPUT_HANDLE = -11;

        public static Color? GetActualConsoleBackgroundColorRgb()
        {
#if NET8_0_OR_GREATER
            if (!OperatingSystem.IsWindows()) return default;
#elif NETSTANDARD1_3 || NETSTANDARD2_0_OR_GREATER || NET471_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return default;
#else
            if ( Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix
                || Environment.OSVersion.Platform == PlatformID.Xbox) return default;
#endif

            var consoleInfo = new CONSOLE_SCREEN_BUFFER_INFO_EX();
            consoleInfo.cbSize = Marshal.SizeOf(consoleInfo);
            IntPtr hConsole = GetStdHandle(STD_OUTPUT_HANDLE);

            if (!GetConsoleScreenBufferInfoEx(hConsole, ref consoleInfo))
                return default;
            //throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            // Get the background color index (upper 4 bits of attributes)
            int bgColorIndex = (consoleInfo.wAttributes >> 4) & 0xF;

            // Get the RGB value from the color table
            COLORREF bgColorRef = consoleInfo.ColorTable[bgColorIndex];
            return bgColorRef.ToColor();
        }
    }
}
