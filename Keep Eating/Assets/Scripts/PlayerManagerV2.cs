/*
    This is used by ALL player prefabs, including the local versions of other clients.
    When writing code in this room, it is important to use photonView.IsMine to make sure
    you are only controlling your own character.

    There are a lot of things that are not automatically synchronized and need to be synchronized
    using RPC calls to send messages through the server. This is done as such:
        sending message:    myPhotonView.RPC("FunctionName", RpcTargets.<desired recipient>, function arguments);
        When the recipient receives the message is calls the "FunctionName" function which is a normal function 
        but with the tag [Pun RPC] above it. 
    A photon view is necessary to send an RPC message. 
    The message call and the [Pun RPC] must be in the same class. 
 */



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

namespace Com.tuf31404.KeepEating
{
    public class PlayerManagerV2 : MonoBehaviourPunCallbacks
    {
        #region Variables
        //Serialized private variables
        [SerializeField]
        private float speed;
        [SerializeField]
        private GameObject PlayerUiPrefab;
        [SerializeField]
        private Sprite eaterSprite, enforcerSprite, shotgunSprite, revolverSprite, taserSprite;
        [SerializeField]
        private SpriteRenderer mySpriteRenderer, weaponSpriteRenderer, taserSpriteRenderer;
        [SerializeField]
        private Shoot shootScript;
        [SerializeField]
        private Transform muzzleTransform;
        [SerializeField]
        private GameObject bulletPrefab;
        //object variables
        private CameraMovement cameraMovement;
        private PhotonTeamsManager teamsManager;
        private GameStateManager gsm;
        private Button eaterSwitch, enforcerSwitch;
        private GameObject[] enforcerSpawns;
        private GameObject[] eaterSpawns;
        private GameObject[] foodSpawns;
        private GameObject[] weaponSpawns;
        //numbers
        private int eaterTeamMax, enforcerTeamMax;
        private int bulletsShot;
        private int myPoints;
        //vectors
        private Vector3 pos;
        //booleans
        private bool hasGun;                                //change to hasWeapon
        private bool isAlive;
        private bool facingLeft;
        private bool gunCollision;
        private bool foodCollision;
        private bool taserCollision;
        private bool inGame;
        //strings
        private Items weaponType;
        private Items tempWeaponType;
        private string tempItemName;
        private Items tempFoodType;
        //public variables
        [Tooltip("The Player's UI GameObject Prefab")]
        public static GameObject LocalPlayerInstance;
        [Tooltip("The current Health of our player")]
        public float Health = 1f;

        //Old Variables, will probably remove
        GameObject weapon;
        bool hasWeapon = false;
        private byte myTeam;
        private GameObject tempWeapon;
        private int foodId;
        private int lastFood;
        #endregion

        public bool HasTaser { get; set; }
        public bool FiringTaser{ get; set; }


        #region Init
        void Awake()
        {
            //PhotonView.IsMine is used so this only runs on your player object.
            //This is needed because other players will also be running this script, but you don't
            //want them to run some of this code - like you only want your character to move when you press a key.
            if (this.photonView.IsMine)
            {
                PlayerManagerV2.LocalPlayerInstance = this.gameObject;
                //PlayerManager.LocalPlayerInstance = this.gameObject;
                isAlive = true;
            }
            inGame = false;
            //Saves this gameObject instance when the scene is changed.
            DontDestroyOnLoad(this.gameObject);
        }


