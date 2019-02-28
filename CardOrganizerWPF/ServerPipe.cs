using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomPipes;

namespace CardOrganizerWPF
{
    class ServerPipe
    {
        static Queue<MsgObject> messages = new Queue<MsgObject>();

        public static void StartServer(string pipeName)
        {
            var pipeThread = new Thread(() => PipeThread(pipeName));
            pipeThread.IsBackground = true;
            pipeThread.Start();
        }

        public static void SendMessage(MsgObject message)
        {
            lock(messages)
            {
                Console.WriteLine($"Enqueue message: {message.path}");
                messages.Enqueue(message);
            }
        }

        static void PipeThread(string pipeName)
        {
            var stream = NamedPipeStream.Create(pipeName, NamedPipeStream.ServerMode.Bidirectional);
            var writer = new StreamWriter(stream, Encoding.Unicode);

            while(true)
            {
                Console.WriteLine("Listening...");
                stream.Listen();
                Console.WriteLine("Client found");

                while(stream.IsConnected)
                {
                    lock(messages)
                    {
                        if(messages.Count > 0)
                        {
                            var path = messages.Dequeue().path;
                            writer.WriteLine(path);
                            writer.Flush();
                            Console.WriteLine($"Writing message: {path}");
                        }
                    }

                    Thread.Sleep(10);
                }
            }
        }
    }
}
