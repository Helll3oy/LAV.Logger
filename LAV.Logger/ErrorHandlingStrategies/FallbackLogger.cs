using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LAV.Logger.ErrorHandlingStrategies
{
    internal sealed class FallbackLogger
    {
        private FallbackLogger()
        {
        }

        // Writes to local file/console when all other options fail
        public static void LogError(string message)
        {
            Console.Error.WriteLine($"[FALLBACK] {message}");
            File.AppendAllText("logs/fallback.log", message);
        }
    }
}
