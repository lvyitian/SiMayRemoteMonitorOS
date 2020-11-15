using SiMay.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SiMay.RemoteControls.Core
{
    public class ApplicationSyncAwaiter : INotifyCompletion
    {
        private int _id;

        private Action _continuation = null;

        private CallSyncResultPacket _callSyncResultPacket;

        private bool _isCompleted = false;

        private IDictionary<int, ApplicationSyncAwaiter> _asyncOperationSequence;
        public ApplicationSyncAwaiter(IDictionary<int, ApplicationSyncAwaiter> asyncOperationSequence, int id)
        {
            _id = id;
            _asyncOperationSequence = asyncOperationSequence;
            _asyncOperationSequence.Add(_id, this);
        }

        void INotifyCompletion.OnCompleted(Action continuation)
            => _continuation = continuation;


        public ApplicationSyncAwaiter GetAwaiter()
            => this;

        public void Complete(CallSyncResultPacket callSyncResultPacket)
        {
            _isCompleted = true;
            _callSyncResultPacket = callSyncResultPacket;
            _asyncOperationSequence.Remove(_id);
            _continuation?.Invoke();
        }

        public CallSyncResultPacket GetResult()
        {
            return _callSyncResultPacket;
        }

        public bool IsCompleted
            => _isCompleted;
    }
}
