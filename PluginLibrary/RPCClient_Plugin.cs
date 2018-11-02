using System;
using System.Threading;
using MessagePack;

namespace PluginLibrary
{
    static class RPCClient_Plugin
    {
        private static IMessenger remoteObject;
        private static Action<MsgObject> messageAction;

        public static void Start(string name, int port, Action<MsgObject> action)
        {
            messageAction = action;
            Type requiredType = typeof(IMessenger);
            remoteObject = (IMessenger)Activator.GetObject(requiredType, $"tcp://localhost:{port}/{name}");
            remoteObject.Register();
            new Thread(RefreshMessages).Start();
        }

        private static void RefreshMessages()
        {
            bool run = true;

            while(run)
            {
                try
                {
                    var msg = remoteObject.GetMessage();
                    if(msg != null)
                    {
                        var message = MessagePackSerializer.Deserialize<MsgObject>(msg);
                        message.Print();
                        messageAction(message);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    run = false;
                }
                
                Thread.Sleep(100);
            }
        }
    }
}
