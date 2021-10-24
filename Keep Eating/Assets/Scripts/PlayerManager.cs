using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public float speed;
    private Vector3 pos, scale;        //2D vector
    GameObject weapon;
    bool hasWeapon = false;
    [Tooltip("The current Health of our player")]
    public float Health = 1f;



    private void Start()
    {
        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();

        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

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
            }
        }
        else if (collision.gameObject.tag == "Food")
        {

        }
    }


}
