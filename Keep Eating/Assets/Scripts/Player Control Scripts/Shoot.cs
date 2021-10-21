using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{

    public Rigidbody2D bullet;
    public Transform barrel;

    // Start is called before the first frame update
 

    // Update is called once per frame
    public void ShootGun()
    {
        if (this.gameObject.name == "Revolver")
        {
            Instantiate(bullet, barrel.position, barrel.rotation);
        }
        else if (this.gameObject.name == "Shotgun")
        {
            Instantiate(bullet, barrel.position, barrel.rotation);
            Instantiate(bullet, barrel.position, barrel.rotation).GetComponent<RevolverBulletMove>().ShotgunBullet(1);
            Instantiate(bullet, barrel.position, barrel.rotation).GetComponent<RevolverBulletMove>().ShotgunBullet(2);
        }
    }
}


