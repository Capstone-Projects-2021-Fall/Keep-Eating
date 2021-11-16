/*
        Name says it all.
        Reads the player name input field and saves it.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(InputField))]
public class PlayerNameInput : MonoBehaviour
{

    const string playerNamePrefKey = "PlayerName";

    private void Start()
    {
        string defaultName = string.Empty;
        InputField _inputField = this.GetComponent<InputField>();

        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
    }

    public void SetPlayerName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.Log("Error setting player pref name");
            return;
        }
        PhotonNetwork.NickName = value;
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