        private void Start()
        {
            DontDestroyOnLoad(this.gameObject);
            //Only the player prefab that you control can call these methods.
            if (this.photonView.IsMine)
            {
                Debug.Log("photon view = " + photonView.ViewID);
                //mySpriteRenderer = this.gameObject.transform.Find("PlayerSpriteRenderer").gameObject.GetComponent<SpriteRenderer>();
                //Debug.Log(this.gameObject.transform.GetChild(0).gameObject.name);
                //Camera movement - see CameraMovement script
                cameraMovement = this.gameObject.GetComponent<CameraMovement>();

                if (cameraMovement != null)
                {
                    if (this.photonView.IsMine)
                    {
                        cameraMovement.StartFollowing();
                    }
                    else
                    {
                        Debug.Log("Fuck");
                    }
                }

                UpdateTeamMax();

                teamsManager = GameObject.Find("Team Manager(Clone)").GetComponent<PhotonTeamsManager>();
                
                if (teamsManager == null)
                {
                    Debug.Log("wtf???");
                }
                //Trying to join a team (randomly) when you get in the lobby.
                TryJoinTeam((byte)UnityEngine.Random.Range(1, 3));
                
                //Calls OnSceneLoaded()
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

                eaterSwitch = GameObject.Find("Eater Button").GetComponent<Button>();
                enforcerSwitch = GameObject.Find("Enforcer Button").GetComponent<Button>();

                eaterSwitch.onClick.AddListener(() => SwitchTeams(1));
                enforcerSwitch.onClick.AddListener(() => SwitchTeams(2));

                this.HasTaser = false;
                this.FiringTaser = false;
                facingLeft = true;
                hasGun = false;
                gunCollision = false;
                foodCollision = false;
                weaponType = Items.Fist;
                bulletsShot = 0;
                myPoints = 0;

                //old
                weapon = null;
                lastFood = 0;
            }
            if (PlayerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(PlayerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.Log("UI is null");
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }
        }

        #endregion

        #region Updates and Inputs
        void Update()
        {

            if (this.photonView.IsMine && isAlive)
            {
                ProcessInputs();
                if (Health <= 0f && inGame)
                {
                    //GameManager.Instance.LeaveRoom();
                    isAlive = false;
                    this.photonView.RPC("PlayerDead", RpcTarget.All, this.photonView.ViewID);
                }
            }

        }


        public void SwitchTeams(byte teamNum)
        {
            if (teamNum == myTeam)
            {
                return;
            }

            Debug.Log("switching teams");
            PhotonTeamExtensions.SwitchTeam(PhotonNetwork.LocalPlayer, teamNum);
            if (myTeam == 1)
            {
                myTeam = 2;
            }
            else
            {
                myTeam = 1;
            }
            //Makes a call to all clients that they have joined a team.
            //RpcTarget.AllBuffered ensures that players that join after the call recieve the message.
            this.photonView.RPC("SetTeam", RpcTarget.AllBuffered, myTeam, this.photonView.ViewID);
            //Changes sprite depending on team.
            if (myTeam == 1)
            {
                mySpriteRenderer.sprite = eaterSprite;
            }
            else
            {
                mySpriteRenderer.sprite = enforcerSprite;
            }

        }

        void ProcessInputs()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (facingLeft && h > 0)
            {
                mySpriteRenderer.flipX = true;
                facingLeft = false;
            }
            else if (!facingLeft && h < 0)
            {
                mySpriteRenderer.flipX = false;
                facingLeft = true;
            }


            //transform.position is the Game Object's position
            pos = transform.position;

            pos.x += h * speed * Time.deltaTime;
            pos.y += v * speed * Time.deltaTime;

            transform.position = pos;


            Vector3 mousepos = Input.mousePosition;
            mousepos.z = 0;
            Vector3 objectpos = Camera.main.WorldToScreenPoint(transform.position);
            mousepos.x -= objectpos.x;
            mousepos.y -= objectpos.y;

            float angle = Mathf.Atan2(mousepos.y, mousepos.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            this.gameObject.transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(0, 0, 0));

            if (weapon != null)
            {
                if (mousepos.x < 0)
                {
                    weapon.transform.GetComponentInChildren<SpriteRenderer>().flipY = false;
                }
                else
                {
                    weapon.transform.GetComponentInChildren<SpriteRenderer>().flipY = true;
                }
            }

            if (Input.GetButtonDown("Fire1") && myTeam == 2)
            {
                if (hasGun)
                {
                    if (weaponType == Items.Shotgun)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            bulletsShot++;
                            photonView.RPC("ShootGun", RpcTarget.All, PhotonNetwork.NickName + bulletsShot, shootScript.ShootGun(weaponType), muzzleTransform.position);
                        }
                    }
                    else
                    {
                        bulletsShot++;
                        photonView.RPC("ShootGun", RpcTarget.All, PhotonNetwork.NickName + bulletsShot, shootScript.ShootGun(weaponType), muzzleTransform.position);
                    }
                }
            }
            else if (Input.GetButtonDown("Fire1") && myTeam == 1)
            {
                if (this.HasTaser)
                {
                    this.FiringTaser = true;
                }
            }
            else
            {
                this.FiringTaser = false;
            }


