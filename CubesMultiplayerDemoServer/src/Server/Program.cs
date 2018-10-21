using Shared;
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

                            OnMessageReceived(new NetworkMessage(msg.data));
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

        public void OnMessageReceived(NetworkMessage message)
        {
            if (message.Buffer == null)
                return;

            switch(message.GetTagPacket())
            {
                case NetworkTagPacket.PlayerPosition:

                    float x = message.GetFloat();
                    float y = message.GetFloat();
                    float z = message.GetFloat();

                    Console.WriteLine($"Got position packet : {x} | {y} | {z}");

                    break;
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
