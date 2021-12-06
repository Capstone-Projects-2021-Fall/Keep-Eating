/*
        This script controls the timer.
        Might move to GameStateManager class in the future.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Com.tuf31404.KeepEating {
    public class Timer : MonoBehaviour
    {
        bool startTimer = false;
        double timerIncrementValue;
        double startTime;
        [SerializeField]
        double timer = 20;
        public Text timerText;
        [SerializeField]
        PhotonView pV;
        public bool StartGame { get; set; }

        [PunRPC]
        void SetTimer(double _startTime)
        {
            startTime = _startTime;
            startTimer = true;
        }

        void Update()
        {
            if (StartGame)
            {
                //Master Client gets the start time and sends it to the other players.
                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    startTime = PhotonNetwork.Time;
                    startTimer = true;

                    pV.RPC("SetTimer", RpcTarget.AllBuffered, startTime);
                }

                timerText.text = "" + timer;
                StartGame = false;
            }

            if (!startTimer) return;

            timerIncrementValue = PhotonNetwork.Time - startTime;

            if (timerIncrementValue > timer)
            {
                timerText.text = "Game Over";
            }
            else
            {
                timerText.text = "" + (int)(timer - timerIncrementValue);
            }
        }
    }
}
