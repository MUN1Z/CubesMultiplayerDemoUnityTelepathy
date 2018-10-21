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

        //foreach (var player in netPlayersDictionary)
        //{
        //    if (!player.Value.GameObjectAdded)
        //    {
        //        player.Value.GameObjectAdded = true;
        //        player.Value.GameObject = Instantiate(netPlayerPrefab, player.Value.Position, Quaternion.identity);
        //    }
        //    else
        //        player.Value.GameObject.transform.position = player.Value.Position;
        //}
    }

    public void OnMessageReceived()
    {
        if (client != null && client.Connected)
        {
            Message message;

            while (client.GetNextMessage(out message))
            {

            }
        }
    }

    private void OnApplicationQuit()
    {
        if (client != null)
            if (client.Connected)
                client.Disconnect();
    }

    // Update is called once per frame
    void Update () {
       
    }
}
