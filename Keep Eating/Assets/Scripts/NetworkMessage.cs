[System.Serializable]
public class NetworkMessage
{
    public string _opCode;
    public string _playerSessionId, _playerId;
    public float _hPos, _vPos;
    public bool _enforcer;

    /*
    //Generic Message
    public NetworkMessage(string opCode, string playerSessionId)
    {
        _opCode = opCode;
        _playerSessionId = playerSessionId;
    }
    */

    //Connection Message
    //Contains boolean to state wheter the player connecting is an enforcer
    public NetworkMessage(string opCode, string playerSessionId, string playerId, float hPos, float vPos, bool enforcer)
    {
        _opCode = opCode;
        _playerSessionId = playerSessionId;
        _playerId = playerId;
        _hPos = hPos;
        _vPos = vPos;
        _enforcer = enforcer;
    }

    /*
    //Movement Message
    public NetworkMessage(string opCode, string playerSessionId, float hPos, float vPos)
    {
        _opCode = opCode;
        _playerSessionId = playerSessionId;
        _hPos = hPos;
        _vPos = vPos;
    }
    */
}