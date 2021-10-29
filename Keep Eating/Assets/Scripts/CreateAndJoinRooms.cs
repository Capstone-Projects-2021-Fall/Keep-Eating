/*
    This is the script for the main menu.
    The name says it all.

    Note: A Room is what we refer to as a lobby.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


namespace Com.tuf31404.KeepEating
{
    public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
    {
        public InputField createInput;
        public InputField joinInput;


        //If no Code is entered, a random code is generated
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


        //If no lobby code is enter, you will join a random room
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

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("Loading Lobby Scene");

                /*
                    LoadLevel is the PhotonNetwork version of LoadScene.
                    This is only called by the Master Client (the person who created the room).
                    When other clients join the room, Photon automatically syncs their scene to
                    match the Master Client.
                 */
                PhotonNetwork.LoadLevel("Lobby");
            }
        }

    }
}