            if (Input.GetKeyDown(KeyCode.F))
            {
                if (gunCollision && myTeam == 2)
                {
                    Debug.Log("Picking Up Gun");
                    hasGun = true;
                    weaponType = tempWeaponType;
                    photonView.RPC("PickUpGun", RpcTarget.All, photonView.ViewID, weaponType, tempItemName);
                    tempWeaponType = Items.NA;
                    gunCollision = false;
                }

                if (foodCollision && myTeam == 1)
                {
                    //myPoints += foodType points
                    photonView.RPC("PickUpFood", RpcTarget.All, tempItemName, tempFoodType);
                    tempItemName = "";
                    tempFoodType = Items.NA;
                    foodCollision = false;
                }

                if (taserCollision && myTeam == 1)
                {
                    this.HasTaser = true;
                    weaponType = Items.Taser;
                    photonView.RPC("PickUpGun", RpcTarget.All, photonView.ViewID, Items.Taser, tempItemName);
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!this.photonView.IsMine)
            {
                return;
            }

            if (other.gameObject.name.Contains("Weapon"))
            {
                tempItemName = other.gameObject.name;
                tempWeaponType = other.gameObject.GetComponent<ItemSpawnScript>().ItemType;
                gunCollision = true;
            }

            if (other.gameObject.name.Contains("Food"))
            {
                tempItemName = other.gameObject.name;
                tempFoodType = other.gameObject.GetComponent<ItemSpawnScript>().ItemType;
                foodCollision = true;
            }

            if (other.gameObject.name.Contains("Taser"))
            {
                Debug.Log("Taser trigger");
                tempItemName = other.gameObject.name;
                taserCollision = true;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!this.photonView.IsMine)
            {
                return;
            }

            if (collision.gameObject.tag.Equals("Bullet") && myTeam == 1){
                photonView.RPC("HitByBullet", RpcTarget.All, photonView.ViewID, collision.gameObject.name);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!this.photonView.IsMine)
            {
                return;
            }

            if (other.gameObject.name.Contains("Weapon"))
            {
                tempItemName = "";
                tempWeaponType = Items.NA;
                gunCollision = false;
            }
            else if (other.gameObject.tag.Equals("Food"))
            {
                tempItemName = "";
                tempFoodType = Items.NA;
                foodCollision = false;
            }
        }


        //This method is to enssure there is always about a 75/25 eater/enforcer ratio.
        private void UpdateTeamMax()
        {
            int roomCount = PhotonNetwork.CurrentRoom.PlayerCount;

            if (roomCount <= 5)
            {
                eaterTeamMax = 3;
                enforcerTeamMax = 2;
                return;
            }

            if (roomCount <= 7)
            {
                enforcerTeamMax = 2;
            }
            else
            {
                enforcerTeamMax = 3;
            }

            eaterTeamMax = roomCount - enforcerTeamMax;
        }

        private void TryJoinTeam(byte teamNum)
        {
            if (teamNum == 1)
            {
                if (teamsManager.GetTeamMembersCount(1) == eaterTeamMax)
                {
                    teamNum = 2;
                }
            }
            else
            {
                if (teamsManager.GetTeamMembersCount(2) == enforcerTeamMax)
                {
                    teamNum = 1;
                }
            }

            if (!PhotonTeamExtensions.JoinTeam(PhotonNetwork.LocalPlayer, teamNum))
            {
                Debug.Log("Join Team fail");
            }

            myTeam = teamNum;
            Debug.Log("my team on start = " + myTeam);
            this.photonView.RPC("SetTeam", RpcTarget.AllBuffered, teamNum, this.photonView.ViewID);
            if (teamNum == 1)
            {
                mySpriteRenderer.sprite = eaterSprite;
            }
            else
            {
                mySpriteRenderer.sprite = enforcerSprite;
            }
        }

        public void Spawn(int spawnNum)
        {
            myTeam = PhotonTeamExtensions.GetPhotonTeam(PhotonNetwork.LocalPlayer).Code;
            Debug.Log("my team = " + myTeam);
            Debug.Log("Owner of photon view " + photonView + " is " + photonView.Owner.NickName);
            if (myTeam == 1)
            {
                LocalPlayerInstance.transform.position = eaterSpawns[spawnNum].transform.position;
                Debug.Log("objpos = " + this.gameObject.transform.position);
                Debug.Log("eaterspawns pos = " + eaterSpawns[spawnNum].transform.position);
            }
            else
            {
                LocalPlayerInstance.transform.position = enforcerSpawns[spawnNum].transform.position;
                Debug.Log("objpos = " + this.gameObject.transform.position);
                Debug.Log("enforcerspawns pos = " + enforcerSpawns[spawnNum].transform.position);
            }
        }

        #endregion


        #region RPC functions

        [PunRPC]
        public void HitByBullet(int viewId, string bulletName)
        {
            PhotonView.Find(viewId).gameObject.GetComponent<PlayerManagerV2>().Health -= 1f;
        }


