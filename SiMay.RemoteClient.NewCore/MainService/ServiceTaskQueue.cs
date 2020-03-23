using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SiMay.ServiceCore
{
    public class ServiceTaskQueue : ConcurrentQueue<ApplicationRemoteService>
    {
        public ApplicationRemoteService Dequeue()
        {
            base.TryDequeue(out var service);
            return service;
        }
    }
}
