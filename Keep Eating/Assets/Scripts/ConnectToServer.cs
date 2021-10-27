/*
    Used by the Connect To Server GameObject on the Loading scene.
    Pretty much just connects to a Photon server. 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

namespace Com.tuf31404.KeepEating
{
    public class ConnectToServer : MonoBehaviourPunCallbacks
    {

        string gameVersion = "1";

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
            Debug.Log("Start");
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
            Debug.Log("Connect Master");
        }

        public override void OnJoinedLobby()
        {
            SceneManager.LoadScene("Main Menu");
            Debug.Log("Join Lobby");
        }
    }
}
