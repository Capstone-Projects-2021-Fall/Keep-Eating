[System.Serializable]
public class NetworkMessage
{
    public string _opCode;
    public string _playerSessionId;
    public float _hPos, _vPos;
    public bool _enforcer = false;

    public NetworkMessage(string opCode, string playerSessionId, float hPos, float vPos, bool enforcer)
    {
        _opCode = opCode;
        _playerSessionId = playerSessionId;
        _hPos = hPos;
        _vPos = vPos;
        _enforcer = enforcer;
    }
}