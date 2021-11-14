using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyStartup : MonoBehaviour
{

    [SerializeField]
    private GameObject GameManagerPrefab, TeamManagerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("Team Manager(Clone)") == null)
        {
            DontDestroyOnLoad(Instantiate(TeamManagerPrefab));
        }
        if (GameObject.Find("Game Manager(Clone)") == null)
        {
            Instantiate(GameManagerPrefab);
        }
    }
}
