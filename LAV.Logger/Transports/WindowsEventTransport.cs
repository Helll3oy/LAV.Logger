using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using LAV.Logger.Formatters;
using System.Diagnostics;
using LAV.Logger.ErrorHandlingStrategies;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.IO;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace LAV.Logger.Transports
{
#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public static class EventViewManager
    {
        private const string ViewsPath = @"C:\ProgramData\Microsoft\Event Viewer\Views";

        public static void CreateCustomView(string viewName,
                                   string[] eventSources,
                                   string[] eventLevels,
                                   string query)
        {
            if (!IsAdministrator())
                throw new UnauthorizedAccessException("Requires admin privileges");

            var viewXml = $"""
            <ViewerConfig>
                <QueryConfig>
            	<QueryParams>
            		<UserQuery />
            	</QueryParams>
            	<QueryNode>
            		<Name LanguageNeutralValue="{viewName}">{viewName}</Name>
            		<QueryList>
            			<Query Id="0" Path="{viewName}">
            				<Select Path="{viewName}">*[System[Provider[@Name='{string.Join("' or @Name='", eventSources)}']]]</Select>
            			</Query>
            		</QueryList>
            	</QueryNode>
            </QueryConfig>
            </ViewerConfig>
            """;

            var fileName = Path.Combine(ViewsPath, $"{viewName}.xml");
            File.WriteAllText(fileName, viewXml);
            RefreshEventViewer();
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void RefreshEventViewer()
        {
            Process.Start("wevtutil", "el /bulk:\"C:\\ProgramData\\Microsoft\\Event Viewer\\Views\"");
        }

        // Example Usage
        public static void CreateApplicationErrorView()
        {
            CreateCustomView(
                viewName: "MyAppErrors",
                eventSources: new[] { "MyApplication" },
                eventLevels: new[] { "2", "3" }, // 2=Error, 3=Warning
                query: "and *[EventData[Data[@Name='UserId']='SPECIFIC_USER']"
            );
        }
    }

    internal class EventDataCollection
    {
        private readonly List<KeyValuePair<string, object>> _data;

        public EventDataCollection(object state)
        {
            _data = (state as IEnumerable<KeyValuePair<string, object>>)?.ToList()
                ?? new List<KeyValuePair<string, object>>();
        }

        public byte[] ToByteArray()
        {
            return _data
                .SelectMany(kvp =>
                    BitConverter.GetBytes(kvp.Key.Length)
                    .Concat(Encoding.UTF8.GetBytes(kvp.Key))
                    .Concat(BitConverter.GetBytes(kvp.Value.ToString().Length))
                    .Concat(Encoding.UTF8.GetBytes(kvp.Value.ToString())))
                .ToArray();
        }
    }

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public static class EventViews
    {
        public static void CreateAllViews(string viewName, string[] eventSources)
        {
            CreateSecurityView(viewName, eventSources);
            CreateCommonView(viewName, eventSources);
            CreatePerformanceView(viewName, eventSources);
            CreateCustomBusinessView(viewName, eventSources);
        }

        private static void CreateCommonView(string viewName, string[] eventSources)
        {
            EventViewManager.CreateCustomView(viewName, eventSources,
                Enumerable.Range(1, 16).Select(s => s.ToString()).ToArray(),
                "");
        }

        private static void CreatePerformanceView(string viewName, string[] eventSources)
        {

        }

        private static void CreateCustomBusinessView(string viewName, string[] eventSources)
        {

        }

        private static void CreateSecurityView(string viewName, string[] eventSources)
        {
            //new EventViewManager().CreateCustomView(
            //    "SecurityAudit",
            //    new[] { "Security" },
            //    new[] { "4" },
            //    "and EventID=4663"
            //);
        }
    }

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public sealed class WindowsEventTransportOptions : ITransportOptions
    {
#if !NET452
        public MessagePackSerializerOptions MessagePackOptions { get; }
#endif

        public ILogFormatter[] Formatters { get; }

        public bool EnableCompression { get; } = false;

        public TimeSpan Timeout { get; set; }

        public int MaximumKilobytes { get; set; } = 10240;

        public Dictionary<string, string> Sources { get; set; } = [];

        public string LogName { get; set; } = $"{Assembly.GetEntryAssembly().GetName().Name}Log";

        public WindowsEventTransportOptions()
        {
#if !NET452
            MessagePackOptions = MessagePackSerializerOptions.Standard
                .WithResolver(
                    CompositeResolver.Create(
                        new IMessagePackFormatter[] { new EventIdFormatter(), new LogLevelFormatter(), new ExceptionFormatter() },
                        new IFormatterResolver[] { StandardResolver.Instance }
                    )
                );

            Formatters = [new WindowsEventFormatter(MessagePackOptions)];
#else
            var resolver = new IFormatterResolver[] { StandardResolver.Instance };
            Formatters = [new WindowsEventFormatter(resolver[0])];
#endif
        }
    }

    internal unsafe struct FixedString32
    {
        private const int MAX_LENGTH = 32;
        private fixed char _buffer[MAX_LENGTH];
        private readonly byte _length;

        public FixedString32(string s)
        {
            _length = (byte)Math.Min(s.Length, MAX_LENGTH);
            fixed (char* ptr = _buffer)
            {
                for (int i = 0; i < _length; i++)
                    ptr[i] = s[i];
            }
        }

        public override string ToString()
        {
            fixed (char* ptr = _buffer)
                return new string(ptr, 0, _length);
        }
    }

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    public sealed class WindowsEventTransport : TransportBase
    {
        new public WindowsEventTransportOptions Options => (WindowsEventTransportOptions)_options;

        private readonly Dictionary<string, EventLog> _eventLogs = [];
        private readonly EventLog _defaultEventLog;

        public WindowsEventTransport(WindowsEventTransportOptions options) : base(options)
        {
            string DEFAULT_SOURCE_NAME = Assembly.GetEntryAssembly().GetName().Name;
            string keyPath = $@"SYSTEM\CurrentControlSet\Services\EventLog\{options.LogName}";

#if NET5_0_OR_GREATER
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("EventLog is only supported on Windows.");
#else
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("EventLog is only supported on Windows.");
#endif
            bool check;
            try
            {
                check = !EventLog.SourceExists(DEFAULT_SOURCE_NAME) || options.Sources.Any(s => !EventLog.SourceExists(s.Value));
            }
            catch
            {
                check = true;
            }

            if (check && !EventViewManager.IsAdministrator())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true,
                    //RedirectStandardOutput = true,
                    Verb = "runas" // Triggers the UAC dialog
                };

                try
                {
                    var process = Process.GetCurrentProcess();

                    if (process.WaitForExit(5000))
                    {
                        Process.Start(startInfo);
                    }
                    else
                    {
                        Process.Start(startInfo);
                        process.Kill();
                    }

                    return;
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    // User canceled the UAC prompt
                    EventLog.WriteEntry("Application", "Admin rights are required to proceed.", EventLogEntryType.Error);
                    throw;
                }
            }

            if (check)
            {
                if (!EventLog.SourceExists(DEFAULT_SOURCE_NAME))
                {
                    EventLog.CreateEventSource(DEFAULT_SOURCE_NAME, options.LogName);
                }

                foreach (var source in options.Sources.Where(static source => !EventLog.SourceExists(source.Value)))
                {
                    EventLog.CreateEventSource(source.Value, options.LogName);
                }

                EventViews.CreateAllViews(options.LogName, options.Sources.Values.Concat([DEFAULT_SOURCE_NAME]).ToArray());

                using RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true)
                    ?? throw new KeyNotFoundException($"Registry key \"{keyPath}\" not found.");

                RegistrySecurity security = key.GetAccessControl();
                var user = WindowsIdentity.GetCurrent().User;
                security.AddAccessRule(new RegistryAccessRule(
                    user,
                    RegistryRights.WriteKey | RegistryRights.ReadKey,
                    AccessControlType.Allow
                ));
                key.SetAccessControl(security);

                Console.WriteLine("Permissions granted.");
            }

            foreach (var source in options.Sources)
            {
                var eventLog = new EventLog(options.LogName, ".", source.Value)
                {
                    MaximumKilobytes = options.MaximumKilobytes,
                };

                eventLog.ModifyOverflowPolicy(OverflowAction.OverwriteOlder, 30);

                _eventLogs.Add(source.Key, eventLog);
            }

            _defaultEventLog = new EventLog(options.LogName, ".", DEFAULT_SOURCE_NAME)
            {
                MaximumKilobytes = options.MaximumKilobytes,
            };

            _defaultEventLog.ModifyOverflowPolicy(OverflowAction.OverwriteOlder, 30);
        }

        public static string Truncate(string value, int maxLength)
        {
            return string.IsNullOrEmpty(value)
                ? value
                : value.Length <= maxLength
                    ? value
                    : value.Substring(0, maxLength);
        }

        //public static string TruncateWithEllipsis(string value, int maxLength)
        //{
        //    const string ellipsis = "...";

        //    if (string.IsNullOrEmpty(value)) return value;
        //    if (maxLength <= ellipsis.Length) return ellipsis;

        //    return value.Length <= maxLength
        //        ? value
        //        : string.Concat(value.AsSpan(0, maxLength - ellipsis.Length), ellipsis);
        //}

        //CRC16 (better collision resistance)
        public static short ComputeCrc16(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            ushort crc = 0;
            foreach (byte b in bytes)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                    crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
            }
            return (short)crc;
        }

        //FNV-1a Hash (fast alternative)
        public static short Fnv1aHash(string input)
        {
            const ushort prime = 0x0103;
            ushort hash = 0x811C;
            foreach (char c in input)
            {
                hash ^= (ushort)c;
                hash *= prime;
            }
            return (short)hash;
        }

        public override void Send(byte[] data)
        {
            try
            {
#if !NET452
                var entry = MessagePackSerializer.Deserialize<LavLogEntry>(new MemoryStream(data), Options.MessagePackOptions);
#else
                var entry = MessagePackSerializer.Deserialize<LavLogEntry>(new MemoryStream(data));
#endif

                if (!_eventLogs.TryGetValue(entry.Category, out var eventLog))
                {
                    eventLog = _defaultEventLog;
                }

                var level = entry.Level switch
                {
                    Microsoft.Extensions.Logging.LogLevel.Trace => EventLogEntryType.Information,
                    Microsoft.Extensions.Logging.LogLevel.Debug => EventLogEntryType.Information,
                    Microsoft.Extensions.Logging.LogLevel.Information => EventLogEntryType.Information,
                    Microsoft.Extensions.Logging.LogLevel.Warning => EventLogEntryType.Warning,
                    Microsoft.Extensions.Logging.LogLevel.Error => EventLogEntryType.Error,
                    Microsoft.Extensions.Logging.LogLevel.Critical => EventLogEntryType.Error,
                    _ => EventLogEntryType.Information
                };

                short category = Fnv1aHash(entry.Category);

                // Reuses existing instance if possible
                //EventLog.WriteEntry("Application", entry.Message, level,
                //entry.EventId.Id, 0, new EventDataCollection(entry.State).ToByteArray());

                // Write to event log with structured data
                eventLog.WriteEntry(
                    Truncate(entry.Message, 31839),
                    type: level,
                    eventID: entry.EventId.Id,
                    category,
                    new EventDataCollection(entry.State).ToByteArray());
            }
            catch (Exception ex)
            {
                // Fallback to debug output
                FallbackLogger.LogError($"Event log write failed: {ex.Message}");
            }
        }

        public override void Send(ReadOnlySpan<byte> data)
        {
            try
            {
#if !NET452
                var entry = MessagePackSerializer.Deserialize<LavLogEntry>(new MemoryStream(data.ToArray()), Options.MessagePackOptions);
#else
                var entry = MessagePackSerializer.Deserialize<LavLogEntry>(new MemoryStream(data.ToArray()));
#endif

                if (!_eventLogs.TryGetValue(entry.Category, out var eventLog))
                {
                    eventLog = _defaultEventLog;
                }

                var level = entry.Level switch
                {
                    Microsoft.Extensions.Logging.LogLevel.Trace => EventLogEntryType.Information,
                    Microsoft.Extensions.Logging.LogLevel.Debug => EventLogEntryType.Information,
                    Microsoft.Extensions.Logging.LogLevel.Information => EventLogEntryType.Information,
                    Microsoft.Extensions.Logging.LogLevel.Warning => EventLogEntryType.Warning,
                    Microsoft.Extensions.Logging.LogLevel.Error => EventLogEntryType.Error,
                    Microsoft.Extensions.Logging.LogLevel.Critical => EventLogEntryType.Error,
                    _ => EventLogEntryType.Information
                };

                short category = Fnv1aHash(entry.Category);

                // Reuses existing instance if possible
                //EventLog.WriteEntry("Application", entry.Message, level,
                //entry.EventId.Id, 0, new EventDataCollection(entry.State).ToByteArray());

                // Write to event log with structured data
                eventLog.WriteEntry(
                    Truncate(entry.Message, 31839),
                    type: level,
                    eventID: entry.EventId.Id,
                    category,
                    new EventDataCollection(entry.State).ToByteArray());
            }
            catch (Exception ex)
            {
                // Fallback to debug output
                FallbackLogger.LogError($"Event log write failed: {ex.Message}");
            }
        }

        public override async Task SendAsync(byte[] data, CancellationToken ct)
        {
            //await _udpClient.SendAsync(data, data.Length);

            await Task.Delay(0);
        }

        public override async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct)
        {
            //await _udpClient.SendAsync(data.ToArray(), data.Length);
            await Task.Delay(0);
        }

//        protected override void Dispose(bool disposing)
//        {
//            base.Dispose(disposing);

//            //if (!disposing) return;

//#if NETFRAMEWORK

//#endif
//        }
    }
}
