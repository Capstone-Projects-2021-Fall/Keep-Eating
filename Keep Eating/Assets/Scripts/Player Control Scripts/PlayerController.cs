using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    private Vector3 pos, scale;        //2D vector
    GameObject weapon;
    bool hasWeapon = false;


 
    // Update is called once per frame
    void Update()
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

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Gun")
        {
            scale = new Vector3(.22f, .22f, 0f);
            weapon = collision.gameObject;

            if (Input.GetKeyDown(KeyCode.F))
            {
                weapon = Instantiate(weapon, this.gameObject.transform);
                weapon.transform.localScale = scale;
                Destroy(collision.gameObject);
                hasWeapon = true;
            }
        }
    }


}