        [PunRPC]
        public void PlayerDead(int pvId)
        {
                if (this.photonView.ViewID == pvId)
                {
                    mySpriteRenderer.enabled = false;
                    this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                    IEnumerator coroutine = RespawnWaiter(pvId);
                    StartCoroutine(coroutine);
                }
                else
                {
                    PhotonView.Find(pvId).gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    PhotonView.Find(pvId).gameObject.GetComponent<BoxCollider2D>().enabled = false;
                }
            GameObject.FindGameObjectWithTag("GSM").GetComponent<GameStateManager>().Death();
        }

        [PunRPC]
        public void PlayerRespawn(int pvId, Vector3 pos)
        {
            if (this.photonView.ViewID == pvId)
            {
                mySpriteRenderer.enabled = true;
                this.gameObject.GetComponent<BoxCollider2D>().enabled = true;
                this.gameObject.transform.position = pos;
                Health = 1f;
                isAlive = true;
            }
            else
            {
                GameObject obj = PhotonView.Find(pvId).gameObject;
                obj.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                obj.GetComponent<BoxCollider2D>().enabled = true;
                obj.transform.position = pos;
                obj.GetComponent<PlayerManager>().Health = 1f;
            }
        }

        [PunRPC]
        void SetTeam(byte teamId, int viewId)
        {
            SpriteRenderer playerSprite = PhotonView.Find(viewId).gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
            if (teamId == 1)
            {
                playerSprite.sprite = eaterSprite;
            }
            else
            {
                playerSprite.sprite = enforcerSprite;
            }
            
        }

        [PunRPC]
        void PickUpFood(string _itemName, Items _foodType)
        {
                GameObject food = GameObject.Find(_itemName);
                food.GetComponent<ItemSpawnScript>().Despawn();
                GameObject.FindWithTag("GSM").GetComponent<GameStateManager>().AddPoints(_itemName, _foodType);
                if (PhotonNetwork.IsMasterClient)
                {
                IEnumerator coroutine = FoodRespawnWaiter(food);
                StartCoroutine(coroutine);
                }
        }

        IEnumerator FoodRespawnWaiter(GameObject food)
        {
            float waitTime = UnityEngine.Random.Range(20, 40);
            yield return new WaitForSeconds(waitTime);
            if (food != null)
            {
                photonView.RPC("SpawnFoodRpc", RpcTarget.All, food.name, UnityEngine.Random.Range(1, 4));
            }
        }

        [PunRPC]
        void PickUpGun(int viewId, Items _weaponType, string itemName)
        {
                Sprite tempSprite;
                if (_weaponType == Items.Shotgun)
                {
                    tempSprite = shotgunSprite;
                }
                else if (_weaponType == Items.Revolver)
                {
                    tempSprite = revolverSprite;
                }
                else
                {
                    tempSprite = taserSprite;
                }

                if (photonView.ViewID == viewId)
                {
                    weaponSpriteRenderer.sprite = tempSprite;
                }
                else
                {
                    PhotonView.Find(viewId).gameObject.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite = tempSprite;
                }
                GameObject.Find(itemName).GetComponent<ItemSpawnScript>().Despawn();
        }

        [PunRPC]
        void ShootGun(string name, Vector3 direction, Vector3 position)
        {
                GameObject newBullet = Instantiate(bulletPrefab, position, Quaternion.identity);
                newBullet.GetComponent<BulletScript>().BulletName = name;
                newBullet.GetComponent<BulletScript>().SetDirection(direction);
        }

        [PunRPC]
        public void SpawnFoodRpc(string name, int type)
        {   
                GameObject food = GameObject.Find(name);
                switch (type)
                {
                    case 1:
                        food.GetComponent<ItemSpawnScript>().Spawn(1, Items.Noodle);
                        break;
                    case 2:
                        food.GetComponent<ItemSpawnScript>().Spawn(2, Items.Egg);
                        break;
                    case 3:
                        food.GetComponent<ItemSpawnScript>().Spawn(3, Items.Meat);
                        break;
                    default:
                        Debug.Log("Spawn food RPC error");
                        break;
                }
        }

        [PunRPC]
        public void SpawnRpc(int spawnLoc, string playerId)
        {

            Spawn(spawnLoc);
            if (playerId.Equals(PhotonNetwork.LocalPlayer.UserId))
            {
                Debug.Log("Spawning " + PhotonNetwork.LocalPlayer.NickName + " in pos " + spawnLoc);
                Spawn(spawnLoc);
            }
            else
            {
                // Debug.Log("PlayerID error");
                // Debug.Log("player id = " + playerId + " local = " + PhotonNetwork.LocalPlayer.UserId);
            }
        }

