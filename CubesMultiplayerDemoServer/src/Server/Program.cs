using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Telepathy;

namespace Server
{
    class Program
    {
        private Telepathy.Server server;
        private Dictionary<long, NetworkPlayer> networkPlayersDictionary;

        public void Run()
        {
            try
            {
                networkPlayersDictionary = new Dictionary<long, NetworkPlayer>();

                server = new Telepathy.Server();
                server.Start(1337);
                
                while (server.Active)
                {
                    // reply to each incoming message
                    Message msg;
                    while (server.GetNextMessage(out msg))
                    {
                        if (msg.eventType == EventType.Connected)
                            OnClientConnected(msg.connectionId);
                        else if (msg.eventType == EventType.Data)
                        {
                            server.Send(msg.connectionId, msg.data);
                            OnMessageReceived(new NetworkMessage(msg.data), msg.connectionId);
                        }
                        else if(msg.eventType == EventType.Disconnected)
                            OnClientDisconnected(msg.connectionId);
                    }

                    SendPlayerPositions();

                    System.Threading.Thread.Sleep(15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void SendPlayerPositions()
        {
            try
            {
                Dictionary<long, NetworkPlayer> sendPosDict = new Dictionary<long, NetworkPlayer>(networkPlayersDictionary);

                foreach (var sendToPlayer in sendPosDict)
                {
                    if (sendToPlayer.Value == null)
                        continue;

                    var message = new NetworkMessage();

                    message.AddTagPacket(NetworkTagPacket.PlayerPositionsArray);
                    message.AddUInt32((uint)sendPosDict.Count(c => c.Key != sendToPlayer.Key && c.Value.Moved));

                    int amountPlayersMoved = 0;

                    foreach (var posPlayers in sendPosDict)
                    {
                        if (sendToPlayer.Key == posPlayers.Key)
                            continue;

                        if (!posPlayers.Value.Moved)
                            continue;

                        message.AddUInt32((uint)posPlayers.Key);
                        
                        message.AddFloat(posPlayers.Value.X);
                        message.AddFloat(posPlayers.Value.Y);
                        message.AddFloat(posPlayers.Value.Z);

                        amountPlayersMoved++;
                    }

                    if (amountPlayersMoved > 0)
                        server.Send(sendToPlayer.Value.ConnectionID, message.Buffer);
                }

                foreach (var player in networkPlayersDictionary)
                    player.Value.Moved = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void OnClientConnected(int connectionId)
        {
            Console.WriteLine($"OnClientConnected: {connectionId}");

            var message = new NetworkMessage();

            message.AddTagPacket(NetworkTagPacket.PlayerPositionsArray);

            message.AddUInt32((uint)networkPlayersDictionary.Count);

            foreach (var player in networkPlayersDictionary)
            {
                message.AddUInt32((uint)player.Key);

                message.AddFloat(player.Value.X);
                message.AddFloat(player.Value.Y);
                message.AddFloat(player.Value.Z);
            }

            server.Send(connectionId, message.Buffer);

            if (!networkPlayersDictionary.ContainsKey(connectionId))
                networkPlayersDictionary.Add(connectionId, new NetworkPlayer(connectionId));

            networkPlayersDictionary[connectionId].Moved = true;
        }

        public void OnClientDisconnected(int connectionId)
        {
            try
            {
                Console.WriteLine($"OnClientDisconnected: {connectionId}");

                if (networkPlayersDictionary.ContainsKey(connectionId))
                    networkPlayersDictionary.Remove(connectionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void OnMessageReceived(NetworkMessage message, int connectionID)
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

                    networkPlayersDictionary[connectionID].X = x;
                    networkPlayersDictionary[connectionID].Y = y;
                    networkPlayersDictionary[connectionID].Z = z;

                    networkPlayersDictionary[connectionID].Moved = true;

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
