
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


        private void Start()
        {
            teamsManager = GameObject.Find("Team Manager(Clone)").GetComponent<PhotonTeamsManager>();
            SetTargets();
        }


        // Update is called once per frame
        void Update()
        {
            if (target == null)
            {
                target = GetTarget();
            }

            if (!target.GetComponent<SpriteRenderer>().enabled)
            {
                target = GetTarget();
            }

            float step = speed * Time.deltaTime;
            myTransform.position = Vector3.MoveTowards(myTransform.position, target.transform.position, step);
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
                    tempDistance = TargetDistance(item.transform.position);
                    if (tempDistance < targetDistance)
                    {
                        targetDistance = tempDistance;
                        retTarget = item;
                    }
                }
            }
            else
            {
                if (!this.hasGun)
                {
                    foreach (GameObject item in itemTargets)
                    {
                        tempDistance = TargetDistance(item.transform.position);
                        if (tempDistance < targetDistance)
                        {
                            targetDistance = tempDistance;
                            retTarget = item;
                        }
                    }
                }
                else
                {
                    foreach (GameObject item in enemyTargets)
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
                        enemyTargets[index++] = player;
                    }
                }
            }
        }

        float TargetDistance(Vector3 targetPos)
        {
            return Mathf.Sqrt(Mathf.Pow(targetPos.x - myTransform.position.x, 2) + Mathf.Pow(targetPos.y - myTransform.position.y, 2));
        }

    }
}
