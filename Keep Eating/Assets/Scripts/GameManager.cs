
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
        public GameObject playerPrefab;                     //Change to PlayerV2
        private const byte playersNeededToStart = 2;
        [SerializeField]
        private GameSettings gameSettings;                  //Not sure if we need this
        [SerializeField]
        private PhotonTeamsManager teamManager;             
        [SerializeField]
        private Button startButton;


        private void Start()
        {
            teamManager = GameObject.Find("Team Manager(Clone)").GetComponent<PhotonTeamsManager>();
            startButton = GameObject.Find("Start Button").GetComponent<Button>();
            gameSettings = GameObject.FindGameObjectWithTag("SettingsButton").GetComponent<GameSettings>();

            Instance = this;
            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (PlayerManagerV2.LocalPlayerInstance == null)
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene());
                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                    Debug.Log("Instantiating a player");
                    PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);

                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }

            UnityEngine.UI.Text codeText = GameObject.Find("Lobby Code").GetComponent<UnityEngine.UI.Text>();
            codeText.text = PhotonNetwork.CurrentRoom.Name;         //Lobby code
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
            if (!StaticSettings.Map.Equals(""))
            {
                PhotonNetwork.LoadLevel(StaticSettings.Map);
            }
        }

        /*
            Called when the start button is pressed. 
            Can only be used by the Master Client.
            Checks that there are at least 5 players and at least 1 enforcer.
         */
        public void StartGame()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (StaticSettings.Bots)
                {
                    Debug.Log("Starting game");
                    //Loads the game map and starts the game.
                    LoadArena();
                }
                else if (PhotonNetwork.CurrentRoom.PlayerCount >= playersNeededToStart && teamManager.GetTeamMembersCount(1) > 0 && teamManager.GetTeamMembersCount(2) > 0)
                {
                    Debug.Log("Starting game");
                    //Loads the game map and starts the game.
                    LoadArena();
                }
            }
        }

        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);

        }

        void CalledOnLevelWasLoaded(int sceneNum)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            if (startButton == null)
            {
                startButton = GameObject.Find("Start Button").GetComponent<Button>();
                startButton.onClick.AddListener(() => StartGame());
            }
        }

    }
}
