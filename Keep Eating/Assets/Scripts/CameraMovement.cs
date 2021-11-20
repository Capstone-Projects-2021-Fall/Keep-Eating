/*
    Used by the Player Manager script to make the camera follow your player.
    Everything in here is fairly straight forward.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.tuf31404.KeepEating
{
    public class CameraMovement : MonoBehaviour
    {
        public Transform cameraTransform;
        public Transform playerTransform;
        private Transform followTransform;
        private bool isFollowing = false;
        private Transform[] otherPlayers;
        private int spectatingCounter;
        public bool Spectating { get; set; }

        void Start()
        {
            //Debug.Log("camera movement start");
            GetCamera();
            float camHeight, camWidth;
            camHeight = 2 * Camera.main.orthographicSize;
            camWidth = camHeight * Camera.main.aspect;
            Debug.Log("camera height = " + camHeight + " cam width = " + camWidth);
            Spectating = false;
            spectatingCounter = 0;
        }

        public void SetSpectating()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            GameObject[] eaterAI = GameObject.FindGameObjectsWithTag("EaterAI");
            GameObject[] enforcerAI = GameObject.FindGameObjectsWithTag("EnforcerAI");

            otherPlayers = new Transform[players.Length + eaterAI.Length + enforcerAI.Length - 1];

            foreach(GameObject player in players)
            {
                if (!player.GetComponent<PhotonView>().IsMine)
                {
                    otherPlayers[spectatingCounter++] = player.transform;
                }
            }

            foreach(GameObject eater in eaterAI)
            {
                otherPlayers[spectatingCounter++] = eater.transform;
            }

            foreach(GameObject enforcer in enforcerAI)
            {
                otherPlayers[spectatingCounter++] = enforcer.transform;
            }

            spectatingCounter = 0;
        }


        public void Spectate()
        {
            if (Spectating)
            {
                if (spectatingCounter + 1 >= otherPlayers.Length)
                {
                    spectatingCounter = 0;
                }
                else
                {
                    spectatingCounter++;
                }
            }
            else
            {
                Spectating = true;
            }
            followTransform = otherPlayers[spectatingCounter];
        }

        public void StopSpectating()
        {
            Spectating = false;
            followTransform = playerTransform;
        }

        public void GetCamera()
        {
            cameraTransform = Camera.main.transform;
        }

        public void StartFollowing()
        {
            Debug.Log("start following CM");
            followTransform = playerTransform;
            isFollowing = true;
        }

        void LateUpdate()
        {
            if (isFollowing)
            {
                //Debug.Log("following update");
                if (cameraTransform != null)
                {
                    cameraTransform.position = new Vector3(followTransform.position.x, followTransform.position.y, -10);
                }
                else
                {
                    Debug.Log("CAMERA NULL");
                }
            }
        }
    }
}
