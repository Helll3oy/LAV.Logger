using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace LAV.Logger
{
#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public sealed class MessageFileCompiler : IDisposable
    {
        private readonly string _tempWorkingDir;
        private readonly string _outputDllPath;
        private bool _disposed;

        public MessageFileCompiler(string outputDllPath)
        {
            _outputDllPath = outputDllPath;
            _tempWorkingDir = CreateSecureTempDirectory();
        }

        public void CompileFromCode(params EventCategory[] categories)
        {
            ValidateExecutionEnvironment();

            var mcFilePath = GenerateMcFile(categories);
            var (_, rcFilePath) = RunMessageCompiler(mcFilePath);
            var resFilePath = CompileResources(rcFilePath);
            LinkResourceFile(resFilePath);

            Console.WriteLine($"Successfully compiled to: {_outputDllPath}");
        }

        private string GenerateMcFile(EventCategory[] categories)
        {
            var mcContent = new StringBuilder();
            mcContent.AppendLine("MessageIdTypedef=WORD");
            mcContent.AppendLine("MessageId=0x1");

            foreach (var category in categories)
            {
                mcContent.AppendLine($"MessageId=0x{category.Id:X4}");
                mcContent.AppendLine($"SymbolicName={category.SymbolicName}");
                mcContent.AppendLine($"Language=English");
                mcContent.AppendLine($"{category.DisplayName}");
                mcContent.AppendLine(".");
            }

            var mcPath = Path.Combine(_tempWorkingDir, "EventCategories.mc");
            File.WriteAllText(mcPath, mcContent.ToString());
            return mcPath;
        }

        private (string HeaderPath, string RcPath) RunMessageCompiler(string mcPath)
        {
            var mcExe = FindWindowsSdkTool("mc.exe");
            ExecuteProcess(mcExe, $"-r \"{_tempWorkingDir}\" -h \"{_tempWorkingDir}\" \"{mcPath}\"");
            return (
                Path.Combine(_tempWorkingDir, "EventCategories.h"),
                Path.Combine(_tempWorkingDir, "EventCategories.rc")
            );
        }

        private string CompileResources(string rcPath)
        {
            var rcExe = FindWindowsSdkTool("rc.exe");
            var resPath = Path.Combine(_tempWorkingDir, "EventCategories.res");
            ExecuteProcess(rcExe, $"/fo \"{resPath}\" \"{rcPath}\"");
            return resPath;
        }

        private void LinkResourceFile(string resPath)
        {
            var linkExe = FindVCTool("link.exe");
            ExecuteProcess(linkExe,
                $"/DLL /NOENTRY /MACHINE:{GetMachineArchitecture()} " +
                $"/OUT:\"{_outputDllPath}\" \"{resPath}\"");
        }

        private static string FindWindowsSdkTool(string toolName)
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var sdkPath = Path.Combine(programFiles, "Windows Kits", "10", "bin");

            var versions = Directory.GetDirectories(sdkPath)
                .Select(Path.GetFileName)
                .Where(v => v?.StartsWith("10.") == true)
                .OrderByDescending(v => v)
                .ToList();

            foreach (var version in versions)
            {
                var toolPath = Path.Combine(sdkPath, version, GetMachineArchitecture(), toolName);
                if (File.Exists(toolPath)) return toolPath;
            }

            throw new FileNotFoundException($"Windows SDK tool {toolName} not found");
        }

        private static string FindVCTool(string toolName)
        {
            var vsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Microsoft Visual Studio",
                "2022"
            );

            var editions = new[] { "Enterprise", "Professional", "Community", "BuildTools" };
            foreach (var edition in editions)
            {
                var toolPath = Path.Combine(vsPath, edition, "VC", "Tools", "MSVC");
                if (!Directory.Exists(toolPath)) continue;

                var versions = Directory.GetDirectories(toolPath)
                    .OrderByDescending(d => d)
                    .ToList();

                foreach (var version in versions)
                {
                    var fullPath = Path.Combine(version, "bin", "Hostx64", GetMachineArchitecture(), toolName);
                    if (File.Exists(fullPath)) return fullPath;
                }
            }

            throw new FileNotFoundException($"Visual Studio tool {toolName} not found");
        }

        private static string CreateSecureTempDirectory()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            // Set secure permissions
            var directoryInfo = new DirectoryInfo(tempPath);
            var security = directoryInfo.GetAccessControl();
            security.SetAccessRuleProtection(true, false);
            security.AddAccessRule(new FileSystemAccessRule(
                WindowsIdentity.GetCurrent().Name,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));

            directoryInfo.SetAccessControl(security);
            return tempPath;
        }

        private void ExecuteProcess(string executable, string arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = _tempWorkingDir
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Process {executable} failed with code {process.ExitCode}\n" +
                    $"Output: {output}\nError: {error}");
            }
        }

        private static void ValidateExecutionEnvironment()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("Message compilation requires Windows");

            if (!IsAdministrator())
                throw new UnauthorizedAccessException("Administrator privileges required");
        }

        private static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static string GetMachineArchitecture()
        {
            return Environment.Is64BitProcess ? "x64" : "x86";
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try { Directory.Delete(_tempWorkingDir, true); }
                catch { /* Suppress cleanup errors */ }
                _disposed = true;
            }
        }
    }

    public record EventCategory(int Id, string SymbolicName, string DisplayName);
}
