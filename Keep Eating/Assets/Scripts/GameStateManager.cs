/*
    Initialized in the map.
    Controls the game (duh). 
    Keeps track of the stuff that causes the game to end.

    TODO: Spawn and respawn items.
                -This can probably be done with a coroutine. 
                -Make object invisible, call coroutine, coroutine waits, coroutine makes the object visible again.
          Spawn and respawn players.
 */



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;            //needed for the PhotonTeamsManager class

namespace Com.tuf31404.KeepEating
{
    public class GameStateManager : MonoBehaviour
    {
        private PhotonTeamsManager teamManager;         //Gives access to team info. Specifically number of players.
        private int eatersDead;
        private int eaterPoints;
        private int pointsToWin;
        private int eaterIndex, enforcerIndex;
        private Text hudText;                           //The text GameObject that displays the time.
        Dictionary<int, Player> players;
        public PlayerManager player;
        [SerializeField]
        PhotonView pv;

        // Start is called before the first frame update
        void Start()
        {
            eatersDead = 0;
            eaterPoints = 0;
            pointsToWin = 100;
            teamManager = GameObject.Find("Team Manager").GetComponent<PhotonTeamsManager>();
            hudText = GameObject.Find("Timer").GetComponent<Text>();

        }

        public void SpawnPlayers()
        {
            Debug.Log("spawn players gsm");
            players = PhotonNetwork.CurrentRoom.Players;
            eaterIndex = 0;
            enforcerIndex = 0;

            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                if (PhotonTeamExtensions.GetPhotonTeam(players[i + 1]).Code == 1)
                {
                    Debug.Log("Eater spawned");
                    pv.RPC("Spawn", RpcTarget.AllBuffered, eaterIndex++, players[i+1].UserId);
                    Debug.Log("UserId = " + players[i + 1].UserId);
                }
                else
                {
                    Debug.Log("Enforcer spawned");
                    pv.RPC("Spawn", RpcTarget.AllBuffered, enforcerIndex++, players[i + 1].UserId);
                }
            }
        }

        // Checks for win conditions.
        void Update()
        {
            if (eatersDead == teamManager.GetTeamMembersCount(1))
            {
                GameOver("Death");
            }
            else if (eaterPoints == pointsToWin)
            {
                GameOver("Points");
            }
        }


        /*
            Called when endgame conition is met.
            Maybe called by Timer class???

            TODO: Timer call, game over process (destroy objects, return to lobby, etc.)
        */
        public void GameOver(string cause)
        {
            switch (cause)
            {
                case "Death":
                    //enforcers win
                    hudText.text = "Enforcers Win";
                    break;
                case "Points":
                    //eaters win
                    hudText.text = "Eaters Win";
                    break;
                case "Time":
                    //tie??
                    hudText.text = "Tie Game";
                    break;
                default:
                    Debug.Log("Oh shit something went wrong");
                    break;
            }


        }


        /*
            TODO: This whole function. Maybe make a food enum instead of using a string.
                  Have the eater call this when they eat something.
        */
        public void AddPoints(string food)
        {

        }

        public void Death()
        {
            eatersDead++;
        }

        public void Respawn()
        {
            eatersDead--;
        }


        [PunRPC]
        public void Spawn(int spawnLoc, string playerId)
        {
            if (playerId.Equals(PhotonNetwork.LocalPlayer.UserId))
            {
                Debug.Log("SpawnRPC");
                player.Spawn(spawnLoc);
            }
            else
            {
                Debug.Log("PlayerID error");
                Debug.Log("player id = " + playerId + " local = " + PhotonNetwork.LocalPlayer.UserId);
            }
        }

    }
}
