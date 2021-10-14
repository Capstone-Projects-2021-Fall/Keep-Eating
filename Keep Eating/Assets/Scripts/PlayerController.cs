using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string playerSessionId = "";
    public float speed = 5f;
    private Vector3 pos;        //2D vector

    

    /*
    // Update is called once per frame
    void Update()
    {
        
            Input.GetAxis() is for directional movement.
            Horizontal is mapped to the A and D keys.
            Vertical is mapped to the W and S keys.
            It returns a value between -1 and 1.

            For example:
            Input.GetAxis("Horizontal") = 1 when D is being pressed.
        
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //transform.position is the Game Object's position
        pos = transform.position;

        
            These can be read as:
            new position = old position + direction * rate of change * time since last frame
        
        pos.x += h * speed * Time.deltaTime;
        pos.y += v * speed * Time.deltaTime;

        transform.position = pos;
    }
    */

}
