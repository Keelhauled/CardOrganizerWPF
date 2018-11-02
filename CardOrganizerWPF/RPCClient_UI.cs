using System;
using MessagePack;

namespace CardOrganizerWPF
{
    static class RPCClient_UI
    {
        private static IMessenger remoteObject;
        private static int id;

        public static void Start(string name, int port)
        {
            Type requiredType = typeof(IMessenger);
            remoteObject = (IMessenger)Activator.GetObject(requiredType, $"tcp://localhost:{port}/{name}");
            id = remoteObject.Register();
        }

        public static void SendMessage(MsgObject message)
        {
            var bytes = MessagePackSerializer.Serialize(message);
            remoteObject.SendMessage(id, bytes);
        }
    }
}
