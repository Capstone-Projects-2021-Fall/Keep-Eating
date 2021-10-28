using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

namespace Com.tuf31404.KeepEating
{
    public class GameManager : MonoBehaviourPunCallbacks
    {

        public static GameManager Instance;
        public GameObject playerPrefab;
        private const byte playersNeededToStart = 2;
        [SerializeField]
        private GameSettings gameSettings;


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
            codeText.text = PhotonNetwork.CurrentRoom.Name;
            DontDestroyOnLoad(GameObject.Find("Team Manager"));
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
                if (PhotonNetwork.CurrentRoom.PlayerCount >= playersNeededToStart)
                {
                    Debug.Log("Starting game");
                    LoadArena();
                }
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
}
