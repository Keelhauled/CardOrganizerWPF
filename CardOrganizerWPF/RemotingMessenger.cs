using System;
using System.Collections.Generic;

namespace CardOrganizerWPF
{
    class RemotingMessenger : MarshalByRefObject, IMessenger
    {
        private static Object lockObj = new Object();
        private Dictionary<string, Queue<byte[]>> messages;

        public RemotingMessenger()
        {
            messages = new Dictionary<string, Queue<byte[]>>();
        }

        public void SendMessage(string process, byte[] message)
        {
            lock(lockObj)
            {
                if(messages.TryGetValue(process, out Queue<byte[]> queue))
                {
                    queue.Enqueue(message);
                }
                else
                {
                    var newQueue = new Queue<byte[]>();
                    messages.Add(process, newQueue);
                    newQueue.Enqueue(message);
                }
            }
        }

        public byte[] GetMessage(string process)
        {
            lock(lockObj)
            {
                if(messages.TryGetValue(process, out Queue<byte[]> queue))
                {
                    return queue.Count > 0 ? queue.Dequeue() : null;
                }
            }

            return null;
        }

        public void ClearMessage(string process)
        {
            lock(lockObj)
            {
                if(messages.TryGetValue(process, out Queue<byte[]> queue))
                {
                    queue.Clear();
                }
            }
        }
    }
}
