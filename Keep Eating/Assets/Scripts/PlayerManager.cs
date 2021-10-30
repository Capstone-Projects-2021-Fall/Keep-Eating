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
        private Vector3 pos, scale;       
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
        public Sprite eaterSprite, enforcerSprite;
        private byte myTeam;
        Button eaterSwitch, enforcerSwitch;


        #region Init
        void Awake()
        {   
            //PhotonView.IsMine is used so this only runs on your player object.
            //This is needed because other players will also be running this script, but you don't
            //want them to run some of this code - like you only want your character to move when you press a key.
            if (photonView.IsMine)
            {
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }
           
            //Saves this gameObject instance when the scene is changed.
            DontDestroyOnLoad(this.gameObject);
        }


        private void Start()
        {
            //Only the player prefab that you control can call these methods.
            if (photonView.IsMine)
            {
                Debug.Log("PLAYER MANAGER START");
                //Camera movement - see CameraMovement script
                cameraMovement = this.gameObject.GetComponent<CameraMovement>();

                if (cameraMovement != null)
                {
                    if (photonView.IsMine)
                    {
                        cameraMovement.StartFollowing();
                    }
                    else
                    {
                        Debug.Log("Fuck");
                    }
                }

                UpdateTeamMax();
                Debug.Log("eaters = " + eaterTeamMax + " enforcers = " + enforcerTeamMax);
                teamsManager = GameObject.Find("Team Manager").GetComponent<PhotonTeamsManager>();
                //Trying to join a team (randomly) when you get in the lobby.
                TryJoinTeam((byte)UnityEngine.Random.Range(1, 3));
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

                if (PlayerUiPrefab != null)
                {

                    Debug.Log("UI not null");
                    GameObject _uiGo = Instantiate(PlayerUiPrefab);
                    Debug.Log("_uiGo name: " + _uiGo.name);
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

            }
        }

        #endregion

        #region Updates and Inputs
        void Update()
        {

            if (photonView.IsMine)
            {
                ProcessInputs();
                if (Health <= 0f)
                {
                    GameManager.Instance.LeaveRoom();
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
            photonView.RPC("SetTeam", RpcTarget.AllBuffered, myTeam, photonView.ViewID);
            //Changes sprite depending on team.
            if (myTeam == 1)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = eaterSprite;
            }
            else
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = enforcerSprite;
            }

        }

        void ProcessInputs()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

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

            if (weapon != null)
            {
                if (mousepos.x < 0)
                {
                    weapon.GetComponent<SpriteRenderer>().flipY = true;
                }
                else
                {
                    weapon.GetComponent<SpriteRenderer>().flipY = false;
                }
            }

            if (Input.GetButtonDown("Fire1"))
            {
                if (hasWeapon)
                {
                    Debug.Log("Shoot attempt");
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC("ShootGun", RpcTarget.MasterClient, this.gameObject.transform.GetChild(0).gameObject.GetPhotonView().ViewID);
                    }
                    else
                    {
                        gameObject.GetComponentInChildren<Shoot>().ShootGun();
                    }
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (!other.name.Contains("Bullet"))
            {
                return;
            }

            if (PhotonTeamExtensions.GetPhotonTeam(PhotonNetwork.LocalPlayer).Code == 1)
            {
                PhotonNetwork.Destroy(other.gameObject);
                Health -= 0.1f;
            }
            
        }
        void OnTriggerStay2D(Collider2D collision)
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (collision.gameObject.tag == "Gun")
            {
                scale = new Vector3(.22f, .22f, 0f);
                weapon = collision.gameObject;

                if (Input.GetKeyDown(KeyCode.F))
                {
                 
                    photonView.RPC("PickUpShotgun", RpcTarget.All, weapon.GetPhotonView().ViewID, LocalPlayerInstance.GetPhotonView().ViewID);

                    hasWeapon = true;
             
                }
            }
            else if (collision.gameObject.tag == "Food")
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC("PickUpFood", RpcTarget.MasterClient, 1);
                    }
                    else
                    {
                        PickUpFood(1);
                    }
                }
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
            Debug.Log("teamNum = " + teamNum);
            if (teamNum == 1)
            {
                if (teamsManager.GetTeamMembersCount(1) == eaterTeamMax){
                    teamNum = 2;
                }
            }
            else
            {
                if (teamsManager.GetTeamMembersCount(1) == eaterTeamMax){
                    teamNum = 1;
                }
            }

            if (!PhotonTeamExtensions.JoinTeam(PhotonNetwork.LocalPlayer, teamNum))
            {
                Debug.Log("Join Team fail");
            }

            myTeam = teamNum;
            photonView.RPC("SetTeam", RpcTarget.AllBuffered, teamNum, photonView.ViewID);
            if (teamNum == 1)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = eaterSprite;
            }
            else
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = enforcerSprite;
            }
        }

        #endregion

        #region RPC functions
        [PunRPC]
        void SetTeam(byte teamId, int viewId)
        {
            SpriteRenderer playerSprite = PhotonView.Find(viewId).gameObject.GetComponent<SpriteRenderer>();
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
        void PickUpFood(int foodId)
        {
            string food = "Food" + foodId;
            Debug.Log(food + " destroyed");
            PhotonNetwork.Destroy(GameObject.Find(food));
        }

        [PunRPC]
        void PickUpShotgun(int shotgunId, int playerId)
        {
            PhotonView player = PhotonView.Find(playerId);
            PhotonView shotgun = PhotonView.Find(shotgunId);
            GameObject shotgunObj = shotgun.gameObject;
            GameObject playerObj = player.gameObject;
            shotgunObj.transform.parent = playerObj.transform;
        }

        [PunRPC]
        void ShootGun(int gunId)
        {
            Debug.Log("in shoot rpc");
            PhotonView.Find(gunId).gameObject.GetComponent<Shoot>().ShootGun();
        }
        #endregion


        #region PunCallbacks

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            UpdateTeamMax();

            Debug.Log("eatersTeam = " + teamsManager.GetTeamMembersCount(1) + " enforcersTeam = " + teamsManager.GetTeamMembersCount(2));
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
