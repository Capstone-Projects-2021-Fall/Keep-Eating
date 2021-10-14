using UnityEngine;
using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.UI;
#if !UNITY_SERVER
// Handles the connection to the server to send and receive messages
public class NetworkClient : MonoBehaviour
{
    private static int MaxMessageSize = 1024;
    private Telepathy.Client _client = new Telepathy.Client(MaxMessageSize);
    private string _playerSessionId;
    public Text _statusText;
    public List<GameObject> eaters = new List<GameObject>();
    public List<GameObject> enforcers = new List<GameObject>();
    public List<GameObject> food = new List<GameObject>();
    private int enforcerNum = 0;
    private int playerNum = 0;

    private void ProcessMessage(NetworkMessage networkMessage)
    {
        if (networkMessage._opCode == "CONNECTED")
        {
            Debug.Log("Connection to server confirmed.");
            StartupClient.GameStatus = "CONNECTED";
        }
        else if (networkMessage._opCode == "NEW_PLAYER")
        {
            Debug.Log("New Player Joined");
            StartupClient.GameStatus = "STARTED";
        }
        else if (networkMessage._opCode == "START")
        {
            Debug.Log("Game has started.");
            StartupClient.GameStatus = "STARTED";
        }
        else if (networkMessage._opCode == "POSITION_CHANGED")
        {
            Debug.Log("Winner");
            GameEnded("GAME OVER - WINNER");
        }
        else
        {
            Debug.LogWarning("Unknown message type received.");
        }
        _statusText.text = StartupClient.GameStatus;
    }

    private void OnDataReceived(ArraySegment<byte> message)
    {
        Debug.Log("OnDataReceived");

        string convertedMessage = Encoding.UTF8.GetString(message.Array, 0, message.Count);
        Debug.Log("Converted message: " + convertedMessage);
        NetworkMessage networkMessage = JsonConvert.DeserializeObject<NetworkMessage>(convertedMessage);

        ProcessMessage(networkMessage);
    }

    public void WPressed()
    {
        NetworkMessage networkMessage = new NetworkMessage("W", _playerSessionId, 0.0f, 0.0f, false);
        Send(networkMessage);
    }

    private void OnConnected()
    {
        Debug.Log("Client Connected");
        NetworkMessage networkMessage = new NetworkMessage("CONNECT", _playerSessionId, 0.0f, 0.0f, false);
        Send(networkMessage);

        Debug.Log("after send message");
    }

    public void Send(NetworkMessage networkMessage)
    {
        var data = JsonConvert.SerializeObject(networkMessage);
        var encoded = Encoding.UTF8.GetBytes(data);
        var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);

        Debug.Log("send message");
        _client.Send(buffer);
    }

    private void OnDisconnected()
    {
        Debug.Log("Client Disconnected");
        GameEnded("GAME OVER - Player Disconnected");
    }

    private void GameEnded(string gameOverMessage)
    {
        StartupClient.GameStatus = gameOverMessage;
    }

    private void NewPlayerConnected()
    {

    }

    void Awake()
    {
        Debug.Log("BADNetworkClient Awake");

        Application.runInBackground = true;

        _client.OnConnected = OnConnected;
        _client.OnData = OnDataReceived;
        _client.OnDisconnected = OnDisconnected;
    }

    public void ConnectToServer(string ip, int port, string playerSessionId)
    {
        Debug.Log($"BADNetworkClient ConnectToServer {ip}, {port}, {playerSessionId}");
        _playerSessionId = playerSessionId;

        // had to set these to 0 or else the TCP connection would timeout after the default 5 seconds.  
        // TODO: Investivate further.
        _client.SendTimeout = 0;
        _client.ReceiveTimeout = 0;

        _client.Connect(ip, port);
    }

    void Update()
    {
        // tick to process messages, (even if not connected so we still process disconnect messages)
        _client.Tick(100);
    }

    void OnApplicationQuit()
    {
        // the client/server threads won't receive the OnQuit info if we are
        // running them in the Editor. they would only quit when we press Play
        // again later. this is fine, but let's shut them down here for consistency
        _client.Disconnect();
    }

}
#endif