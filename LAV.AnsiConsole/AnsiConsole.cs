using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace LAV.AnsiConsole
{
    public sealed class AnsiConsoleWriter
    {
        private readonly AnsiConsole _parent;
        private readonly StringBuilder _sb;
        public AnsiConsoleWriter(AnsiConsole parent, StringBuilder sb)
        {
            _parent = parent;
            _sb = sb;
        }

        public void Write()
        {
            Console.Write(_sb.ToString());
        }

        public void WriteLine()
        {
            Console.WriteLine(_sb.ToString());
        }

        public string GetAnsiString() => _sb.ToString();

        public AnsiConsole ApplyStyle(Action<AnsiConsole> styler)
        {
            styler?.Invoke(_parent);

            return _parent;
        }

        public AnsiConsoleWriter AddSimpleText(string text)
        {
            _parent.AddSimpleText(text);

            return this;
        }
    }

    public sealed class AnsiConsole
    {
        private const string AnsiStart = "\x1b[";
        private const string AnsiEnd = AnsiStart + "0m";

        static AnsiConsole()
        {
            AnsiCodeEnabler.Enable();
        }

        private readonly StringBuilder _ansiCode;

        private bool _stylized;

        private int _styleStartPosition;

        private readonly AnsiConsoleWriter _writer;

        private AnsiConsole(StringBuilder sb)
        {
            _ansiCode = sb ?? new StringBuilder();
            _writer = new AnsiConsoleWriter(this, sb);
        }

        public string GetAnsiString() => _ansiCode.ToString();

        public static AnsiConsole ApplyStyle(Action<AnsiConsole> styler)
        {
            var instance = new AnsiConsole(new StringBuilder());
            styler?.Invoke(instance);

            return instance;
        }

        public static void Write(string text)
        {
            Console.Write(text);
        }

        public static void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        // Text Formatting
        private enum AnsiTextFormatting
        {
            ResetAllStyles = 0,
            BoldOrBright = 1,
            FaintOrDim = 2,
            Italic = 3,
            Underline = 4,
            SlowBlink = 5,
            RapidBlink = 6,
            InvertColors = 7,
            ConcealOrHidden = 8,
            StrikeOrThrough = 9
        }

        public AnsiConsole ResetAllStyles => ApplyFormatting(AnsiTextFormatting.ResetAllStyles);
        public AnsiConsole BoldOrBright => ApplyFormatting(AnsiTextFormatting.BoldOrBright);
        public AnsiConsole FaintOrDim => ApplyFormatting(AnsiTextFormatting.FaintOrDim);
        public AnsiConsole Italic => ApplyFormatting(AnsiTextFormatting.Italic);
        public AnsiConsole Underline => ApplyFormatting(AnsiTextFormatting.Underline);
        public AnsiConsole SlowBlink => ApplyFormatting(AnsiTextFormatting.SlowBlink);
        public AnsiConsole RapidBlink => ApplyFormatting(AnsiTextFormatting.RapidBlink);
        public AnsiConsole InvertColors => ApplyFormatting(AnsiTextFormatting.InvertColors);
        public AnsiConsole ConcealOrHidden => ApplyFormatting(AnsiTextFormatting.ConcealOrHidden);
        public AnsiConsole StrikeOrThrough => ApplyFormatting(AnsiTextFormatting.StrikeOrThrough);

        // Foreground colors
        private enum AnsiForegroundColor
        {
            Black = 30,
            Red = 31,
            Green = 32,
            Yellow = 33,
            Blue = 34,
            Magenta = 35,
            Cyan = 36,
            White = 37,
            BrightBlack = 90,
            BrightRed = 91,
            BrightGreen = 92,
            BrightYellow = 93,
            BrightBlue = 94,
            BrightMagenta = 95,
            BrightCyan = 96,
            BrightWhite = 97
        }

        public AnsiConsole Black => ApplyForegroundColor(AnsiForegroundColor.Black);
        public AnsiConsole Red => ApplyForegroundColor(AnsiForegroundColor.Red);
        public AnsiConsole Green => ApplyForegroundColor(AnsiForegroundColor.Green);
        public AnsiConsole Yellow => ApplyForegroundColor(AnsiForegroundColor.Yellow);
        public AnsiConsole Blue => ApplyForegroundColor(AnsiForegroundColor.Blue);
        public AnsiConsole Magenta => ApplyForegroundColor(AnsiForegroundColor.Magenta);
        public AnsiConsole Cyan => ApplyForegroundColor(AnsiForegroundColor.Cyan);
        public AnsiConsole White => ApplyForegroundColor(AnsiForegroundColor.White);
        public AnsiConsole BrightBlack => ApplyForegroundColor(AnsiForegroundColor.BrightBlack);
        public AnsiConsole BrightRed => ApplyForegroundColor(AnsiForegroundColor.BrightRed);
        public AnsiConsole BrightGreen => ApplyForegroundColor(AnsiForegroundColor.BrightGreen);
        public AnsiConsole BrightYellow => ApplyForegroundColor(AnsiForegroundColor.BrightYellow);
        public AnsiConsole BrightBlue => ApplyForegroundColor(AnsiForegroundColor.BrightBlue);
        public AnsiConsole BrightMagenta => ApplyForegroundColor(AnsiForegroundColor.BrightMagenta);
        public AnsiConsole BrightCyan => ApplyForegroundColor(AnsiForegroundColor.BrightCyan);
        public AnsiConsole BrightWhite => ApplyForegroundColor(AnsiForegroundColor.BrightWhite);

        // Background colors (existing implementation)
        private enum AnsiBackgroundColor
        {
            Black = 40,
            Red = 41,
            Green = 42,
            Yellow = 43,
            Blue = 44,
            Magenta = 45,
            Cyan = 46,
            White = 47,
            BrightBlack = 100,
            BrightRed = 101,
            BrightGreen = 102,
            BrightYellow = 103,
            BrightBlue = 104,
            BrightMagenta = 105,
            BrightCyan = 106,
            BrightWhite = 107
        }

        public AnsiConsole BgBlack => ApplyBackgroundColor(AnsiBackgroundColor.Black);
        public AnsiConsole BgRed => ApplyBackgroundColor(AnsiBackgroundColor.Red);
        public AnsiConsole BgGreen => ApplyBackgroundColor(AnsiBackgroundColor.Green);
        public AnsiConsole BgYellow => ApplyBackgroundColor(AnsiBackgroundColor.Yellow);
        public AnsiConsole BgBlue => ApplyBackgroundColor(AnsiBackgroundColor.Blue);
        public AnsiConsole BgMagenta => ApplyBackgroundColor(AnsiBackgroundColor.Magenta);
        public AnsiConsole BgCyan => ApplyBackgroundColor(AnsiBackgroundColor.Cyan);
        public AnsiConsole BgWhite => ApplyBackgroundColor(AnsiBackgroundColor.White);
        public AnsiConsole BgBrightBlack => ApplyBackgroundColor(AnsiBackgroundColor.BrightBlack);
        public AnsiConsole BgBrightRed => ApplyBackgroundColor(AnsiBackgroundColor.BrightRed);
        public AnsiConsole BgBrightGreen => ApplyBackgroundColor(AnsiBackgroundColor.BrightGreen);
        public AnsiConsole BgBrightYellow => ApplyBackgroundColor(AnsiBackgroundColor.BrightYellow);
        public AnsiConsole BgBrightBlue => ApplyBackgroundColor(AnsiBackgroundColor.BrightBlue);
        public AnsiConsole BgBrightMagenta => ApplyBackgroundColor(AnsiBackgroundColor.BrightMagenta);
        public AnsiConsole BgBrightCyan => ApplyBackgroundColor(AnsiBackgroundColor.BrightCyan);
        public AnsiConsole BgBrightWhite => ApplyBackgroundColor(AnsiBackgroundColor.BrightWhite);

        private AnsiConsole ApplyFormatting(AnsiTextFormatting formatting)
        {
            return AddCode(((int)formatting).ToString());
        }

        /// <summary>
        /// 16-Color Mode foreground color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private AnsiConsole ApplyForegroundColor(AnsiForegroundColor color)
        {
            return AddCode(((int)color).ToString());
        }

        /// <summary>
        /// 256-Color Mode foreground color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public AnsiConsole ApplyForegroundColor(byte color)
        {
            return AddCode($"38;5;{color}");
        }

        /// <summary>
        /// Truecolor (RGB) foreground color
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <returns></returns>
        public AnsiConsole ApplyForegroundColor(byte red, byte green, byte blue)
        {
            return AddCode($"38;2;{red};{green};{blue}");
        }

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER || NETFRAMEWORK
        public AnsiConsole ApplyForegroundColor(Color color)
        {
            return AddCode($"38;2;{color.R};{color.G};{color.B}");
        }
#endif

        /// <summary>
        /// 16-Color Mode background color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private AnsiConsole ApplyBackgroundColor(AnsiBackgroundColor color)
        {
            return AddCode(((int)color).ToString());
        }

        /// <summary>
        /// 256-Color Mode background color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public AnsiConsole ApplyBackgroundColor(byte color)
        {
            return AddCode($"48;5;{color}");
        }

        /// <summary>
        /// Truecolor (RGB) background color
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <returns></returns>
        public AnsiConsole ApplyBackgroundColor(byte red, byte green, byte blue)
        {
            return AddCode($"48;2;{red};{green};{blue}");
        }

#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER || NETFRAMEWORK
        public AnsiConsole ApplyBackgroundColor(Color color)
        {
            return AddCode($"48;2;{color.R};{color.G};{color.B}");
        }
#endif

        #region Cursor Control
        public enum AnsiCursorMoveControl
        {
            Up = 'A',          // \x1b[<n>A
            Down = 'B',        // \x1b[<n>B
            Right = 'C',       // \x1b[<n>C
            Left = 'D',        // \x1b[<n>D
        }

        private enum AnsiCursorControl
        {
            MoveToPosition = 'H',  // \x1b[<row>;<col>H

            SavePosition = 's',    // \x1b[s
            RestorePosition = 'u', // \x1b[u

            HideCursor = 'l',      // \x1b[?25l
            ShowCursor = 'h'       // \x1b[?25h
        }

        public AnsiConsole MoveCursor(AnsiCursorMoveControl moveControl, int n = 1)
        {
            return AddCode($"{n}{(char)moveControl}");
        }

        public AnsiConsole MoveToPosition(int row, int col)
        {
            return AddCode($"{row};{col}{(char)AnsiCursorControl.MoveToPosition}");
        }

        public AnsiConsole SavePosition()
        {
            return AddCode($"{(char)AnsiCursorControl.SavePosition}");
        }

        public AnsiConsole RestorePosition()
        {
            return AddCode($"{(char)AnsiCursorControl.RestorePosition}");
        }

        public AnsiConsole ShowCursor()
        {
            return AddCode($"?25{(char)AnsiCursorControl.ShowCursor}");
        }

        public AnsiConsole HideCursor()
        {
            return AddCode($"?25{(char)AnsiCursorControl.HideCursor}");
        }
        #endregion Cursor Control

        #region Screen Control
        public enum AnsiLineControl
        {
            ClearLineToEnd = 0,    // \x1b[0K
            ClearLineToStart = 1,  // \x1b[1K
            ClearEntireLine = 2    // \x1b[2K
        }

        public enum AnsiScreenControl
        {
            ClearScreen = 2,       // \x1b[2J
            ClearScrollback = 3,   // \x1b[3J
        }

        public AnsiConsole ClearScreen(AnsiScreenControl control)
        {
            return AddCode($"{(int)control}J");
        }

        public AnsiConsole ClearLine(AnsiLineControl control)
        {
            return AddCode($"{(int)control}K");
        }
        #endregion Screen Control

        public AnsiConsole AddCode(string code)
        {
            if (!_stylized)
            {
                _styleStartPosition = _ansiCode.Length - 1;

                _ansiCode.EnsureCapacity(_styleStartPosition + 64);

                _ansiCode.Append(AnsiStart);
                _stylized = true;
            }

            _ansiCode.Append(code);
            _ansiCode.Append(';');
            return this;
        }

        public AnsiConsoleWriter AddText(string text)
        {
            EndStyling();

            _ansiCode.Append(text);

            _ansiCode.Append(AnsiEnd);
            _stylized = false;

            return _writer;
        }

        public AnsiConsoleWriter AddSimpleText(string text)
        {
            ClearlastStyling();

            _ansiCode.Append(text);

            return _writer;
        }

        private void ClearlastStyling()
        {
            if (!_stylized) return;

            _ansiCode.Remove(_styleStartPosition, _ansiCode.Length - 1 - _styleStartPosition);
        }

        private void EndStyling()
        {
            if (!_stylized) return;

            int len = _ansiCode.Length;
            int i = len;
            while (_ansiCode[i - 1] == ';')
            {
                i--;
            }

            _ansiCode.Remove(i, len - i);

            _ansiCode.Append('m');
        }
    }
}
