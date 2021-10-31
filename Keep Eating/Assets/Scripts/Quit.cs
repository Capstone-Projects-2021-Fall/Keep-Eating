using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quit : MonoBehaviour
{
    public void QuitConsole()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
