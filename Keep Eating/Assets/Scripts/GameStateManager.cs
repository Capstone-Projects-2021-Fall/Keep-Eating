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
    public class GameStateManager : MonoBehaviourPunCallbacks
    {
        private PhotonTeamsManager teamManager;         //Gives access to team info. Specifically number of players.
        private int eatersDead;
        private int eaterPoints;
        private int pointsToWin;
        private int eaterIndex, enforcerIndex;
        public int eaterCount;
        private Vector3[] foodSpawns;
        private Text hudText;                           //The text GameObject that displays the time.
        private Text eaterScoreText;
        private Text eatersAliveText;
        Dictionary<int, Player> players;
        public PlayerManager player;
        [SerializeField]
        PhotonView pv;

        private void Awake()
        {
            foodSpawns = new Vector3[5];
            foodSpawns[0] = GameObject.Find("FoodSpawn").transform.position;
            foodSpawns[1] = GameObject.Find("FoodSpawn (1)").transform.position;
            foodSpawns[2] = GameObject.Find("FoodSpawn (2)").transform.position;
            foodSpawns[3] = GameObject.Find("FoodSpawn (3)").transform.position;
            foodSpawns[4] = GameObject.Find("FoodSpawn (4)").transform.position;
        }
        // Start is called before the first frame update
        void Start()
        {
            eatersDead = 0;
            eaterPoints = 0;
            pointsToWin = 100;
            teamManager = GameObject.Find("Team Manager").GetComponent<PhotonTeamsManager>();
            hudText = GameObject.Find("Timer").GetComponent<Text>();
            eaterScoreText = GameObject.Find("Eater Score").GetComponent<Text>();
            eatersAliveText = GameObject.Find("Eaters Alive").GetComponent<Text>();
            eatersAliveText.text = "Eaters Alive: " + teamManager.GetTeamMembersCount(1);
        }

        public void SpawnPlayers()
        {
            players = PhotonNetwork.CurrentRoom.Players;
            eaterIndex = 0;
            enforcerIndex = 0;

            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                if (PhotonTeamExtensions.GetPhotonTeam(players[i + 1]).Code == 1)
                {
                    pv.RPC("Spawn", RpcTarget.AllBuffered, eaterIndex++, players[i+1].UserId);
                    Debug.Log("UserId = " + players[i + 1].UserId);
                }
                else
                {
                    pv.RPC("Spawn", RpcTarget.AllBuffered, enforcerIndex++, players[i + 1].UserId);
                }
            }
        }

        public void SpawnFood()
        {
            string food = "Food";
            int rand;
            foreach(Vector3 pos in foodSpawns)
            {
                rand = UnityEngine.Random.Range(1, 4);
                food += rand;
                PhotonNetwork.Instantiate(food, pos, Quaternion.identity);
                food = "food";
            }
        }

        public void SpawnWeapons()
        {
            int rand = UnityEngine.Random.Range(1, 3);
            if (rand == 1)
            {
                PhotonNetwork.Instantiate("Revolver", GameObject.Find("WeaponSpawn").transform.position, Quaternion.identity);
            }
            else
            {
                PhotonNetwork.Instantiate("Shotgun", GameObject.Find("WeaponSpawn").transform.position, Quaternion.identity);
            }
            rand = UnityEngine.Random.Range(1, 3);
            if (rand == 1)
            {
                PhotonNetwork.Instantiate("Revolver", GameObject.Find("WeaponSpawn (1)").transform.position, Quaternion.identity);
            }
            else
            {
                PhotonNetwork.Instantiate("Shotgun", GameObject.Find("WeaponSpawn (1)").transform.position, Quaternion.identity);
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


        public void AddPoints(int points)
        {
            eaterPoints += points;
            pv.RPC("UpdateScoreText", RpcTarget.AllBuffered, eaterPoints);
        }

        public void Death()
        {
            eatersDead++;
            pv.RPC("UpdateAliveText", RpcTarget.AllBuffered, 1);
        }

        public void PlayerRespawn()
        {
            eatersDead--;
            pv.RPC("UpdateAliveText", RpcTarget.AllBuffered, -1);
        }

        [PunRPC]
        public void UpdateScoreText(int newPoints)
        {
            eaterPoints = newPoints;
            string newScoreText = "Eater Score: " + eaterPoints;
            eaterScoreText.text = newScoreText;
        }

        [PunRPC]
        public void UpdateAliveText(int newDeath)
        {
            eatersDead += newDeath;
            string newAliveText = "Eaters Alive: " + (teamManager.GetTeamMembersCount(1) - eatersDead);
            eatersAliveText.text = newAliveText;
        }

        [PunRPC]
        public void Spawn(int spawnLoc, string playerId)
        {
            if (playerId.Equals(PhotonNetwork.LocalPlayer.UserId))
            {
                player.Spawn(spawnLoc);
            }
            else
            {
               // Debug.Log("PlayerID error");
               // Debug.Log("player id = " + playerId + " local = " + PhotonNetwork.LocalPlayer.UserId);
            }
        }

        public void Respawn(GameObject respawnObject)
        {
            string objectName = respawnObject.name;
            if (objectName.Contains("Food1")){
                AddPoints(10);
            }
            else if (objectName.Contains("Food2")){
                    AddPoints(20);
            }
            else
            {
                AddPoints(30);
            }
            Vector3 foodPos = respawnObject.transform.position;
            string food = "Food";
            food += UnityEngine.Random.Range(1, 4);
            IEnumerator coroutine = SpawnWaiter(foodPos, food);

            StartCoroutine(coroutine);
            PhotonNetwork.Destroy(respawnObject);
        }

        IEnumerator SpawnWaiter(Vector3 pos, string prefabName)
        {
            float waitTime = UnityEngine.Random.Range(20, 40);
            yield return new WaitForSeconds(waitTime);
            PhotonNetwork.Instantiate(prefabName, pos, Quaternion.identity);
        }

    }
}
