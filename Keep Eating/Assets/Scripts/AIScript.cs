
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
    public class AIScript : MonoBehaviour
    {

        [SerializeField]
        private bool isEater;
        //Serialized private variables
        [SerializeField]
        private float speed;
        [SerializeField]
        private GameObject PlayerUiPrefab;
        [SerializeField]
        private Sprite shotgunSprite, revolverSprite, taserSprite;
        [SerializeField]
        private SpriteRenderer mySpriteRenderer, weaponSpriteRenderer, taserSpriteRenderer;
        [SerializeField]
        private Shoot shootScript;
        [SerializeField]
        private Transform muzzleTransform;
        [SerializeField]
        private GameObject bulletPrefab;
        //object variables
        private PhotonTeamsManager teamsManager;
        private GameStateManager gsm;
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
        private bool hasGun = false;                                //change to hasWeapon
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


        private GameObject target;
        private GameObject[] enemyTargets;
        private GameObject[] itemTargets;
        [SerializeField]
        private Transform myTransform;
        [SerializeField]
        private PhotonView thisPV;
        private int shootDistance;
        private bool canShoot;

        public PhotonView PV { get; set; }


        private void Start()
        {
            teamsManager = GameObject.Find("Team Manager(Clone)").GetComponent<PhotonTeamsManager>();
            SetTargets();
            shootDistance = 0;
            canShoot = true;
        }


        // Update is called once per frame
        void Update()
        {
            if (target == null)
            {
                target = GetTarget();
            }
            else 
            {
                if (target.tag.Equals("Player"))
                {
                    if (!target.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled)
                    {
                        target = GetTarget();
                    }
                }
                else if (!target.GetComponent<SpriteRenderer>().enabled)
                {
                    target = GetTarget();
                }
            }

            if (target != null)
            {
                float step = speed * Time.deltaTime;
                myTransform.position = Vector3.MoveTowards(myTransform.position, target.transform.position, step);
                if (target.tag.Equals("Player") && hasGun)
                {
                    if (TargetDistance(target.transform.position) <= shootDistance && canShoot)
                    {
                        if (weaponType == Items.Shotgun)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                bulletsShot++;
                                this.PV.RPC("ShootGun", RpcTarget.All, PhotonNetwork.NickName, shootScript.ShootGun(weaponType, target.transform.position), muzzleTransform.position);
                            }
                        }
                        else
                        {
                            bulletsShot++;
                            this.PV.RPC("ShootGun", RpcTarget.All, PhotonNetwork.NickName, shootScript.ShootGun(weaponType, target.transform.position), muzzleTransform.position);
                        }
                        StartCoroutine("ShootWaiter");
                    }
                }
            }

        }

        GameObject GetTarget()
        {
            GameObject retTarget = null;
            float targetDistance = 100000f;
            float tempDistance = 0;

            if (isEater)
            {
                foreach (GameObject item in itemTargets)
                {
                    if (item != null && item.GetComponent<SpriteRenderer>().enabled)
                    {
                        tempDistance = TargetDistance(item.transform.position);
                        if (tempDistance < targetDistance)
                        {
                            targetDistance = tempDistance;
                            retTarget = item;
                        }
                    }
                }
            }
            else
            {
                if (!this.hasGun)
                {
                    foreach (GameObject item in itemTargets)
                    {
                        if (item != null && item.GetComponent<SpriteRenderer>().enabled)
                        {
                            tempDistance = TargetDistance(item.transform.position);
                            if (tempDistance < targetDistance)
                            {
                                targetDistance = tempDistance;
                                retTarget = item;
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Getting enemy target");
                    foreach (GameObject item in enemyTargets)
                    {
                        if (item != null && item.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled)
                        {
                            tempDistance = TargetDistance(item.transform.position);
                            if (tempDistance < targetDistance)
                            {
                                targetDistance = tempDistance;
                                retTarget = item;
                            }
                        }
                    }
                }
            }

            return retTarget;
        }

        void SetTargets()
        {
            if (isEater)
            {
                itemTargets = GameObject.FindGameObjectsWithTag("Food");
            }
            else
            {
                itemTargets = GameObject.FindGameObjectsWithTag("Weapon");
                enemyTargets = new GameObject[teamsManager.GetTeamMembersCount(1)];
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                int index = 0;
                foreach (GameObject player in players)
                {
                    if (player.GetComponent<PlayerManagerV2>().MyTeam == 1)
                    {

                        Debug.Log("Adding Enemy targets");
                        enemyTargets[index++] = player;
                    }
                }
            }
        }

        float TargetDistance(Vector3 targetPos)
        {
            return Mathf.Sqrt(Mathf.Pow(targetPos.x - myTransform.position.x, 2) + Mathf.Pow(targetPos.y - myTransform.position.y, 2));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.name.Contains("Weapon") && !hasGun && !isEater)
            {
                tempItemName = other.gameObject.name;
                weaponType = other.gameObject.GetComponent<ItemSpawnScript>().ItemType;
                hasGun = true;
                this.PV.RPC("PickUpGun", RpcTarget.All, thisPV.ViewID, weaponType, tempItemName);
                if (weaponType == Items.Shotgun)
                {
                    shootDistance = 60;
                }
                else
                {
                    shootDistance = 90;
                }
            }
            else if (other.name.Contains("Food") && isEater)
            {
                tempItemName = other.gameObject.name;
                tempFoodType = other.gameObject.GetComponent<ItemSpawnScript>().ItemType;
                this.PV.RPC("PickUpFood", RpcTarget.All, tempItemName, tempFoodType);
            }
        }

        IEnumerator ShootWaiter()
        {
            canShoot = false;
            yield return new WaitForSeconds(1);
            canShoot = true;
        }
    }
}
