using System;
using System.Threading;
using MessagePack;

namespace PluginLibrary
{
    static class RPCClient_Plugin
    {
        private static IMessenger remoteObject;
        private static Action<MsgObject, string> messageAction;
        private static string serverName;
        private static int serverPort;
        private static volatile bool threadRunning = false;
        private static string mainId;
        private static string subId;
        private static string id = "";
        private static object lockObj;

        public static void Init(string name, int port, string mainid, Action<MsgObject, string> action)
        {
            lockObj = new object();
            messageAction = action;
            serverName = name;
            serverPort = port;
            mainId = mainid;

            StartServer();
        }

        public static void ChangeId(string newId)
        {
            lock(lockObj)
            {
                subId = newId;
                id = $"{mainId}_{subId}";
                Console.WriteLine($"[CardOrganizer] Id changed to {newId}");
            }
        }

        public static void StartServer()
        {
            if(!threadRunning)
            {
                new Thread(RefreshMessages).Start();
            }
            else
            {
                Console.WriteLine("[CardOrganizer] Client already running");
            }
        }

        public static void StopServer()
        {
            threadRunning = false;
        }

        public static bool Status()
        {
            return threadRunning;
        }

        private static void RefreshMessages()
        {
            try
            {
                var requiredType = typeof(IMessenger);
                var url = $"tcp://localhost:{serverPort}/{serverName}";
                remoteObject = (IMessenger)Activator.GetObject(requiredType, url);

                threadRunning = true;
                lock(lockObj) remoteObject.ClearMessage(id);
                Console.WriteLine("[CardOrganizer] Starting client");
            }
            catch(Exception ex)
            {
                Console.WriteLine("[CardOrganizer] Server not found");
                Console.WriteLine(ex);
            }

            while(threadRunning)
            {
                try
                {
                    lock(lockObj)
                    {
                        var msg = remoteObject.GetMessage(id);
                        if(msg != null)
                        {
                            var message = MessagePackSerializer.Deserialize<MsgObject>(msg);
                            message.Print();
                            messageAction(message, subId);
                        }
                    }

                    Thread.Sleep(100);
                }
                catch(ArgumentException ex)
                {
                    Console.WriteLine("ERROR: Old bug in MessagePack-CSharp (Duplicate type name within an assembly, issue #127)\n" +
                                      "Must use a fixed Assembly-CSharp-firstpass.dll for this to work with scriptloader in KK\n" + ex);
                    threadRunning = false;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    threadRunning = false;
                }
            }

            Console.WriteLine("[CardOrganizer] Stopping client");
        }
    }
}
