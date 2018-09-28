using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using MessagePack;

public class TCPServerManager : MonoBehaviour
{
    public static TCPServerManager Instance;
    public Action<MsgObject> MessageAction;

    private TcpListener tcpListener;
    private Thread tcpListenerThread;
    private TcpClient connectedTcpClient;

    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        tcpListenerThread = new Thread(ListenForIncomingRequests){ IsBackground = true };
        tcpListenerThread.Start();
    }

    void OnDestroy()
    {
        SendMessage(MsgObject.QuitMsg());
    }
    
    private void ListenForIncomingRequests()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8052);
            tcpListener.Start();
            Console.WriteLine("Server is listening");
            Byte[] bytes = new Byte[1024];
            bool loop = true;
            while(loop)
            {
                loop = true;
                using(connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    Console.WriteLine("Client was found");
                    using(NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        Console.WriteLine("Client stream was found");

                        int length = 0;
                        while(loop && (length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incomingData = new byte[length];
                            Array.Copy(bytes, 0, incomingData, 0, length);
                            var message = MessagePackSerializer.Deserialize<MsgObject>(incomingData);
                            message.Print(length);

                            switch(message.type)
                            {
                                case MsgObject.Type.Quit:
                                    loop = false;
                                    Console.WriteLine("Connection to client closed gracefully. Restarting listener");
                                    break;

                                case MsgObject.Type.Use:
                                    UnityMainThreadDispatcher.Instance.Enqueue(() => MessageAction(message));
                                    break;
                            }
                        }
                    }
                }
            }
        }
        catch(Exception ex)
        {
            if(ex is SocketException || ex is InvalidOperationException)
            {
                Console.WriteLine("Connection to client was closed forcefully. Restarting listener.");
                tcpListener.Stop();
                ListenForIncomingRequests();
                return;
            }

            throw ex;
        }
    }

    public void SendMessage(MsgObject message)
    {
        if(connectedTcpClient != null)
        {
            try
            {
                NetworkStream stream = connectedTcpClient.GetStream();
                if(stream.CanWrite)
                {
                    byte[] serverMessageAsByteArray = MessagePackSerializer.Serialize(message);
                    stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                    Console.WriteLine($"SENDING MESSAGE: {message.type}");
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
