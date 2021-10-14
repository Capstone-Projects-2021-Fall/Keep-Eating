using UnityEngine;
#if !UNITY_SERVER

public class StartupClient : MonoBehaviour
{
    
    public static string GameStatus = "LOADING";


    void Start()
    {
        GameLiftClient gameLiftClient = new GameLiftClient();
    }

}
#endif