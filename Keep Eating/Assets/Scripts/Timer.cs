using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

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
    void Start()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startTime = PhotonNetwork.Time;
            startTimer = true;
            pV.RPC("SetTimer", RpcTarget.Others, startTime);
        }
        
        timerText.text = "" + timer;
    }

    [PunRPC]
    void SetTimer(double _startTime)
    {
        startTime = _startTime;
        startTimer = true;
    }

    void Update()
    {
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
