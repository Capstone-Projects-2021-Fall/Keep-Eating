/***********************************************************
    This is an example script with some explaination. 

    This script is a component of the Circle Game Obeject.
    It executes when the Circle is instantiated and ends
    when the Circle is destroyed. It allows the user to 
    move the Circle with the WASD keys. 

***********************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
   MonoBehaviour is inhereted by default when creating a 
   script through the editor. It provides methods for 
   interacting with the engine (like start() and update()).
*/
public class TestAction : MonoBehaviour
{
    private int health = 300;
    public float speed = 5f;
    private Vector2 pos;        //2D vector

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {   
        /*
            Input.GetAxis() is for directional movement.
            Horizontal is mapped to the A and D keys.
            Vertical is mapped to the W and S keys.
            It returns a value between -1 and 1.

            For example:
            Input.GetAxis("Horizontal") = 1 when D is being pressed.
        */
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //transform.position is the Game Object's position
        pos = transform.position;

        /*
            These can be read as:
            new position = old position + direction * rate of change * time since last frame
        */
        pos.x += h * speed * Time.deltaTime;
        pos.y += v * speed * Time.deltaTime;

        transform.position = pos;

        if(Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        if(Input.GetMouseButtonDown(1))
        {
            PickUp();
        }

    }


    void Attack()
    {
        Debug.Log("Attacking...");
    }
    void PickUp()
    {
        Debug.Log("Picking up...");
    }

    void TakeDamage(int damageAmount)
    {
        health = health - damageAmount;

        if(health < 0)
        {
            //respownd
        }
    }
}
