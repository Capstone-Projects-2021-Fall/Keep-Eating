/*
        THIS SCRIPT IS  P E R F E C T. 
        D O  N O T  T O U C H.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BulletScript : MonoBehaviour
{

    [SerializeField]
    private float speed = 100f;
    private Vector3 mousePos;
    private Vector3 direction = Vector3.zero;
    private bool hasDirection;
    public int BulletID {get; set;}

    void Awake()
    {
        hasDirection = false;
    }

    void Start()
    {
        Destroy(this.gameObject, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (hasDirection)
        {
            float step = speed * Time.deltaTime;
            transform.position += direction * step;
        }
    }
    

    public void SetDirection(Vector3 _direction)
    {
        direction = _direction;
        hasDirection = true;
    }
}
