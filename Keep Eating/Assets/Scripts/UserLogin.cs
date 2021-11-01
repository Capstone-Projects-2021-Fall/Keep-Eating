using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class UserLogin : MonoBehaviour
{

    public InputField userNameField;
    public InputField passwordField;
    public Button loginButton;
    public Text Prompt;

    // Start is called before the first frame update
    void Start()
    {
        //Subscribe to onClick event
        loginButton.onClick.AddListener(adminDetails);
    }

    Dictionary<string, string> staffDetails = new Dictionary<string, string>
    {
        {"admin", "admin"}
    };


    public void adminDetails()
    {
        //Get Username from Input
        string userName = userNameField.text;

        //Get Password from Input 
        string password = passwordField.text;

        string foundPassword;
        if (staffDetails.TryGetValue(userName, out foundPassword) && (foundPassword == password))
        {
            Prompt.text = "Logging in...";
            Debug.Log("User authenticated");
            SceneManager.LoadScene("Dashboard");
        }
        else
        {
            Prompt.text = "Invalid Username or Password";
            Debug.Log("Invalid password");
        }
    }
}
