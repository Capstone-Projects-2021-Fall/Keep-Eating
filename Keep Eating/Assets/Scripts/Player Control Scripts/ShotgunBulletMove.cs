using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBulletMove : MonoBehaviour
{
    private float speed = 50f;
    private Vector3 mousePos;
    private Vector3 direction = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos += Random.insideUnitSphere * 5;
        // z is set to 0 so the camera can see it
        mousePos.z = 0;
        direction = (mousePos - transform.position).normalized ;
        Destroy(gameObject, 0.75f);
        direction = Quaternion.Euler(0, -45, 0) * direction;
    }

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position += direction * step;
        
    }
}
