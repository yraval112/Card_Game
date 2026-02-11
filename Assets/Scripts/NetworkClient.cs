using System;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;


public class NetworkClient : MonoBehaviour
{
    public SocketIOUnity socket;

    void Start()
    {
        var uri = new Uri("http://192.168.1.64:3000");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", "UNITY" }
            },
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Connected to server");
        };
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("Disconnected from server");
        };
        socket.OnError += (sender, e) =>
        {
            Debug.LogError($"Socket error: {e}");
        };

        socket.Connect();
    }
}
