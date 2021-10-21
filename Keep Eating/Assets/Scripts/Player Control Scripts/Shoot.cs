using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{

    public Rigidbody2D bullet;
    public Transform barrel;

    // Start is called before the first frame update
 

    // Update is called once per frame
    void ShootGun()
    {
                Instantiate(bullet, barrel.position, barrel.rotation);
    }
}


