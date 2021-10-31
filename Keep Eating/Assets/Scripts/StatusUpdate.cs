using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class StatusUpdate : MonoBehaviour
{
    public Text PlayerCountNumber;
    public Text RoomCountNumber;
    public Text PlayerInRoomNumber;
    public Text PlayerInMasterNumber;
    private int counter = 0;
    private int playerCount;

    // Update is called once per frame
    private void Update()
    {
        playerCount = PhotonNetwork.CountOfPlayers - 1;
        PlayerCountNumber.text = playerCount.ToString();
        RoomCountNumber.text = PhotonNetwork.CountOfRooms.ToString();
        PlayerInRoomNumber.text = PhotonNetwork.CountOfPlayersInRooms.ToString();
        PlayerInMasterNumber.text = PhotonNetwork.CountOfPlayersOnMaster.ToString();
        
        counter++;
        if(counter == 300)
        {
            Debug.Log(PhotonNetwork.CountOfPlayers.ToString() + PhotonNetwork.CountOfRooms.ToString() + 
                        PhotonNetwork.CountOfPlayersInRooms.ToString() + PhotonNetwork.CountOfPlayersOnMaster.ToString());
            counter = 0;
        }
        
    }
}
