using System;
using Telepathy;

namespace Server
{
    class Program
    {
        private Telepathy.Server server;
        
        public void Run()
        {
            try
            {
                server = new Telepathy.Server();
                server.Start(1337);
                
                while (server.Active)
                {
                    // reply to each incoming message
                    Message msg;
                    while (server.GetNextMessage(out msg))
                    {
                        if (msg.eventType == EventType.Data)
                        {
                            server.Send(msg.connectionId, msg.data);
                        }
                    }

                    System.Threading.Thread.Sleep(15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();

            Console.ReadKey();
        }
    }
}
