using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CardOrganizerWPF
{
    class ServerPipe
    {
        static bool stopThread = false;
        static Thread pipeThread;
        static StreamWriter writer;

        public static void StartServer(string pipeName)
        {
            pipeThread = new Thread(() => ClientThread(pipeName));
            pipeThread.IsBackground = true;
            pipeThread.Start();
        }

        public static void StopServer()
        {
            stopThread = false;
        }

        public static void SendMessage(MsgObject message)
        {
            if(writer != null)
            {
                lock(writer)
                {
                    writer.WriteLine(message);
                }
            }
        }

        static void ClientThread(string pipeName)
        {
            var server = new NamedPipeServerStream(pipeName);
            server.WaitForConnection();
            writer = new StreamWriter(server);
            Console.WriteLine("Server get");
        }
    }
}
