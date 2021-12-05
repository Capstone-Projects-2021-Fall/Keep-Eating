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
    public enum Items {Fist = 0, Shotgun = 1, Revolver = 2, Taser = 3, Noodle = 10,  Egg = 20, Meat = 30, NA = -1}
    public class GameStateManager : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private GameObject foodPrefab, eaterAiPrefab, enforcerAiPrefab;
        private GameObject Taser;
        private GameObject[] eaterSpawns;
        private GameObject[] enforcerSpawns;
        private GameObject[] foodSpawn;
        private GameObject[] weaponSpawns;
        private PhotonTeamsManager teamManager;         //Gives access to team info. Specifically number of players.
        [SerializeField]
        private int pointsToWin;
        private int eaterIndex, enforcerIndex;
        public int eaterCount;
        private Vector3[] foodSpawns;
        private Text hudText;                           //The text GameObject that displays the time.
        Dictionary<int, Player> players;
        public PlayerManagerV2 player;
        [SerializeField]
        PhotonView pV;

        public bool ReturnToLobby { get; set; }
        public Text EatersScoreText { get; set; }
        public Text EatersAliveText { get; set; }

        public int EatersAlive { get; set; }

        public int EaterPoints { get; set; }

        private GameObject[] eaterAI, enforcerAI;
        private int eaterAiCount, enforcerAiCount;

        private void Awake()
        {
            InitArrays();

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                PhotonView tempPv = player.GetPhotonView();
                if (tempPv != null)
                {
                    Debug.Log("tempPv = " + tempPv.ViewID);
                    pV = tempPv;
                    Debug.Log("Owner of photon view " + pV + " is " + pV.Owner.NickName);
                    break;
                }
            }
            teamManager = GameObject.Find("Team Manager(Clone)").GetComponent<PhotonTeamsManager>();
            eaterAiCount = eaterSpawns.Length - teamManager.GetTeamMembersCount(1);
            enforcerAiCount = enforcerSpawns.Length - teamManager.GetTeamMembersCount(2);
            eaterAI = new GameObject[eaterAiCount];
            enforcerAI = new GameObject[enforcerAiCount];
        }
        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(this.gameObject);
            this.EaterPoints = 0;
            pointsToWin = 300;
            hudText = GameObject.Find("Timer").GetComponent<Text>();
            this.EatersScoreText = GameObject.Find("Eater Score").GetComponent<Text>();
            this.EatersAliveText = GameObject.Find("Eaters Alive").GetComponent<Text>();
            if (StaticSettings.Bots)
            {
                this.EatersAliveText.text = "Eaters Alive: " + eaterSpawns.Length;
                this.EatersAlive = eaterSpawns.Length;
            }
            else
            {
                this.EatersAliveText.text = "Eaters Alive: " + teamManager.GetTeamMembersCount(1);
                this.EatersAlive = teamManager.GetTeamMembersCount(1);
            }
            this.ReturnToLobby = false;
        }


        // TODO make generic for different maps
        private void InitArrays()
        {
            eaterSpawns = GameObject.FindGameObjectsWithTag("EaterSpawn");
            enforcerSpawns = GameObject.FindGameObjectsWithTag("EnforcerSpawn");
            foodSpawn = GameObject.FindGameObjectsWithTag("Food");
            weaponSpawns = GameObject.FindGameObjectsWithTag("Weapon");
        }

        public void SpawnPlayers()
        {
            players = PhotonNetwork.CurrentRoom.Players;
            eaterIndex = 0;
            enforcerIndex = 0;
            Debug.Log("players = " + players);
            Debug.Log("players count = " + PhotonNetwork.CurrentRoom.PlayerCount);
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                if (PhotonTeamExtensions.GetPhotonTeam(players[i + 1]).Code == 1)
                {
                    pV.RPC("SpawnRpc", players[i+1], eaterIndex++, players[i+1].UserId);
                    //Debug.Log("UserId = " + players[i + 1].UserId);
                    Debug.Log("player name " + i + " = " + players[i + 1].NickName + " is spawning as an eater");
                }
                else
                {
                    //Debug.Log("player id = " + players[i + 1].UserId);
                    pV.RPC("SpawnRpc", players[i+1], enforcerIndex++, players[i + 1].UserId);
                   Debug.Log("player name " + i + " = " + players[i + 1].NickName + " is spawning as an enforcer");
                }
            }

            if (StaticSettings.Bots)
            {
                
                for (int i = 0; i < eaterAiCount; i++)
                {
                    eaterAI[i] = PhotonNetwork.InstantiateRoomObject("EaterAI", eaterSpawns[eaterIndex++].transform.position, Quaternion.identity);
                    eaterAI[i].GetComponent<AIScript>().PV = pV;
                }
                for (int i = 0; i < enforcerAiCount; i++)
                {
                    enforcerAI[i] = PhotonNetwork.InstantiateRoomObject("EnforcerAI", enforcerSpawns[enforcerIndex++].transform.position, Quaternion.identity);
                    enforcerAI[i].GetComponent<AIScript>().PV = pV;
                }
                
                //eaterAI[0] = PhotonNetwork.InstantiateRoomObject("EaterAI", eaterSpawns[eaterIndex++].transform.position, Quaternion.identity);
                //eaterAI[0].GetComponent<AIScript>().PV = pV;
                eaterAI[0].GetComponent<AIScript>().isAlpha = true;
            }
            
        }

        public void SpawnFood()
        {
            int rand;
            foreach(GameObject spawn in foodSpawn)
            {
                rand = UnityEngine.Random.Range(1, 4);
                pV.RPC("SpawnFoodRpc", RpcTarget.AllBuffered, spawn.name, rand);
            }
        }

        public void SpawnWeapons()
        {
            /*
            int rand = UnityEngine.Random.Range(1, 3);
            if (rand == 1)
            {
                pV.RPC("SpawnWeaponRpc", RpcTarget.AllBuffered, "WeaponSpawn0", 1);
            }
            else
            {
                pV.RPC("SpawnWeaponRpc", RpcTarget.AllBuffered, "WeaponSpawn0", 2);
            }
            rand = UnityEngine.Random.Range(1, 3);
            if (rand == 1)
            {
                pV.RPC("SpawnWeaponRpc", RpcTarget.AllBuffered, "WeaponSpawn1", 1);
            }
            else
            {
                pV.RPC("SpawnWeaponRpc", RpcTarget.AllBuffered, "WeaponSpawn1", 2);
            } */

            int rand;
            foreach (GameObject spawn in weaponSpawns)
            {
                rand = UnityEngine.Random.Range(1, 3);
                pV.RPC("SpawnFoodRpc", RpcTarget.AllBuffered, spawn.name, rand);
            }
        }

        // Checks for win conditions.
        void Update()
        {
            if (this.EatersAlive <= 0)
            {
                GameOver("Death");
            }
            else if (this.EaterPoints >= pointsToWin)
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
            
            if (PhotonNetwork.IsMasterClient && !this.ReturnToLobby)
            {
                player.cameraMovement.StopSpectating();
                if (StaticSettings.Bots)
                {
                    foreach (GameObject eater in eaterAI)
                    {
                        PhotonNetwork.Destroy(eater);
                    }
                    foreach (GameObject enforcer in enforcerAI)
                    {
                        PhotonNetwork.Destroy(enforcer);
                    }
                    //PhotonNetwork.Destroy(eaterAI[0]);
                    //PhotonNetwork.Destroy(enforcerAI[0]);
                }
                Debug.Log("Master Client returning to lobby");
                this.ReturnToLobby = true;
                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.LoadLevel("Lobby");
            }
        }


        public void AddPoints(string itemName, Items foodType)
        {
            this.EaterPoints += (int)foodType;
            pV.RPC("UpdateScoreText", RpcTarget.All, this.EaterPoints);
        }

        public void Death()
        {
            if (pV.IsMine)
            {
                Debug.Log("Death pv = " + pV.ViewID);
                this.EatersAlive--;
                if (this.EatersAlive > 0)
                {
                    pV.RPC("UpdateAliveText", RpcTarget.All, -1);
                }
            }
        }

        public void PlayerRespawn()
        {
            if (pV.IsMine)
            {
                this.EatersAlive++;
                pV.RPC("UpdateAliveText", RpcTarget.All, 1);
            }
        }

    }
}









/*

imagining yourself as a abacus
or like a kangaroo jumping around
breathing in the cold air from this morning
inhale exhale
like breathing soup

-JHC 
 
 */