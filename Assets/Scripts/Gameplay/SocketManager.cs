using SocketIOClient;
using UnityEngine;

public class GameSocketManager : MonoBehaviour
{
    public static GameSocketManager Instance;
    public SocketIO socket;
    public string playerName;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async void Connect(string name)
    {
        playerName = name;
        socket = new SocketIO("http://192.168.1.64:3000");

        socket.OnConnected += async (sender, e) =>
        {
            Debug.Log("Connected to server");
            await socket.EmitAsync("findMatch");
        };

        socket.On("message", OnMessageReceived);
        await socket.ConnectAsync();
    }

    void OnMessageReceived(SocketIOResponse response)
    {
        Debug.Log("Message received: " + response.GetValue().ToString());
        string json = response.GetValue().ToString();
        NetworkRouter.Handle(json);
    }

    public async void Send(string eventName, object data)
    {
        await socket.EmitAsync(eventName, data);
    }
}
