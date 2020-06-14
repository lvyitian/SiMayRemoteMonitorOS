using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.ServiceCore
{
    public class EventList<T> : List<T>
    {
        int _num = 0;
        int _count;
        public event Action<List<T>,T[]> Notify;
        public EventList(int count)
            => _count = count;
        public void EAdd(T item)
        {
            base.Add(item);
            _num++;
            if (_num >= _count)
            {
                Notify?.Invoke(this, this.ToArray());
                this.Clear();
            }
        }
    }
}
