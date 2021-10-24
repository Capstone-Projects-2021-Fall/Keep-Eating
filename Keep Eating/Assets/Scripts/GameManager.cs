using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{

    public static GameManager Instance;

    private void Start()
    {
        Instance = this;
    }


    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(1);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }


    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Only Master can load the arena.....");
        }
        PhotonNetwork.LoadLevel("SmallGameMap");
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", newPlayer.NickName);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() isMasterClient {0}", PhotonNetwork.IsMasterClient);

            LoadArena();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", otherPlayer.NickName);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom() isMasterClient {0}", PhotonNetwork.IsMasterClient);

            LoadArena();
        }
    }
}
