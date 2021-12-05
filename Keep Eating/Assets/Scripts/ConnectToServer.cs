/*
    This is the first script called !!!!!
    
    Used by the Connect To Server GameObject on the Loading scene.
    Pretty much just connects to a Photon server.

    PhotonNetwork is very important. Used to connect to and get info from the server.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

namespace Com.tuf31404.KeepEating
{
    public class ConnectToServer : MonoBehaviourPunCallbacks      //needed for PUN functions
    {
        
        string gameVersion = "2";                                  //needed so you are connected to a compatible lobby
        bool isConsole = false;

        //Awake is always the first function called
        private void Awake()
        {
            //This automatically syncs scene changes, i.e., on the Master Client calls to change scenes.
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
            Debug.Log("Start");
        }

        //These last two functions are PUN Callbacks
        public override void OnConnectedToMaster()
        {
            //A Photon "Lobby" is not the same as what we refer to as a lobby.
            //They use the term Room.
            PhotonNetwork.JoinLobby();
            Debug.Log("Connect Master");
        }

        public override void OnJoinedLobby()
        {
            if (isConsole)
            {
                SceneManager.LoadScene("LoginPage");
            }
            else
            {
                SceneManager.LoadScene("Main Menu");
            }
            Debug.Log("Join Lobby");
        }
    }
}
