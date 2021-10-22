using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class LobbyCodeGeneration : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        var rand = new System.Random();
        char[] code = new char[5];
        for (int i=0; i<5; i++)
        {
            int unicode = rand.Next(65, 91);
            char letter = (char)unicode;
            code[i] = letter;
        }
        string codeString = new string(code);
        Debug.Log("Code: "+ codeString);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
