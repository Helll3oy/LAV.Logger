using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
#if !NET452
using System.Text.Json;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace LAV.Logger.ErrorHandlingStrategies
{
    /// <summary>
    /// Persistent Storage Implementation (File System)
    /// </summary>
    public class FileDlqStorage : IDlqPersistentStorage
    {
        private readonly string _storagePath;

        public FileDlqStorage(string storagePath)
        {
            _storagePath = storagePath;
            Directory.CreateDirectory(storagePath);
        }

        public async Task StoreAsync(DlqEntry entry)
        {
            var path = GetFilePath(entry.Id);
            using var stream = File.Create(path);
#if !NET452
            await JsonSerializer.SerializeAsync(stream, entry);
#endif
        }

#if !NET452
        public async IAsyncEnumerable<DlqEntry> RetrieveAsync([EnumeratorCancellation] CancellationToken ct)
        {
            foreach (var file in Directory.EnumerateFiles(_storagePath, "*.json"))
            {
                using var stream = File.OpenRead(file);
                yield return await JsonSerializer.DeserializeAsync<DlqEntry>(stream, cancellationToken: ct);
            }
        }
#endif

        public Task RemoveAsync(Guid entryId)
        {
            File.Delete(GetFilePath(entryId));
#if !NET452
            return Task.CompletedTask;
#else
            return TaskHelpers.CompletedTask;
#endif
        }

        private string GetFilePath(Guid id) => Path.Combine(_storagePath, $"{id}.json");
    }
}
