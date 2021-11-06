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
        //bool startGameTimer = false;
        double timerIncrementValue;
        double startTime;
        double startGameTimer = 10;
        [SerializeField]
        double timer = 20;
        public Text timerText;
        [SerializeField]
        PhotonView pV;

        public bool StartGameTime { get; set; }

        void Awake()
        {
            //Master Client gets the start time and sends it to the other players.
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Debug.Log("Timer Awake");
                this.StartGameTime = true;
            }
        }

        void Start()
        {
            //Master Client gets the start time and sends it to the other players.
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                startTime = PhotonNetwork.Time;
                pV.RPC("SetTimer", RpcTarget.AllBuffered, startTime, true);
                Debug.Log("Timer start -- startTime = " + startTime + " startGameTimer = " + startGameTimer);
            }

            timerText.text = "" + startGameTimer;
        }

        [PunRPC]
        void SetTimer(double _startTime, bool startGame)
        {
            startTime = _startTime;
            if (startGame)
            {
                this.StartGameTime = true;
            }
            else
            {
                startTimer = true;
                this.StartGameTime = false;
            }
        }

        void Update()
        {
            
            
            if (startTimer)
            {
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
            else
            {
                timerIncrementValue = PhotonNetwork.Time - startTime;
                //Debug.Log("in update -- timer incr = " + timerIncrementValue);
                if (timerIncrementValue > startGameTimer)
                {
                    timerText.text = "Game Start!";
                    StartCoroutine("StartGame");
                }
                else if (this.StartGameTime)
                {
                    timerText.text = "" + (int)(startGameTimer - timerIncrementValue);
                }
            }
        }

        IEnumerator StartGame()
        {
            yield return new WaitForSeconds(2f);
            startTime = PhotonNetwork.Time;
            startTimer = true;
            timerText.text = "" + startTime;
            pV.RPC("SetTimer", RpcTarget.AllBuffered, startTime, false);
        }
    }
}
