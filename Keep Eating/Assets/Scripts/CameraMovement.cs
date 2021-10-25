using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Com.tuf31404.KeepEating
{
    public class CameraMovement : MonoBehaviour
    {
        public Transform cameraTransform;
        public float smoothing = 0.5f;
        public Transform playerTransform;
        private bool isFollowing = false;

        void Start()
        {
            Debug.Log("camera movement start");
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
            }
        }
    }
}
