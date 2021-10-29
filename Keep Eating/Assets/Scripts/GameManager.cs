/*
    Should probably be called Lobby (Room) Manager.
    Controls what happens when players join and leave the room.

    TODO: Maybe implement a game settings jawn to choose the map and set the 
          max players per room, idk.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

namespace Com.tuf31404.KeepEating
{
    public class GameManager : MonoBehaviourPunCallbacks
    {

        public static GameManager Instance;                 //I forget what this is for lol.
        public GameObject playerPrefab;                     //Player prefab has no sprite until player joins a team.
        private const byte playersNeededToStart = 2;
        [SerializeField]
        private GameSettings gameSettings;                  //Not sure if we need this
        [SerializeField]
        private PhotonTeamsManager teamManager;             
        [SerializeField]
        private Button startButton;



        private void Start()
        {
            Instance = this;
            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (PlayerManager.LocalPlayerInstance == null)
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene());
                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                    PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);

                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }

            UnityEngine.UI.Text codeText = GameObject.Find("Lobby Code").GetComponent<UnityEngine.UI.Text>();
            codeText.text = PhotonNetwork.CurrentRoom.Name;         //Lobby code
            DontDestroyOnLoad(this.gameObject);                     //This causes the GameManager object to go to the map
            DontDestroyOnLoad(GameObject.Find("Team Manager"));     //Team Manager object goes to the map
            startButton.onClick.AddListener(() => StartGame());     //Start button listener
        }


        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(1);
        }

        public void LeaveRoom()
        {
            PhotonTeamExtensions.LeaveCurrentTeam(PhotonNetwork.LocalPlayer);
            PhotonNetwork.LeaveRoom();
        }


        void LoadArena()
        {
            //You'll see this IsMasterClient a lot. Some functions should only be called by the Master Client.
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("Only Master can load the arena.....");
            }
            PhotonNetwork.LoadLevel("SmallGameMap");
        }

        /*
            Called when the start button is pressed. 
            Can only be used by the Master Client.
            Checks that there are at least 5 players and at least 1 enforcer.
         */
        public void StartGame()
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= playersNeededToStart && teamManager.GetTeamMembersCount(2) > 0)
            {
                Debug.Log("Starting game");
                //Loads the game map and starts the game.
                LoadArena();
            }
        }


        /*
            I was using this to automatically start the game after enough player joined
            before the start button was implemented.
            Might remove.
         */
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", newPlayer.NickName);

            if (PhotonNetwork.IsMasterClient)
            {
                /*
                Debug.LogFormat("OnPlayerEnteredRoom() isMasterClient {0}", PhotonNetwork.IsMasterClient);
                if (PhotonNetwork.CurrentRoom.PlayerCount >= playersNeededToStart)
                {
                    Debug.Log("Starting game");
                    LoadArena();
                }
                */
            }
        }


        //Not sure why this was ever here
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", otherPlayer.NickName);
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom() isMasterClient {0}", PhotonNetwork.IsMasterClient);

                //LoadArena();
            }
        }

    }
}
