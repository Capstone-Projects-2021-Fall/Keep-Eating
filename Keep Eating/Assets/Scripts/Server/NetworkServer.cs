#if UNITY_SERVER
using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

public class NetworkServer : MonoBehaviour
{
    private static int MaxMessageSize = 1024;
    //private static int MaxPlayersPerSession = 10;
    private static int MinPlayersPerSession = 5;
    private int enforcerNum = 0;
    private Telepathy.Server _server = new Telepathy.Server(MaxMessageSize);
    private Dictionary<int, string> _playerSessions;
    public Dictionary<string, GameObject> _players;
    private GameLiftServer _gameLiftServer;
    public string GameSessionState = "";
    public string GameOverState = "GAME_OVER";

    private void OnDataReceived(int connectionId, ArraySegment<byte> message)
    {
        Debug.Log("Data received from connectionId: " + connectionId);

        string convertedMessage = Encoding.UTF8.GetString(message.Array, 0, message.Count);
        Debug.Log("Converted message: " + convertedMessage);
        NetworkMessage networkMessage = JsonConvert.DeserializeObject<NetworkMessage>(convertedMessage);

        ProcessMessage(connectionId, networkMessage);
    }

    private void ProcessMessage(int connectionId, NetworkMessage networkMessage)
    {
        Debug.Log("Network message: " + networkMessage);

        if (networkMessage != null && networkMessage._opCode != null)
        {
            Debug.Log("processing opcode");

            if (networkMessage._opCode == "CONNECT")
            {
                Debug.Log("CONNECT OP CODE HIT");
                HandleConnect(connectionId, networkMessage._playerSessionId);

                // send response
                NetworkMessage responseMessage = new NetworkMessage("CONNECTED", networkMessage._playerSessionId, networkMessage._playerId, 0.0f, 0.0f, false);
                SendMessage(connectionId, responseMessage);

                GameObject newPlayer = InitPlayerObject(networkMessage._playerId);

                //Initializes all existing player GameObjects on the  newPlayer client
                foreach (KeyValuePair<string, GameObject> player in _players)
                {
                    NetworkMessage responseMessage2;
                    if (player.Value.tag.Equals("Enforcer")){
                        responseMessage2 = new NetworkMessage("NEW_PLAYER", "", player.Key, player.Value.transform.position.x, player.Value.transform.position.y, true);
                    }
                    else {
                        responseMessage2 = new NetworkMessage("NEW_PLAYER", "", player.Key, player.Value.transform.position.x, player.Value.transform.position.y, false);
                    }
                    SendMessage(connectionId, responseMessage2);
                }

                //Initializes the newPlayer GameObject on the other clients
                foreach (KeyValuePair<int, string> playerSession in _playerSessions)
                {
                    NetworkMessage responseMessage3 = new NetworkMessage("NEW_PLAYER", networkMessage._playerSessionId, networkMessage._playerId, newPlayer.transform.position.x, newPlayer.transform.position.y, false);
                    SendMessage(playerSession.Key, responseMessage3);
                }

                CheckAndSendGameReadyToStartMsg(connectionId);

            }
            else if (networkMessage._opCode == "PLAYER_MOVED")
            {
                float speed = 5f;
                Vector3 newPos = _players[networkMessage._playerId].transform.position;

                newPos.x = networkMessage._hPos * speed * Time.deltaTime;
                newPos.y = networkMessage._vPos * speed * Time.deltaTime;
                _players[networkMessage._playerId].transform.position = newPos;
                
                //sends playerId's new position to every client
                foreach (KeyValuePair<int, string> playerSession in _playerSessions)
                {
                    NetworkMessage responseMessage = new NetworkMessage("POSITION_CHANGED", networkMessage._playerSessionId, networkMessage._playerId, newPos.x, newPos.y, false);
                    SendMessage(playerSession.Key, responseMessage);
                }
            }
            else if (networkMessage._opCode == "KILL")
            {
                CheckForGameOver(connectionId);
            }

            // can handle additional opCods here

        }
        else
        {
            Debug.Log("ProcessMessage: empty message or null opCode, message ignored.");
        }
    }

    public GameObject InitPlayerObject(string playerId){
        bool isEnforcer = false;
        if (enforcerNum == 0){
            int enforcer = UnityEngine.Random.Range(0,1);
            if (enforcer == 1){
                isEnforcer = true;
            }
        }

        int rand = UnityEngine.Random.Range(1, 4);
        Vector3 spawnPos = new Vector3 (0,0,0);

        switch (rand){
            case 1:
                spawnPos = GameObject.Find("Spawn1").transform.position;
                break;
            case 2:
                spawnPos = GameObject.Find("Spawn2").transform.position;
                break;
            case 3:
                spawnPos = GameObject.Find("Spawn3").transform.position;
                break;
            case 4:
                spawnPos = GameObject.Find("Spawn4").transform.position;
                break;
        }

        GameObject spawner = GameObject.Find("InstantiatePlayer");
        GameObject newPlayer = spawner.GetComponent<InstantiatePlayer>().NewPlayer(isEnforcer, spawnPos);
        _players.Add(playerId, newPlayer);
        return newPlayer;

    }

