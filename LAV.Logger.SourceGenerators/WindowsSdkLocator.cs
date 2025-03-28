using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace LAV.Logger.SourceGenerators
{
    public static class WindowsSdkLocator
    {
        public static string? GetLatestWindowsSdkVersion()
        {
            // Method 1: Check registry first (most reliable)
            var sdkVersion = GetVersionFromRegistry();
            if (!string.IsNullOrEmpty(sdkVersion))
                return sdkVersion;

            //// Method 2: Scan program files as fallback
            //return GetVersionFromProgramFiles();

            return null;
        }

        private static string? GetVersionFromRegistry()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows Kits\Installed Roots");

                var maxVersion = new Version(0, 0);
                foreach (var valueName in key!.GetValueNames())
                {
                    if (Version.TryParse(valueName, out var current) &&
                        current > maxVersion)
                    {
                        maxVersion = current;
                    }
                }

                return maxVersion > new Version(0, 0) ? maxVersion.ToString() : null;
            }
            catch
            {
                return null;
            }
        }

        //private static string? GetVersionFromProgramFiles()
        //{
        //    try
        //    {
        //        var programFiles = Environment.GetFolderPath(
        //            Environment.SpecialFolder.ProgramFilesX86);
        //        var kitsRoot = Path.Combine(programFiles, "Windows Kits", "10", "bin");

        //        if (!Directory.Exists(kitsRoot))
        //            return null;

        //        return Directory.GetDirectories(kitsRoot)
        //            .Select(Path.GetFileName)
        //            .Where(v => Version.TryParse(v, out _))
        //            .Select(v => new Version(v))
        //            .OrderByDescending(v => v)
        //            .FirstOrDefault()?
        //            .ToString();
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //// Helper method for your source generator
        //public static string GetWindowsSdkBinPath()
        //{
        //    var version = GetLatestWindowsSdkVersion();
        //    if (version == null)
        //        throw new FileNotFoundException("Windows SDK not found");

        //    var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        //    return Path.Combine(
        //        programFiles,
        //        "Windows Kits",
        //        "10",
        //        "bin",
        //        version,
        //        "x64");
        //}
    }
}
