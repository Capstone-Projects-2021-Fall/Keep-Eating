using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    private float timer;
    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        timer = 60.0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        text.text = timer.ToString();

        if (timer <= 0)
        {
            Debug.Log("Timer Restarted");
            timer = 300.0f;
        }
    }

}
