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
    public enum Items {Fist = 0, Shotgun = 1, Revolver = 2, Noodle = 10,  Egg = 20, Meat = 30, NA = -1}
    public class GameStateManager : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private GameObject foodPrefab;

        private GameObject[] eaterSpawns;
        private GameObject[] enforcerSpawns;
        private GameObject[] foodSpawn;
        private GameObject[] weaponSpawns;
        private PhotonTeamsManager teamManager;         //Gives access to team info. Specifically number of players.
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

        public int EatersDead { get; set; }

        public int EaterPoints { get; set; }

        private void Awake()
        {
            /*
            foodSpawns = new Vector3[5];
            foodSpawns[0] = GameObject.Find("FoodSpawn").transform.position;
            foodSpawns[1] = GameObject.Find("FoodSpawn (1)").transform.position;
            foodSpawns[2] = GameObject.Find("FoodSpawn (2)").transform.position;
            foodSpawns[3] = GameObject.Find("FoodSpawn (3)").transform.position;
            foodSpawns[4] = GameObject.Find("FoodSpawn (4)").transform.position;
            */
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
        }
        // Start is called before the first frame update
        void Start()
        {
            this.EatersDead = 0;
            this.EaterPoints = 0;
            pointsToWin = 100;
            teamManager = GameObject.Find("Team Manager(Clone)").GetComponent<PhotonTeamsManager>();
            hudText = GameObject.Find("Timer").GetComponent<Text>();
            this.EatersScoreText = GameObject.Find("Eater Score").GetComponent<Text>();
            this.EatersAliveText = GameObject.Find("Eaters Alive").GetComponent<Text>();
            this.EatersAliveText.text = "Eaters Alive: " + teamManager.GetTeamMembersCount(1);
            this.ReturnToLobby = false;
        }


        // TODO make generic for different maps
        private void InitArrays()
        {
            eaterSpawns = new GameObject[3];
            enforcerSpawns = new GameObject[2];
            foodSpawn = new GameObject[5];
            weaponSpawns = new GameObject[2];
            for (int i = 0; i < 3; i++)
            {
                string spName = "EaterSpawn" + i;
                eaterSpawns[i] = GameObject.Find(spName);
            }
            for (int i = 0; i < 2; i++)
            {
                string spName = "EnforcerSpawn" + i;
                enforcerSpawns[i] = GameObject.Find(spName);
            }
            for (int i = 0; i < 5; i++)
            {
                string spName = "FoodSpawn" + i;
                foodSpawn[i] = GameObject.Find(spName);
            }
            for (int i = 0; i < 2; i++)
            {
                string spName = "WeaponSpawn" + i;
                weaponSpawns[i] = GameObject.Find(spName);
            }
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
        }

        public void SpawnFood()
        {
            string food = "Food";
            int rand;
            foreach(GameObject spawn in foodSpawn)
            {
                rand = UnityEngine.Random.Range(1, 4);
                pV.RPC("SpawnFoodRpc", RpcTarget.AllBuffered, spawn.name, rand);
            }
        }

        public void SpawnWeapons()
        {
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
            }
        }

        // Checks for win conditions.
        void Update()
        {
            if (this.EatersDead == teamManager.GetTeamMembersCount(1))
            {
                GameOver("Death");
            }
            else if (this.EaterPoints == pointsToWin)
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
            //this.EatersDead++;
            pV.RPC("UpdateAliveText", RpcTarget.All, 1);
        }

        public void PlayerRespawn()
        {
            //this.EatersDead--;
            pV.RPC("UpdateAliveText", RpcTarget.All, -1);
        }

        public void Respawn(GameObject respawnObject)
        {
            string objectName = respawnObject.name;
            /*
            if (objectName.Contains("Food1")){
                AddPoints(Items.Noodle);
            }
            else if (objectName.Contains("Food2")){
                 AddPoints(20);
            }
            else
            {
                AddPoints(30);
            }
            */
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
