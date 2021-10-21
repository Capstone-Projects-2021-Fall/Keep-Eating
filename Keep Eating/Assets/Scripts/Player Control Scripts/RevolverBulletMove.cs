using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevolverBulletMove : MonoBehaviour
{

    private float speed = 50f;
    private Vector3 mousePos;
    private Vector3 direction = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // z is set to 0 so the camera can see it
        mousePos.z = 0;
        direction = (mousePos - transform.position).normalized;
        Destroy(gameObject, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position += direction * step;
    }

}

