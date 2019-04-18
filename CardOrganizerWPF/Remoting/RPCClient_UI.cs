using MessagePack;
using System;

namespace CardOrganizerWPF.Remoting
{
    static class RPCClient_UI
    {
        private static IMessenger remoteObject;

        public static void Start(string name, int port)
        {
            Type requiredType = typeof(IMessenger);
            remoteObject = (IMessenger)Activator.GetObject(requiredType, $"tcp://localhost:{port}/{name}");
            remoteObject.ClearMessage("");
        }

        public static void SendMessage(MsgObject message)
        {
            var bytes = MessagePackSerializer.Serialize(message);
            remoteObject.SendMessage(message.id, bytes);
        }
    }
}
