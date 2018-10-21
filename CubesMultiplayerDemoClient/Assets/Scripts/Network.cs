using Shared;
using System.Collections.Generic;
using System.Threading;
using Telepathy;
using UnityEngine;

public class Network : MonoBehaviour
{
    public Player player;
    private Vector3 lastNetworkedPosition = Vector3.zero;

    private float lastDistance = 0.0f;
    const float MIN_DISTANCE_TO_SEND_POSITION = 0.01f;

    private Client client;

    public GameObject netPlayerPrefab;
    private Dictionary<long, NetPlayer> netPlayersDictionary;

    // Use this for initialization
    void Start () {
        netPlayersDictionary = new Dictionary<long, NetPlayer>();

        client = new Client();
        client.Connect("127.0.0.1", 1337);

        Thread.Sleep(15);

        if (client.Connected)
            Debug.Log("Client  started!");
        else
            Debug.LogError("Could not start client!");
    }

    private void FixedUpdate()
    {
        if (client != null && client.Connected)
        {
            OnNetworkReceived();

            lastDistance = Vector3.Distance(lastNetworkedPosition, player.transform.position);
            if (lastDistance >= MIN_DISTANCE_TO_SEND_POSITION)
            {
                var message = new NetworkMessage();

                message.AddTagPacket(NetworkTagPacket.PlayerPosition);
                message.AddFloat(player.transform.position.x);
                message.AddFloat(player.transform.position.y);
                message.AddFloat(player.transform.position.z);

                client.Send(message.Buffer);

                lastNetworkedPosition = player.transform.position;
            }
        }

        foreach (var player in netPlayersDictionary)
        {
            if (!player.Value.GameObjectAdded)
            {
                player.Value.GameObjectAdded = true;
                player.Value.GameObject = Instantiate(netPlayerPrefab, player.Value.Position, Quaternion.identity);
            }
            else
                player.Value.GameObject.transform.position = player.Value.Position;
        }
    }

    public void OnNetworkReceived()
    {
        if (client != null && client.Connected)
        {
            Message message;

            while (client.GetNextMessage(out message))
                if (message.eventType == Telepathy.EventType.Data)
                    OnMessageReceived(new NetworkMessage(message.data));
        }
    }

    public void OnMessageReceived(NetworkMessage message)
    {
        if (message.Buffer == null)
            return;

        switch (message.GetTagPacket())
        {
            case NetworkTagPacket.PlayerPositionsArray:

                uint lengthArr = message.GetUInt32();

                Debug.Log($"Got positions array data num : {lengthArr}");

                for (int i = 0; i < lengthArr; i++)
                {
                    long playerid = message.GetUInt32();

                    if (!netPlayersDictionary.ContainsKey(playerid))
                        netPlayersDictionary.Add(playerid, new NetPlayer());

                    netPlayersDictionary[playerid].X = message.GetFloat();
                    netPlayersDictionary[playerid].Y = message.GetFloat();
                    netPlayersDictionary[playerid].Z = message.GetFloat();
                }

                break;
        }
    }

    private void OnApplicationQuit()
    {
        if (client != null)
            if (client.Connected)
                client.Disconnect();
    }
}
