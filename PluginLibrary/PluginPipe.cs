using System;
using System.IO;
using System.Threading;
using BepInEx.Logging;
using Logger = BepInEx.Logger;
using CustomPipes;
using System.Text;

namespace PluginLibrary
{
    static class PluginPipe
    {
        public static void StartClient(string pipeName, Action<MsgObject, string> msgAction)
        {
            var pipeThread = new Thread(() => PipeThread(pipeName));
            pipeThread.IsBackground = true;
            pipeThread.Start();
        }

        static void PipeThread(string pipeName)
        {
            try
            {
                Console.WriteLine("Trying to connect to server...");
                var stream = new NamedPipeStream($@"\\.\pipe\{pipeName}", FileAccess.ReadWrite);
                var reader = new StreamReader(stream, Encoding.Unicode);
                Console.WriteLine("Connected");

                while(true)
                {
                    while(stream.DataAvailable)
                    {
                        string s = reader.ReadLine();
                        if(s != null && s.Length > 0)
                        {
                            Console.WriteLine(s);
                        }
                    }

                    Thread.Sleep(10);
                }
            }
            catch(Exception ex)
            {
                Logger.Log(LogLevel.Error, ex);
            }
        }
    }
}
