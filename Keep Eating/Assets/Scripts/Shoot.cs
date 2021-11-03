/*
        Contains the function to shoot the guns.
        Only called by the master client to make syncronization easier.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.tuf31404.KeepEating
{
    public class Shoot : MonoBehaviour
    {

        public Rigidbody2D bullet;
        public Transform muzzle;

        // Update is called once per frame
        public void ShootGun()
        {
            Debug.Log("in shootgun");
            if (this.gameObject.name.Contains("Revolver"))
            {
                //Instantiates one bullet
                PhotonNetwork.Instantiate("RevolverBullet", muzzle.position, muzzle.rotation);
            }
            else if (this.gameObject.name.Contains("Shotgun"))
            {
                Debug.Log("shooting shotgun");

                //Instantiates 5 bullets in different directions like a real shotgun!!!
                for (int i = 0; i < 5; i++)
                {
                    PhotonNetwork.Instantiate("ShotgunBullet", muzzle.position, muzzle.rotation);
                }
            }
        }

        public Vector3 ShootGun(Items weaponType)
        {
            Vector3 mousePos;
            Vector3 direction = new Vector3(0, 0, 0);
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);         //Gets the position of the mouse 
            if (weaponType.Equals("Shotgun"))
            {
                mousePos += Random.insideUnitSphere * 5;                            //This is where the MAGIC happens.
            }
            mousePos.z = 0;                                                         // z is set to 0 so the camera can see it
            direction = (mousePos - transform.position).normalized;
            direction = Quaternion.Euler(0, -45, 0) * direction;
            return direction;
        }
    }
}