        [PunRPC]
        public void SpawnWeaponRpc(string name, int type)
        {

                if (type == 1)
                {
                    GameObject.Find(name).GetComponent<ItemSpawnScript>().Spawn(type, Items.Shotgun);
                }
                else
                {
                    GameObject.Find(name).GetComponent<ItemSpawnScript>().Spawn(type, Items.Revolver);
                }
        }

        [PunRPC]
        public void UpdateScoreText(int newPoints)
        {
           
                Debug.Log("Updating score");
                GameObject.FindWithTag("GSM").GetComponent<GameStateManager>().EaterPoints = newPoints;
                string newScoreText = "Eater Score: " + GameObject.FindWithTag("GSM").GetComponent<GameStateManager>().EaterPoints;
                GameObject.FindWithTag("GSM").GetComponent<GameStateManager>().EatersScoreText.text = newScoreText;
            
        }

        [PunRPC]
        public void UpdateAliveText(int newDeath) 
        {
            if (photonView.IsMine)
            {

                try
                {
                    
                    string newAliveText = "Eaters Alive: " + (teamsManager.GetTeamMembersCount(1) - GameObject.FindWithTag("GSM").GetComponent<GameStateManager>().EatersDead);
                    GameObject.FindWithTag("GSM").GetComponent<GameStateManager>().EatersAliveText.text = newAliveText;
                }
                catch (System.NullReferenceException e)
                {
                    if (teamsManager == null)
                    {
                        Debug.Log("teamsmanager null");
                    }
                    else if (GameObject.FindWithTag("GSM").GetComponent<GameStateManager>() == null)
                    {
                        Debug.Log("gsm null");
                    }
                    else
                    {
                        Debug.Log("???");
                    }
                }
            }
        }
        #endregion


        IEnumerator RespawnWaiter(int pvId)
        {
            GameObject.FindWithTag("GSM").GetComponent<GameStateManager>().Death();
            yield return new WaitForSeconds(10f);
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("EaterSpawn");
            if (spawns.Length != 0)
            {
                int spawnPoint = UnityEngine.Random.Range(0, spawns.Length);
                photonView.RPC("PlayerRespawn", RpcTarget.All, pvId, spawns[spawnPoint].transform.position);
                GameObject.FindWithTag("GSM").GetComponent<GameStateManager>().PlayerRespawn();
            }
            else
            {
                photonView.RPC("PlayerRespawn", RpcTarget.All, pvId, Vector3.zero);
            }
        }
        #region PunCallbacks

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            UpdateTeamMax();
        }

        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);

        }

        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
            PhotonNetwork.AutomaticallySyncScene = true;
            if (level == 2)
            {
                if (GameObject.FindGameObjectWithTag("GSM") != null)
                {
                    Destroy(GameObject.FindGameObjectWithTag("GSM"));
                }
                inGame = false;
                hasGun = false;
                weaponType = Items.NA;
                weaponSpriteRenderer.sprite = null;
                GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
                foreach(GameObject player in playerObjects)
                {
                    PhotonView.Find(player.GetComponent<PhotonView>().ViewID).gameObject.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite = null;
                }
                eaterSwitch = GameObject.Find("Eater Button").GetComponent<Button>();
                enforcerSwitch = GameObject.Find("Enforcer Button").GetComponent<Button>();

                eaterSwitch.onClick.AddListener(() => SwitchTeams(1));
                enforcerSwitch.onClick.AddListener(() => SwitchTeams(2));
            }

            cameraMovement.GetCamera();
            if (level == 3)
            {
                inGame = true;
                GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
                photonView.RPC("InitSpawnArrays", RpcTarget.AllBuffered);
                //Debug.Log("scene loaded called");
                gsm = GameObject.FindWithTag("GSM").GetComponent<GameStateManager>();
                Debug.Log(gsm.ToString());
                gsm.player = this;
                if (PhotonNetwork.IsMasterClient)
                {
                    gsm.SpawnPlayers();
                    //this.gameObject.transform.position = GameObject.Find("EaterSpawn").transform.position;
                    gsm.SpawnFood();
                    gsm.SpawnWeapons();
                    gsm.eaterCount = 0;
                }
            }
        }

        [PunRPC]
        private void InitSpawnArrays()
        {
            enforcerSpawns = GameObject.FindGameObjectsWithTag("EnforcerSpawn");
            eaterSpawns = GameObject.FindGameObjectsWithTag("EaterSpawn");
        }

        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endregion

        public PhotonView GetPhotonView()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                return this.photonView;
            }
            else return null;
        }
    }
}
