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
    private Button bigMap, smallMap, makePrivate, exit;
    [SerializeField]
    private InputField maxPlayersInput;

    private bool isPrivate;

    public string Map { get; set; }
    public int MaxPlayers { get; set; }

    private void Start()
    {
        this.Map = "";
        this.MaxPlayers = 0;
        isPrivate = false;
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
        this.Map = chosenMap;
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
            maxPlayersInput.text = "" + this.MaxPlayers;
        }

        if (num < 1 || num > 14)
        {
            Debug.LogError("Bad Number !!!!!!!!!!");
            maxPlayersInput.text = "" + this.MaxPlayers;
        }

        if (this.Map.Equals("SmallGameMap") && num > 5)
        {
            this.MaxPlayers = 5;
            maxPlayersInput.text = "" + this.MaxPlayers;
        }
        else
        {
            this.MaxPlayers = num;
        }
    }

    public void TogglePrivate()
    {
        if (isPrivate)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            isPrivate = false;
            makePrivate.GetComponent<Image>().color = Color.white;
        }        
        else
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            isPrivate = true;
            makePrivate.GetComponent<Image>().color = Color.red;
        }
    }
}
