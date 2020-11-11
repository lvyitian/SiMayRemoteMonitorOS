using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Core
{
    public class SyncOperationHelper
    {
        public static ApplicationSyncAwaiter SyncAwait(IDictionary<int, ApplicationSyncAwaiter> asyncOperationSequence, int id) 
            => new ApplicationSyncAwaiter(asyncOperationSequence, id);
    }
}
