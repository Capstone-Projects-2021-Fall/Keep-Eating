/*
    Used by the PLAYER prefab.
    Controlls a bunch of player stuff.
 */



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

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

#if UNITY_5_4_OR_NEWER
            
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
#endif

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
        }

        
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
                    gameObject.GetComponentInChildren<Shoot>().ShootGun();
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

            Health -= 0.1f;
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
                   
                    /*
                    Debug.Log("hello?");
                    if (hasWeapon)
                    {
                        Destroy(weapon);
                    }
                    weapon = Instantiate(weapon, this.gameObject.transform);
                    weapon.transform.localScale = scale;
                    if (weapon.name == "Revolver(Clone")
                    {
                        weapon.transform.position = this.gameObject.transform.position + new Vector3(.88f, .88f, 0);
                    }
                    else
                    {
                        weapon.transform.position = this.gameObject.transform.position + new Vector3(1.5f, 0, 0);
                    }
                    Destroy(collision.gameObject);
                    hasWeapon = true;
                    */
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


#if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
            
        }
#endif


#if !UNITY_5_4_OR_NEWER
    /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
    void OnLevelWasLoaded(int level)
    {
        this.CalledOnLevelWasLoaded(level);
    }
#endif


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

#if UNITY_5_4_OR_NEWER
        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
#endif

    }
}
