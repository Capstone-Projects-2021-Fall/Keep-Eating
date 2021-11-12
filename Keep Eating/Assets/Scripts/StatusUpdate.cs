using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class StatusUpdate : MonoBehaviour
{
    public Text PlayerCountNumber;
    private int pre1 = 0;
    private int temp1 = 0;
    public Text RoomCountNumber;
    private int pre2 = 0;
    private int temp2 = 0;
    public Text PlayerInRoomNumber;
    private int pre3 = 0;
    private int temp3 = 0;
    public Text PlayerInMasterNumber;
    private int pre4 = 0;
    private int temp4 = 0;


    // Update is called once per frame
    private void Update()
    {
        pre1 = PhotonNetwork.CountOfPlayers - 1;
        pre2 = PhotonNetwork.CountOfRooms;
        pre3 = PhotonNetwork.CountOfPlayersInRooms;
        pre4 = PhotonNetwork.CountOfPlayersOnMaster - 1;

        if(pre1 != temp1 || pre2 != temp2 || pre3 != temp3 || pre4 != temp4)
        {
            temp1 = pre1;
            temp2 = pre2;
            temp3 = pre3;
            temp4 = pre4;
            refresh();
        }
    }

    private void refresh()
    {
        PlayerCountNumber.text = temp1.ToString();
        RoomCountNumber.text = temp2.ToString();
        PlayerInRoomNumber.text = temp3.ToString();
        PlayerInMasterNumber.text = temp4.ToString();

        Debug.Log("On time:" + DateTime.Now.ToString("HH:mm:ss tt") + " Player or room number changed!");
        Debug.Log("players: " + temp1 + " rooms: " + temp2 + " player in room: "+ temp3 + " player in main: "+ temp4);
    }
}
