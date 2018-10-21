using System.Threading;
using Telepathy;
using UnityEngine;

public class Network : MonoBehaviour
{
    private Client client;
    
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

    // Update is called once per frame
    void Update () {
		
	}
}
