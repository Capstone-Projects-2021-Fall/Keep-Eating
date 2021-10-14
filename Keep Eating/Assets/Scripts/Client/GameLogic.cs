using UnityEngine;
#if !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour
{
    public NetworkClient _NetworkClient;

    void Update()
    {
        if (Startup.GameStatus == "STARTED" && Input.GetKeyDown("w"))
        {
            Debug.Log("w key was pressed");
            _NetworkClient.WPressed();
        }
    }

    void SpawnEnemy(){
        
    }
}
#endif