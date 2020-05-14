using System;
using System.Collections.Generic;
using System.Text;

namespace SuperWebSocket
{
    internal class WebSocketClientCollection  : System.Collections.IEnumerable
    {
        private List<IWebSocketClient> client_list = new List<IWebSocketClient>();

        internal int Count
        {
            get { return client_list.Count; }
        }

        internal void Add(IWebSocketClient client)
        {
            this.client_list.Add(client);
        }

        internal void Remove(IWebSocketClient item)
        {
            this.client_list.Remove(item);
        }

        internal void RemoveAt(int index)
        {
            this.client_list.RemoveAt(index);
        }

        internal void Clear()
        {
            this.client_list.Clear();
        }

        public IWebSocketClient[] ToArray()
        {
            return this.client_list.ToArray();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            foreach(IWebSocketClient client in client_list)
            {
                yield return client;
            }
        }

        public IWebSocketClient GetWebSocketClient(string Id)
        {
            foreach (IWebSocketClient client in client_list)
            {
                if (client.Id == Id)
                    return client;
            }
            return null;
        }

    }

    internal static class WebSocketClientCollectionPlus
    {
        internal static string ToUsersJson(this WebSocketClientCollection collection)
        {
            int index = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("[");            
            foreach(IWebSocketClient client in collection)
            {
                if (index == 0)
                    sb.Append("{\"id\":\"" + client.Id + "\",\"name\":\"" + client.Name + "\"}");
                else
                    sb.Append(",{\"id\":\"" + client.Id + "\",\"name\":\"" + client.Name + "\"}");
                index++;
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    
}
