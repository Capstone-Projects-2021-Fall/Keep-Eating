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


    TODO: Add if statements to control what the player can do depending on their team.
          Revolver pickup.
          Stop the player from moving if they are dead or stunned.
          Add fists.
          Maybe change weapon and food prefab tags and names.
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
    public class PlayerManager : MonoBehaviourPunCallbacks
    {
        public float speed;
        private Vector3 pos;       
        GameObject weapon;
        bool hasWeapon = false;
        [Tooltip("The current Health of our player")]
        public float Health = 1f;
        [Tooltip("The Player's UI GameObject Prefab")]
        //PlayerUiPrefab is the name and health bar that appears above your character.
        [SerializeField]
        public GameObject PlayerUiPrefab;
        public static GameObject LocalPlayerInstance;
        CameraMovement cameraMovement;
        private PhotonTeamsManager teamsManager;
        int eaterTeamMax, enforcerTeamMax;
        public Sprite eaterSprite, enforcerSprite, deadEaterSprite;
        private byte myTeam;
        Button eaterSwitch, enforcerSwitch;
        [SerializeField]
        private GameStateManager gsm;
        private bool isAlive;
        private bool facingLeft;
        private SpriteRenderer mySpriteRenderer;
        private bool gunCollision;
        private bool foodCollision;
        private GameObject tempWeapon;
        private int foodId;
        private int lastFood;


        #region Init
        void Awake()
        {   
            //PhotonView.IsMine is used so this only runs on your player object.
            //This is needed because other players will also be running this script, but you don't
            //want them to run some of this code - like you only want your character to move when you press a key.
            if (this.photonView.IsMine)
            {
                PlayerManager.LocalPlayerInstance = this.gameObject;
                isAlive = true;
            }
           
            //Saves this gameObject instance when the scene is changed.
            DontDestroyOnLoad(this.gameObject);
        }


        private void Start()
        {
            //Only the player prefab that you control can call these methods.
            if (this.photonView.IsMine)
            {
                mySpriteRenderer = this.gameObject.transform.Find("PlayerSpriteRenderer").gameObject.GetComponent<SpriteRenderer>();
                Debug.Log(this.gameObject.transform.GetChild(0).gameObject.name);
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
                teamsManager = GameObject.Find("Team Manager").GetComponent<PhotonTeamsManager>();
                //Trying to join a team (randomly) when you get in the lobby.
                TryJoinTeam((byte)UnityEngine.Random.Range(1, 3));
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

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
                eaterSwitch = GameObject.Find("Eater Button").GetComponent<Button>();
                enforcerSwitch = GameObject.Find("Enforcer Button").GetComponent<Button>();

                eaterSwitch.onClick.AddListener(() => SwitchTeams(1));
                enforcerSwitch.onClick.AddListener(() => SwitchTeams(2));
                facingLeft = true;
                weapon = null;
                gunCollision = false;
                foodCollision = false;
                lastFood = 0;
            }
        }

        #endregion

        #region Updates and Inputs
        void Update()
        {

            if (this.photonView.IsMine && isAlive)
            {
                ProcessInputs();
                if (Health <= 0f)
                {
                    //GameManager.Instance.LeaveRoom();
                    isAlive = false;
                    this.photonView.RPC("PlayerDead", RpcTarget.All, LocalPlayerInstance.GetPhotonView().ViewID);
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
                if (hasWeapon && weapon.CompareTag("Gun"))
                {
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        this.photonView.RPC("ShootGun", RpcTarget.MasterClient, this.gameObject.transform.GetChild(1).gameObject.GetPhotonView().ViewID);
                    }
                    else
                    {
                        gameObject.GetComponentInChildren<Shoot>().ShootGun();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (gunCollision && myTeam == 2)
                {
                    if (tempWeapon != null)
                    {
                        weapon = tempWeapon;

                        this.photonView.RPC("PickUpGun", RpcTarget.All, weapon.GetPhotonView().ViewID, LocalPlayerInstance.GetPhotonView().ViewID);

                        hasWeapon = true;
                    }
                    gunCollision = false;
                    tempWeapon = null;
                }

                if (foodCollision && myTeam == 1)
                {
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        this.photonView.RPC("PickUpFood", RpcTarget.MasterClient, foodId);
                    }
                    else
                    {
                        PickUpFood(foodId);
                    }
                    foodCollision = false;
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!this.photonView.IsMine)
            {
                return;
            }

            if (!other.name.Contains("Bullet"))
            {
                return;
            }

            if (PhotonTeamExtensions.GetPhotonTeam(PhotonNetwork.LocalPlayer).Code == 1)
            {
                Health -= 0.1f;
                if (Health >= 0)
                {
                    this.photonView.RPC("HitByBullet", RpcTarget.All, this.photonView.ViewID, other.GetComponent<PhotonView>().ViewID);
                }
            }
            
        }
        
        void OnTriggerStay2D(Collider2D collision)
        {
            if (!this.photonView.IsMine)
            {
                return;
            }

            if (collision.gameObject.tag.Equals("Gun") && myTeam == 2)
            {
                tempWeapon = collision.gameObject;
                gunCollision = true;
            }
            else if (collision.gameObject.tag.Equals("Food") && myTeam == 1)
            {
                if (foodId != collision.gameObject.GetComponent<PhotonView>().ViewID){
                    foodId = collision.gameObject.GetComponent<PhotonView>().ViewID;
                    Debug.Log("foodId = " + foodId);
                    foodCollision = true;
                }
            }

        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!this.photonView.IsMine)
            {
                return;
            }

            if (other.gameObject.tag.Equals("Gun"))
            {
                tempWeapon = null;
                gunCollision = false;
            }
            else if (other.gameObject.tag.Equals("Food"))
            {
                foodId = -1;
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
                if (teamsManager.GetTeamMembersCount(1) == eaterTeamMax){
                    teamNum = 2;
                }
            }
            else
            {
                if (teamsManager.GetTeamMembersCount(2) == enforcerTeamMax){
                    teamNum = 1;
                }
            }

            if (!PhotonTeamExtensions.JoinTeam(PhotonNetwork.LocalPlayer, teamNum))
            {
                Debug.Log("Join Team fail");
            }

            myTeam = teamNum;
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

            if (myTeam == 1)
            {
                switch (spawnNum)
                {
                    case 0:
                        this.gameObject.transform.position = GameObject.Find("EaterSpawn").transform.position;
                        break;
                    case 1:
                        this.gameObject.transform.position = GameObject.Find("EaterSpawn (1)").transform.position;
                        break;
                    case 2:
                        this.gameObject.transform.position = GameObject.Find("EaterSpawn (2)").transform.position;
                        break;
                    default:
                        Debug.Log("Oops enforcer spawn");
                        break;
                }
            }
            else
            {
                switch (spawnNum)
                {
                    case 0:
                        this.gameObject.transform.position = GameObject.Find("EnforcerSpawn").transform.position;
                        break;
                    case 1:
                        this.gameObject.transform.position = GameObject.Find("EnforcerSpawn (1)").transform.position;
                        break;
                    default:
                        Debug.Log("Oops enforcer spawn");
                        break;
                }
            }
        }

        #endregion

        #region RPC functions

        [PunRPC]
        public void HitByBullet(int viewId, int bulletId)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonView.Find(bulletId) != null)
                {
                    PhotonNetwork.Destroy(PhotonView.Find(bulletId).gameObject);
                }
            }
            if (viewId != this.photonView.ViewID)
            {
                PhotonView.Find(viewId).gameObject.GetComponent<PlayerManager>().Health -= 0.1f;
            }
        }


        [PunRPC]
        public void PlayerDead(int pvId)
        {
            if (this.photonView.IsMine)
            {
                mySpriteRenderer.sprite = deadEaterSprite;
                this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                this.photonView.RPC("showDeadPlayer", RpcTarget.All, pvId);
                IEnumerator coroutine = RespawnWaiter(pvId);
                StartCoroutine(coroutine);
            }
        }
        
        [PunRPC]
        public void PlayerRespawn(int pvId, Vector3 pos)
        {
            if (this.photonView.ViewID == pvId)
            {
                mySpriteRenderer.sprite = eaterSprite;
                this.gameObject.GetComponent<BoxCollider2D>().enabled = true;
                this.gameObject.transform.position = pos;
                Health = 1f;
                isAlive = true;
                this.photonView.RPC("showAlivePlayer", RpcTarget.All, pvId);
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
        void showDeadPlayer(int viewId)
        {
            SpriteRenderer playerSprite = PhotonView.Find(viewId).gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
            playerSprite.sprite = deadEaterSprite;
        }

        [PunRPC]
        void showAlivePlayer(int viewId)
        {
            SpriteRenderer playerSprite = PhotonView.Find(viewId).gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
            playerSprite.sprite = eaterSprite;
        }

        [PunRPC]
        void PickUpFood(int _foodId)
        {
            if (photonView.IsMine) {
                Debug.Log("I am here");
                Debug.Log("_foodId = " + _foodId);
                Debug.Log(PhotonView.Find(_foodId).gameObject.name);
                gsm.Respawn(PhotonView.Find(_foodId).gameObject);
            }
            else
            {
                Debug.Log("Someone else is accessing this");
            }
        }

        [PunRPC]
        void PickUpGun(int gunId, int playerId)
        {
            PhotonView player = PhotonView.Find(playerId);
            PhotonView gun = PhotonView.Find(gunId);
            GameObject gunObj = gun.gameObject;
            GameObject playerObj = player.gameObject;
            gunObj.transform.parent = playerObj.transform;
            gunObj.transform.position = playerObj.transform.position;
            gunObj.transform.rotation = playerObj.transform.rotation;
        }

        [PunRPC]
        void ShootGun(int gunId)
        {
            PhotonView.Find(gunId).gameObject.GetComponent<Shoot>().ShootGun();
        }
        #endregion


        IEnumerator RespawnWaiter(int pvId)
        {
            yield return new WaitForSeconds(10f);
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("PlayerSpawn");
            int spawnPoint = UnityEngine.Random.Range(0, spawns.Length);
            photonView.RPC("PlayerRespawn", RpcTarget.All, pvId, spawns[spawnPoint].transform.position);
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

            GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);

            cameraMovement.GetCamera();
            if (this.photonView.IsMine)
            {
                gsm = GameObject.Find("Game State Manager").GetComponent<GameStateManager>();
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

        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endregion
    }
}
