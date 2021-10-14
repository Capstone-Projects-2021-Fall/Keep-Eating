[System.Serializable]
public class NetworkMessage
{
    public string _opCode;
    public string _playerSessionId;
    public float _hPos, _vPos;
    public bool _enforcer = false;


    //Generic Message
    public NetworkMessage(string opCode, string playerSessionId)
    {
        _opCode = opCode;
        _playerSessionId = playerSessionId;
    }

    //Connection Message
    //Contains boolean to state wheter the player connecting is an enforcer
    public NetworkMessage(string opCode, string playerSessionId, string enforcer, float hPos, float vPos)
    {
        _opCode = opCode;
        _playerSessionId = playerSessionId;
        
        if (enforcer.Equals("Enforcer"))
        {
            _enforcer = true;
        }
        else
        {
            _enforcer = false;
        }
    }

    //Movement Message
    public NetworkMessage(string opCode, string playerSessionId, float hPos, float vPos)
    {
        _opCode = opCode;
        _playerSessionId = playerSessionId;
        _hPos = hPos;
        _vPos = vPos;
    }
}