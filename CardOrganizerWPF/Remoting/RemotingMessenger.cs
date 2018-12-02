using System;
using System.Collections.Generic;

namespace CardOrganizerWPF.Remoting
{
    class RemotingMessenger : MarshalByRefObject, IMessenger
    {
        private static Object lockObj = new Object();
        private Dictionary<string, Queue<byte[]>> messages;

        public RemotingMessenger()
        {
            messages = new Dictionary<string, Queue<byte[]>>();
        }

        public void SendMessage(string id, byte[] message)
        {
            lock(lockObj)
            {
                if(messages.TryGetValue(id, out Queue<byte[]> queue))
                {
                    queue.Enqueue(message);
                }
                else
                {
                    var newQueue = new Queue<byte[]>();
                    messages.Add(id, newQueue);
                    newQueue.Enqueue(message);
                }
            }
        }

        public byte[] GetMessage(string id)
        {
            lock(lockObj)
            {
                if(messages.TryGetValue(id, out Queue<byte[]> queue))
                {
                    return queue.Count > 0 ? queue.Dequeue() : null;
                }
            }

            return null;
        }

        public void ClearMessage(string id)
        {
            lock(lockObj)
            {
                if(messages.TryGetValue(id, out Queue<byte[]> queue))
                {
                    queue.Clear();
                }
            }
        }
    }
}
