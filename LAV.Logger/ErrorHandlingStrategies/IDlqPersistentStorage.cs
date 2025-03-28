using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;

namespace LAV.Logger.ErrorHandlingStrategies
{
    public interface IDlqPersistentStorage
    {
        Task StoreAsync(DlqEntry entry);
#if !NET452
        IAsyncEnumerable<DlqEntry> RetrieveAsync(CancellationToken ct);
#endif
        Task RemoveAsync(Guid entryId);
    }
}