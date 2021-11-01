using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    //int lobbySize, eaterNum, enforcerNum;
    //string map;


    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(this.gameObject);
    }

    public int LobbySize { get; set; }

    public int EaterNum { get; set; }

    public int EnforcerNum { get; set; }

    public string Map { get; set; }


}
