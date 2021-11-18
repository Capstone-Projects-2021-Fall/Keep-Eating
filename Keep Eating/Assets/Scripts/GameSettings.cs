using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameSettings : MonoBehaviour
{

    [SerializeField]
    private Canvas settingsCanvas;
    [SerializeField]
    private Button bigMap, smallMap, toggleBots, makePrivate, exit;
    [SerializeField]
    private InputField maxPlayersInput;

    private void Start()
    {
        StaticSettings.SetVars();
        //ChangeMap("SmallGameMap");
        maxPlayersInput.text = "" + StaticSettings.MaxPlayers;
        TogglePrivate();
    }

    public void OpenSettingsMenu()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            settingsCanvas.enabled = true;
        }
    }

    public void CloseSettingsMenu()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            settingsCanvas.enabled = false;
        }
    }

    public void ChangeMap(string chosenMap)
    {
        StaticSettings.Map = chosenMap;

        if (chosenMap.Equals("SmallGameMap"))
        {
            smallMap.GetComponent<Image>().color = Color.red;
            bigMap.GetComponent<Image>().color = Color.white;
           
        }
        else
        {
            smallMap.GetComponent<Image>().color = Color.white;
            bigMap.GetComponent<Image>().color = Color.red;
        }
    }

    public void SetMaxPlayers()
    {

        int num;
        if (!int.TryParse(maxPlayersInput.text, out num))
        {
            Debug.LogError("Input a number!!!!!!!!!");
            maxPlayersInput.text = "" + StaticSettings.MaxPlayers;
        }

        if (num < 1 || num > 14)
        {
            Debug.LogError("Bad Number !!!!!!!!!!");
            maxPlayersInput.text = "" + StaticSettings.MaxPlayers;
        }

        StaticSettings.MaxPlayers = num;

        /*
         
          If there are 5 or less playersthe default map will be the small map. If more than 5, it will default to the big game map

         */
        if (num <= 5)
        {
            ChangeMap("SmallGameMap");
        }
        else
        {
            ChangeMap("BigGameMap");
        }

        PhotonNetwork.CurrentRoom.MaxPlayers = (byte)StaticSettings.MaxPlayers;
    }

    public void TogglePrivate()
    {
        if (StaticSettings.IsPrivate)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            StaticSettings.IsPrivate = false;
            makePrivate.GetComponent<Image>().color = Color.white;
        }        
        else
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            StaticSettings.IsPrivate = true;
            makePrivate.GetComponent<Image>().color = Color.red;
        }
    }

    public void ToggleBots()
    {
        if (StaticSettings.Bots)
        {
            StaticSettings.Bots = false;
            toggleBots.GetComponent<Image>().color = Color.white;
        }
        else
        {
            StaticSettings.Bots = true;
            toggleBots.GetComponent<Image>().color = Color.red;
        }
    }
}