    public void SendMessage(int connectionId, NetworkMessage networkMessage)
    {
        var data = JsonConvert.SerializeObject(networkMessage);
        var encoded = Encoding.UTF8.GetBytes(data);
        var asWriteBuffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);

        Debug.Log("send message to playerSessionId: " + networkMessage._playerSessionId + ", with connId: " + connectionId);
        _server.Send(connectionId, asWriteBuffer);
    }

    private void CheckAndSendGameReadyToStartMsg(int connectionId)
    {
        if (_playerSessions.Count == MinPlayersPerSession)
        {
            Debug.Log("Game is full and is ready to start.");

            // tell all players the game is ready to start
            foreach (KeyValuePair<int, string> playerSession in _playerSessions)
            {
                GameSessionState = "STARTED";
                NetworkMessage responseMessage = new NetworkMessage("START", playerSession.Value, "", 0.0f, 0.0f, false);
                SendMessage(playerSession.Key, responseMessage);
            }
        }
    }

    public bool HandleConnect(int connectionId, string playerSessionId)
    {
        Debug.Log("HandleConnect");

        var outcome = _gameLiftServer.AcceptPlayerSession(playerSessionId);
        if (outcome.Success)
        {
            Debug.Log("PLAYER SESSION VALIDATED");
        }
        else
        {
            Debug.Log("PLAYER SESSION REJECTED. AcceptPlayerSession() returned " + outcome.Error.ToString());
        }

        // track our player sessions
        _playerSessions.Add(connectionId, playerSessionId);

        return outcome.Success;
    }

    private void CheckForGameOver(int fromConnectionId)
    {
        if (GameSessionState != GameOverState)
        {
            GameSessionState = GameOverState;
            foreach (KeyValuePair<int, string> playerSession in _playerSessions)
            {
                // send out the win/lose status to all players
                NetworkMessage responseMessage;
                if (playerSession.Key == fromConnectionId)
                {
                    responseMessage = new NetworkMessage("WIN", playerSession.Value, "", 0.0f, 0.0f, false);
                }
                else
                {
                    responseMessage = new NetworkMessage("LOSE", playerSession.Value, "", 0.0f, 0.0f, false);
                }

                SendMessage(playerSession.Key, responseMessage);

                _gameLiftServer.RemovePlayerSession(playerSession.Value); // player session id

                _server.Disconnect(playerSession.Key);
            }

            Debug.Log($"Ending game, player with connection Id {fromConnectionId} hit W first.");

            _gameLiftServer.HandleGameEnd();
        }
        else
        {
            Debug.Log("CheckForGameOver: Game over already being processed.");
        }
    }

    // For the sake of simplicity for this demo, if any player disconnects, just end the game. 
    // That means if only one player joins, then disconnects, the game session ends.
    // Your game may remain open to receiving new players, without ending the game session, up to you.
    private void EndGameAfterDisconnect(int disconnectingId)
    {
        Debug.Log("CheckForGameEnd");

        // TODO: also probably check state of game here or something?

        if (GameSessionState != GameOverState)
        {
            GameSessionState = GameOverState;

            // For this demo game, just disconnect everyone else in the session when one player disconnects. 
            // An all or nothing type of game. And at this point, since the game session will be ending, we don't 
            // need to worry about removing playerSessions from the _playerSessions Dictonary.
            foreach (KeyValuePair<int, string> playerSession in _playerSessions)
            {
                // disconnect all other clients
                if (playerSession.Key != disconnectingId)
                {
                    _server.Disconnect(playerSession.Key);
                }

                _gameLiftServer.RemovePlayerSession(playerSession.Value); // player session id
            }

            Debug.Log("Ending game, player disconnected.");
            _gameLiftServer.HandleGameEnd();
        }
        else
        {
            Debug.Log("EndGameAfterDisconnect: Disconnecting game over is already being processed.");
        }
    }

    private void OnDisonnected(int connectionId)
    {
        Debug.Log("Connection ID: " + connectionId + " Disconnected.");

        EndGameAfterDisconnect(connectionId);
    }

    private void OnConnected(int connectionId)
    {
        Debug.Log("Connection ID: " + connectionId + " Connected");
    }

    public void StartTCPServer(int port)
    {
        // had to set these to 0 or else the TCP connection would timeout after the default 5 seconds.  Investivate further.
        _server.SendTimeout = 0;
        _server.ReceiveTimeout = 0;

        _server.Start(port);
    }

    void Awake()
    {
        _playerSessions = new Dictionary<int, string>();
        _players = new Dictionary<string, GameObject>();
        _gameLiftServer = GetComponent<GameLiftServer>();

        Application.runInBackground = true;

        _server.OnConnected = OnConnected;
        _server.OnData = OnDataReceived;
        _server.OnDisconnected = OnDisonnected;
    }

    void Update()
    {
        // tick to process messages, (even if not active so we still process disconnect messages)
        _server.Tick(100);
    }

    void OnApplicationQuit()
    {
        Debug.Log("BADNetworkServer.OnApplicationQuit");
        _server.Stop();
    }
}
#endif