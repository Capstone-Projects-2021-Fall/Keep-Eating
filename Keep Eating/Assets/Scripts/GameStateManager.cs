/*
    Initialized in the map.
    Controls the game (duh). 
    Keeps track of the stuff that causes the game to end.

    TODO: Spawn and respawn items.
                -This can probably be done with a coroutine. 
                -Make object invisible, call coroutine, coroutine waits, coroutine makes the object visible again.
          Spawn and respawn players.
 */



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;               
using Photon.Pun.UtilityScripts;            //needed for the PhotonTeamsManager class

public class GameStateManager : MonoBehaviour
{
    private PhotonTeamsManager teamManager;         //Gives access to team info. Specifically number of players.
    private int eatersDead;
    private int eaterPoints;
    private int pointsToWin;
    private Text hudText;                           //The text GameObject that displays the time.

    // Start is called before the first frame update
    void Start()
    {
        eatersDead = 0;
        eaterPoints = 0;
        pointsToWin = 100;
        teamManager = GameObject.Find("Team Manager").GetComponent<PhotonTeamsManager>();
        hudText = GameObject.Find("Timer").GetComponent<Text>();
    }

    // Checks for win conditions.
    void Update()
    {
        if (eatersDead == teamManager.GetTeamMembersCount(1))
        {
            GameOver("Death");
        }
        else if (eaterPoints == pointsToWin)
        {
            GameOver("Points");
        }
    }


    /*
        Called when endgame conition is met.
        Maybe called by Timer class???
        
        TODO: Timer call, game over process (destroy objects, return to lobby, etc.)
    */
    public void GameOver(string cause)
    {
        switch (cause)
        {
            case "Death":
                //enforcers win
                hudText.text = "Enforcers Win";
                break;
            case "Points":
                //eaters win
                hudText.text = "Eaters Win";
                break;
            case "Time":
                //tie??
                hudText.text = "Tie Game";
                break;
            default:
                Debug.Log("Oh shit something went wrong");
                break;
        }


    }

    
    /*
        TODO: This whole function. Maybe make a food enum instead of using a string.
              Have the eater call this when they eat something.
    */
    public void AddPoints(string food)
    {

    }

    public void Death()
    {
        eatersDead++;
    }

    public void Respawn()
    {
        eatersDead--;
    }
}
