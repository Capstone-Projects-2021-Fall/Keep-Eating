/*
    Used by the Player Manager script to make the camera follow your player.
    Everything in here is fairly straight forward.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Com.tuf31404.KeepEating
{
    public class CameraMovement : MonoBehaviour
    {
        public Transform cameraTransform;
        public Transform playerTransform;
        private bool isFollowing = false;

        void Start()
        {
            //Debug.Log("camera movement start");
            GetCamera();
            float camHeight, camWidth;
            camHeight = 2 * Camera.main.orthographicSize;
            camWidth = camHeight * Camera.main.aspect;
            Debug.Log("camera height = " + camHeight + " cam width = " + camWidth);
        }

        public void GetCamera()
        {
            cameraTransform = Camera.main.transform;
        }
        public void StartFollowing()
        {
            Debug.Log("start following CM");
            isFollowing = true;
        }

        void LateUpdate()
        {
            if (isFollowing)
            {
                //Debug.Log("following update");
                if (cameraTransform != null)
                {
                    cameraTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -10);
                }
                else
                {
                    Debug.Log("CAMERA NULL");
                }
            }
        }
    }
}
