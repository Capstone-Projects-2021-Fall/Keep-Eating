/*
        Contains the function to shoot the guns.
        Only called by the master client to make syncronization easier.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shoot : MonoBehaviour
{

    public Rigidbody2D bullet;
    public Transform barrel;

    // Update is called once per frame
    public void ShootGun()
    {
        Debug.Log("in shootgun");
        if (this.gameObject.name == "Revolver(Clone)")
        {
            //Instantiates one bullet
            PhotonNetwork.Instantiate("RevolverBullet", barrel.position, barrel.rotation);
        }
        else if (this.gameObject.name == "Shotgun1")
        {
            Debug.Log("shooting shotgun");

            //Instantiates 5 bullets in different directions like a real shotgun!!!
            for (int i = 0; i < 5; i++)
            {
                PhotonNetwork.Instantiate("ShotgunBullet", barrel.position, barrel.rotation);
            }
        }
    }
}


