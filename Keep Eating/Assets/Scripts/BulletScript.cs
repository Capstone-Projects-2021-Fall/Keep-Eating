

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
    public string BulletName {get; set;}

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(this.gameObject);
    }
}
