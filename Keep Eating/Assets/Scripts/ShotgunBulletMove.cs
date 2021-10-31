/*
        THIS SCRIPT IS  P E R F E C T. 
        D O  N O T  T O U C H.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBulletMove : MonoBehaviour
{
    [SerializeField]
    private float speed = 100f;
    private Vector3 mousePos;
    private Vector3 direction = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);         //Gets the position of the mouse 
        mousePos += Random.insideUnitSphere * 5;                                //This is where the MAGIC happens.
        mousePos.z = 0;                                                         // z is set to 0 so the camera can see it
        direction = (mousePos - transform.position).normalized ;
        Destroy(gameObject, 0.5f);                                              //Destroys after 1/2 second so it doesnt go far like a real shotgun!!!
        direction = Quaternion.Euler(0, -45, 0) * direction;
    }

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position += direction * step;
    }
}
