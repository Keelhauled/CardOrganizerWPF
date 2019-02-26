using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace PluginLibrary
{
    static class PluginPipe
    {
        static bool stopThread = false;
        static Thread pipeThread;

        public static void StartClient(string pipeName, Action<MsgObject, string> msgAction)
        {
            pipeThread = new Thread(ClientThread);
            pipeThread.IsBackground = true;
            pipeThread.Start();
            Console.WriteLine("[CardOrganizer] StartClient end");
        }

        public static void StopClient()
        {
            stopThread = false;
        }

        public static void SetId(string id)
        {

        }

        static void ClientThread()
        {
            Console.WriteLine("[CardOrganizer] Connecting to server...");
            var client = new NamedPipeClientStream("");
            client.Connect();

            if(client.IsConnected)
            {
                var reader = new StreamReader(client);

                while(!stopThread)
                {
                    Console.WriteLine(reader.ReadLine());
                } 
            }
        }
    }
}
