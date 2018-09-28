using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using MessagePack;

namespace CardOrganizerWPF
{
    public class TCPClientManager
    {
        private TcpClient socketConnection;
        private Thread clientReceiveThread;
        private Action<MsgObject> MessageAction;
        
        public TCPClientManager(Action<MsgObject> action)
        {
            MessageAction = action;
            clientReceiveThread = new Thread(ListenForData){ IsBackground = true };
            clientReceiveThread.Start();
        }
        
        private void ListenForData()
        {
            try
            {
                socketConnection = new TcpClient("localhost", 8052);
                Byte[] bytes = new Byte[1024];
                while(true)
                {
                    using(NetworkStream stream = socketConnection.GetStream())
                    {
                        int length;		
                        while((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incomingData = new byte[length];
                            Array.Copy(bytes, 0, incomingData, 0, length);
                            var message = MessagePackSerializer.Deserialize<MsgObject>(incomingData);
                            message.Print(length);

                            switch(message.type)
                            {
                                case MsgObject.Type.Quit:
                                    SendMessage(MsgObject.QuitMsg());
                                    throw new SocketException();

                                case MsgObject.Type.Add:
                                    MessageAction(message);
                                    break;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                if(ex is SocketException || ex is InvalidOperationException || ex is IOException)
                {
                    Console.WriteLine("Server not found, try again");
                    ListenForData();
                    return;
                }

                throw ex;
            }
        }

        public void SendMessage(MsgObject clientMessage)
        {
            if(socketConnection != null)
            {
                try
                {
                    NetworkStream stream = socketConnection.GetStream();
                    if(stream.CanWrite)
                    {
                        byte[] clientMessageAsByteArray = MessagePackSerializer.Serialize(clientMessage);
                        stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                        Console.WriteLine($"SENDING MESSAGE: {clientMessage.type}");
                    }
                }
                catch(Exception ex)
                {
                    if(ex is SocketException || ex is InvalidOperationException)
                    {
                        Console.WriteLine(ex);
                        return;
                    }

                    throw ex;
                }
            }
        }
    } 
}
