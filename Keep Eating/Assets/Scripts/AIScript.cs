/********************************************************************
 * 
 * Script to control the AI
 * 
 * Clamp Values: minX, maxX, minY, maxY
 *    Small Map; -150,  152, -108,  82
 * 
 ********************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        private Sprite shotgunSprite, revolverSprite, taserSprite;
        [SerializeField]
        private SpriteRenderer mySpriteRenderer, weaponSpriteRenderer;
        [SerializeField]
        private Shoot shootScript;
        [SerializeField]
        private Transform muzzleTransform;
        [SerializeField]
        private GameObject bulletPrefab;
        //object variables
        private PhotonTeamsManager teamsManager;
        private int bulletsShot;
        //booleans
        private bool hasGun = false;                                
        //strings
        private Items weaponType;
        private string tempItemName;
        private Items tempFoodType;
        [Tooltip("The current Health of our player")]
        public float Health = 1f;


        private GameObject target;
        private GameObject[] enemyTargets;
        private GameObject[] itemTargets;
        private GameObject[] nodes;
        [SerializeField]
        private Transform myTransform;
        [SerializeField]
        private PhotonView thisPV;
        private int shootDistance;
        private bool canShoot;
        [SerializeField]
        private int fieldOfVisionX, fieldOfVisionY;
        private float minX, maxX, minY, maxY;
        private bool wandering;
        private Vector3 wanderTarget;
        private bool hasTarget, newWander;
        private BotMap botMap;
        public PhotonView PV { get; set; }
        public bool IsAlive { get; set; }

        public bool isAlpha = false;

        public bool hasDijkstraTarget = false;
        public bool inDijkstra = false;
        [SerializeField]
        private int[] shortestPath;
        private int pathCounter = 1;
        int dijkstraTarget = -1;
        int currentNode = -1;


        private void Start()
        {
            
            teamsManager = GameObject.Find("Team Manager(Clone)").GetComponent<PhotonTeamsManager>();
            SetTargets();
            shootDistance = 0;
            canShoot = true;
            hasTarget = false;
            newWander = true;
            wanderTarget = Vector3.zero;
            target = null;
            IsAlive = false;
            if (StaticSettings.Map.Equals("SmallGameMap"))
            {
                minX = -150;
                maxX = 152;
                minY = -108;
                maxY = 82;
                wandering = true;
            }
            else
            {
                minX = -250f;
                maxX = 250f;
                minY = -235f;
                maxY = 235f;
                wandering = false;
                // Debug.Log("Nodes.Length = " + nodes.Length);
                botMap = new BotMap(nodes.Length);
                shortestPath = new int[36];
                ResetPath();
                //SetBotMap();
                //botMap.PrintMap();
                StartCoroutine("WaitSetBotMap");
            }
        }


        // Update is called once per frame
        void Update()
        {
            if (IsAlive)
            {

                if (StaticSettings.Map.Equals("SmallGameMap"))
                {
                    SmallMapMove();
                }
                else
                {
                    BigMapMove();
                }

                if (isEater && Health <= 0f)
                {
                    //GameManager.Instance.LeaveRoom();
                    IsAlive = false;
                    hasTarget = false;
                    hasDijkstraTarget = false;
                    pathCounter = 1;
                    currentNode = -1;
                    PV.RPC("PlayerDead", RpcTarget.All, thisPV.ViewID);
                    IEnumerator coroutine = RespawnWaiter(thisPV.ViewID);
                    StartCoroutine(coroutine);
                }
                this.gameObject.transform.GetChild(0).rotation = Quaternion.identity;
            }
        }
        
        private void BigMapMove()
        {

            if (!hasTarget)
            {
                target = GetTarget();
            }
            else
            {
                if (!TargetInView(target.transform.position))
                {
                    GameObject newTarget = GetTarget();
                    if (TryRayCast(myTransform.position, newTarget.transform.position))
                    {
                        target = newTarget;
                    }
                    else if (TargetDistance(myTransform.position, newTarget.transform.position, false) < TargetDistance(myTransform.position, target.transform.position, false))
                    {
                        target = newTarget;
                    }
                }

                if (target.tag.Equals("Player") || target.tag.Equals("EaterAI"))
                {
                    if (!target.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled)
                    {
                        //target = GetTarget();
                        hasTarget = false;
                        target = null;
                        inDijkstra = false;
                        
                        //Debug.Log("Eater dead");
                    }
                }
                else if (!target.GetComponent<SpriteRenderer>().enabled)
                {
                    //target = GetTarget();
                    hasTarget = false;
                    target = null;
                    inDijkstra = false;
                    
                }
            }

            if (target != null)
            {
                if (TargetInView(target.transform.position))
                {
                    if (TryRayCast(myTransform.position, target.transform.position))
                    {
                        //move towards target
                        hasTarget = true;
                        hasDijkstraTarget = false;
                        currentNode = -1;
                        TryShoot();
                    }
                    else if (!hasDijkstraTarget)
                    {
                        //get closest node to target
                        dijkstraTarget = GetClosestNode(target.transform.position);
                        //dijksrta closest node to target
                        if (dijkstraTarget != -1)
                        {
                            GetDjikstra(dijkstraTarget, true);
                            hasDijkstraTarget = true;
                        }
                    }
                }
            }
            else
            {
                hasTarget = false;
            }

            if (!hasDijkstraTarget && target != null)
            {
                if (TryRayCast(myTransform.position, target.transform.position))
                {
                    float step = speed * Time.deltaTime;
                    myTransform.position = Vector3.MoveTowards(myTransform.position, target.transform.position, step);
                    RotateTo(target.transform.position);
                }
                else
                {
                    //get closest node to target
                    dijkstraTarget = GetClosestNode(target.transform.position);
                    //dijksrta closest node to target
                    if (dijkstraTarget != -1)
                    {
                        GetDjikstra(dijkstraTarget, true);
                        hasDijkstraTarget = true;
                    }
                    if (this.transform.gameObject.GetComponent<PhotonView>().ViewID == 20)
                    {
                        Debug.Log("target = " + dijkstraTarget);
                        Debug.Log("target pos = " + target.transform.position.ToString());
                    }
                }
            }
            else if (!inDijkstra)
            {
                ResetPath();
                GetDjikstra(UnityEngine.Random.Range(0,36), false);
                hasDijkstraTarget = true;
            }
            else
            {
                if (shortestPath[pathCounter] != -1)
                {
                    if (TargetDistance(nodes[shortestPath[pathCounter]].transform.position, myTransform.position, false) == 0)
                    {
                        currentNode = shortestPath[pathCounter];
                        pathCounter++;
                    }
                }
                else
                {
                    inDijkstra = false;
                    hasDijkstraTarget = false;
                }
                if (pathCounter <= 35)
                {
                    if (shortestPath[pathCounter] != -1)
                    {
                        if (TryRayCast(myTransform.position, nodes[shortestPath[pathCounter]].transform.position))
                        {
                            float step = speed * Time.deltaTime;
                            myTransform.position = Vector3.MoveTowards(myTransform.position, nodes[shortestPath[pathCounter]].transform.position, step);
                            RotateTo(nodes[shortestPath[pathCounter]].transform.position);
                        }
                        else
                        {
                            inDijkstra = false;
                            hasDijkstraTarget = false;
                        }
                    }
                    else
                    {
                        inDijkstra = false;
                        hasDijkstraTarget = false;
                    }
                }
                else
                {
                    inDijkstra = false;
                    hasDijkstraTarget = false;
                }
            }

            myTransform.position = new Vector3(
                    Mathf.Clamp(myTransform.position.x, minX, maxX),
                    Mathf.Clamp(myTransform.position.y, minY, maxY),
                    0.0f);
        }

        private void SmallMapMove()
        {

            if (!hasTarget)
            {
                target = GetTarget();
            }
            else
            {
                if (TargetInView(target.transform.position))
                {
                    if (target.tag.Equals("Player") || target.tag.Equals("EaterAI"))
                    {
                        if (!target.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled)
                        {
                            target = GetTarget();
                        }
                    }
                    else if (!target.GetComponent<SpriteRenderer>().enabled)
                    {
                        target = GetTarget();
                    }
                }
                else
                {
                    target = null;
                }
            }

            if (target != null)
            {
                hasTarget = true;
                wandering = false;

                TryShoot();
            }
            else
            {
                hasTarget = false;
                if (newWander || myTransform.position == wanderTarget)
                {
                    wanderTarget = Wander();
                }
                wandering = true;
            }
            Move(wandering);
        }

        private void TryShoot()
        {
            if ((target.tag.Equals("Player") || target.tag.Equals("EaterAI")) && hasGun && target.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled)
            {
                if (TargetDistance(target.transform.position, myTransform.position, false) <= shootDistance && canShoot)
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
       
        void Move(bool _isWandering)
        {
            float step = speed * Time.deltaTime;
            if (_isWandering)
            {
                myTransform.position = Vector3.MoveTowards(myTransform.position, wanderTarget, step);
                RotateTo(wanderTarget);
            }
            else
            {
                myTransform.position = Vector3.MoveTowards(myTransform.position, target.transform.position, step);
                RotateTo(target.transform.position);
            }

            myTransform.position = new Vector3(
                    Mathf.Clamp(myTransform.position.x, minX, maxX),
                    Mathf.Clamp(myTransform.position.y, minY, maxY),
                    0.0f);
        }

        private void RotateTo(Vector3 targetPos)
        {
            Vector3 direction = targetPos - myTransform.position;
            direction.z = 0;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
            myTransform.rotation = rotation;
        }

        private int GetClosestNode(Vector3 _targetPos)
        {
            int closestNode = -1;
            float prev = Mathf.Infinity;
            for (int i = 0; i < nodes.Length; i++)
            {
                float temp;
                if (_targetPos != myTransform.position)
                {
                    temp = TargetDistance(_targetPos, nodes[i].transform.position, true);
                }
                else
                {
                    temp = TargetDistance(_targetPos, nodes[i].transform.position, false);
                }
                if (temp < prev)
                {
                    if (TryRayCast(_targetPos, nodes[i].transform.position))
                    {
                        prev = temp;
                        closestNode = i;
                    }
                }
            }
            if (closestNode == -1)
            {
                Debug.Log("CLOSEST NODE ERROR");
                Debug.Log("error view = " + this.gameObject.GetComponent<PhotonView>().ViewID);
            }
            return closestNode;
        }

        void GetDjikstra(int num, bool hasTarget)
        {
            inDijkstra = true;
            pathCounter = 1;
            if (currentNode == -1)
            {
                currentNode = GetClosestNode(myTransform.position);
                if (this.gameObject.GetComponent<PhotonView>().ViewID == 20)
                {
                    Debug.Log("closest node " + currentNode);
                }
            }

            int targetNode = num;
            if (!hasTarget)
            {
                while (targetNode == currentNode)
                {
                    targetNode = UnityEngine.Random.Range(0, 36);
                }
            }
            
            int[] tempPath = botMap.Dijkstra(currentNode, targetNode);
            shortestPath[0] = currentNode;
            for (int i = tempPath.Length-1; i >= 0; i--)
            {
                if (tempPath[i] != -1)
                {
                    shortestPath[pathCounter++] = tempPath[i];
                }
            }
            pathCounter = 0;   
        }

        GameObject GetTarget()
        {
            GameObject retTarget = null;
            float targetDistance = 10000f;
            float tempDistance = 0;

            if (isEater)
            {
                foreach (GameObject item in itemTargets)
                {
                    if (item != null && item.GetComponent<SpriteRenderer>().enabled)
                    {
                        tempDistance = TargetDistance(item.transform.position, myTransform.position, false);
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
                            tempDistance = TargetDistance(item.transform.position, myTransform.position, false);
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
                    foreach (GameObject item in enemyTargets)
                    {
                        if (item != null && item.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled)
                        {
                            tempDistance = TargetDistance(item.transform.position, myTransform.position, false);
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
        
        Vector3 Wander()
        {
            float xPos = UnityEngine.Random.Range(-150, 152);
            float yPos = UnityEngine.Random.Range(-108, 82);
            StartCoroutine("WanderWaiter");
            return new Vector3(xPos, yPos, 0f);
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
                //Debug.Log("weapons size = " + itemTargets.Length);
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                GameObject[] eaterAI = GameObject.FindGameObjectsWithTag("EaterAI");
                enemyTargets = new GameObject[players.Length + eaterAI.Length];
                int index = 0;
                foreach (GameObject player in players)
                {
                    if (player.GetComponent<PlayerManagerV2>().MyTeam == 1)
                    {

                       // Debug.Log("Adding Enemy targets");
                        enemyTargets[index++] = player;
                    }
                }
                foreach (GameObject eater in eaterAI)
                {
                    enemyTargets[index++] = eater;
                }
            }
            if (StaticSettings.Map.Equals("BigGameMap"))
            {
                nodes = new GameObject[36];

                string n = "Node";
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i] = GameObject.Find(n + i);
                }
            }
        }

        float TargetDistance(Vector3 _a, Vector3 _b, bool other)
        {
            if (!other)
            {
                if (TargetInView(_b))
                {
                    return Mathf.Sqrt(Mathf.Pow(_b.x - _a.x, 2) + Mathf.Pow(_b.y - _a.y, 2));
                }
                else
                {
                    return 10001f;
                }
            }
            else return Mathf.Sqrt(Mathf.Pow(_b.x - _a.x, 2) + Mathf.Pow(_b.y - _a.y, 2));
        }

        bool TargetInView(Vector3 targetPos)
        {
            float distX = Mathf.Abs(targetPos.x - myTransform.position.x);
            float distY = Mathf.Abs(targetPos.y - myTransform.position.y);
            if (distX < fieldOfVisionX && distY < fieldOfVisionY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void ResetPath()
        {
            for (int i = 0; i < shortestPath.Length; i++)
            {
                shortestPath[i] = -1;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.name.Contains("Weapon") && !hasGun && !isEater)
            {
                tempItemName = other.gameObject.name;
                weaponType = other.gameObject.GetComponent<ItemSpawnScript>().ItemType;
                Debug.Log("weapon type = " + weaponType);
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
                hasTarget = false;
                target = null;
            }
            else if (other.name.Contains("Food") && isEater)
            {
                tempItemName = other.gameObject.name;
                tempFoodType = other.gameObject.GetComponent<ItemSpawnScript>().ItemType;
                this.PV.RPC("PickUpFood", RpcTarget.All, tempItemName, tempFoodType);
                hasTarget = false;
                target = null;
                inDijkstra = false;
                hasDijkstraTarget = false;
            }

            Debug.Log("trigger");
            if (other.gameObject.CompareTag("Bullet") && isEater)
            {
                Debug.Log("Ouch");
                Health -= 0.3f;
            }

            if (other.gameObject.CompareTag("Taser Bullet") && !isEater)
            {
                StartCoroutine(FreezeRoutine());
            }
        }

        private IEnumerator FreezeRoutine()
        {
            //Debug.Log("Freeze Coroutine");
            //Constrains players movement in any direction for 5 seconds before allowing movement to resume
            //this.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            IsAlive = false;
            yield return new WaitForSeconds(10);
            IsAlive = true;
        }

            private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("Collision");
            if (collision.gameObject.CompareTag("Bullet") && isEater)
            {
                Debug.Log("Ouch");
                Health -= 1f;
            }
        }
        IEnumerator ShootWaiter()
        {
            canShoot = false;
            yield return new WaitForSeconds(1);
            canShoot = true;
        }

        IEnumerator WanderWaiter()
        {
            newWander = false;
            float waitTime = UnityEngine.Random.Range(0, 10);
            yield return new WaitForSeconds(waitTime);
            newWander = true;
        }

        IEnumerator WaitSetBotMap()
        {
            SetBotMap();
           // Debug.Log("Bot Map Set");
            yield return null; 
        }

        IEnumerator RespawnWaiter(int pvId)
        {
            myTransform.position = GameObject.FindGameObjectWithTag("Purgatory").transform.position;
            yield return new WaitForSeconds(10f);
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("EaterSpawn");
            if (spawns.Length != 0)
            {
                int spawnPoint = UnityEngine.Random.Range(0, spawns.Length);
                PV.RPC("PlayerRespawn", RpcTarget.All, pvId, spawns[spawnPoint].transform.position);
                GameObject.FindWithTag("GSM").GetComponent<GameStateManager>().PlayerRespawn();
            }
            else
            {
                PV.RPC("PlayerRespawn", RpcTarget.All, pvId, Vector3.zero);
            }
            IsAlive = true;
            Health = 1f;
        }

        private void SetBotMap()
        {
            Vector3 a, b;
            for (int i = 0; i < nodes.Length; i++)
            {
                for (int j = 0; j < nodes.Length; j++)
                {
                    if (i != j)
                    {
                        a = nodes[i].transform.position;
                        b = nodes[j].transform.position;
                        if (TryRayCast(a, b))
                        {
                            botMap.Add(a, b, i, j);
                        }
                    }
                }
            }
            if (isAlpha)
            {
                //botMap.PrintMap(nodes);
                //botMap.Dijkstra(1, 20);
            }
        }

        private bool TryRayCast(Vector3 _a, Vector3 _b)
        {
            float dist = Mathf.Sqrt(Mathf.Pow(_a.x - _b.x, 2) + Mathf.Pow(_a.y - _b.y, 2));
            Vector2 direction = new Vector2(_b.x - _a.x, _b.y - _a.y).normalized;
            Vector2 a = new Vector2(_a.x, _a.y);
            LayerMask mask = LayerMask.GetMask("Wall");
            RaycastHit2D hit = Physics2D.Raycast(a, direction, dist, mask);
         
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.CompareTag("Wall"))
                {
                    return false;
                }
                else
                {
                   // Debug.Log("somethin wrong");
                }
            }
            return true;
        }
    }
}


