using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public InputField createInput;
    public InputField joinInput;

    public void CreateRoom()
    {
        if (createInput.text == "")
        {
            var rand = new System.Random();
            char[] code = new char[5];
            for (int i = 0; i < 5; i++)
            {
                int unicode = rand.Next(65, 91);
                char letter = (char)unicode;
                code[i] = letter;
            }
            string codeString = new string(code);
            Debug.Log("Code: " + codeString);

            PhotonNetwork.CreateRoom(codeString);
        }
        else
        {
            PhotonNetwork.CreateRoom(createInput.text);
        }
    }

    public void JoinRoom()
    {
        Debug.Log(joinInput.text);
        if (joinInput.text == "")
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.JoinRoom(joinInput.text);
        }
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Lobby");
    }

}
