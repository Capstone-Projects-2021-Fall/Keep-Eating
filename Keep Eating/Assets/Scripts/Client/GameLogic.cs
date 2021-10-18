using UnityEngine;
#if !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour
{
    public NetworkClient _NetworkClient;

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                _NetworkClient.killGameSession();
                //Debug.Log("Hello");
            }
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (StartupClient.GameStatus == "CONNECTED")
        {
            if (h != 0 || v != 0)
            {
                _NetworkClient.PlayerMove(h, v);
            }
        }
    }

    void SpawnEnemy(){
        
    }
}
#endif