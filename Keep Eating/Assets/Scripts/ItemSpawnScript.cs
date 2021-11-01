using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnScript : MonoBehaviour
{
    [SerializeField]
    private Sprite sprite1;
    [SerializeField]
    private Sprite sprite2;
    [SerializeField]
    private Sprite sprite3;
    [SerializeField]
    private SpriteRenderer mySpriteRenderer;
    [SerializeField]
    private Collider2D myCollider;

    void Start()
    {
        mySpriteRenderer.enabled = false;
        myCollider.enabled = false;
    }

    public void Spawn(int spriteId)
    {
        switch (spriteId)
        {
            case 1:
                mySpriteRenderer.sprite = sprite1;
                break;
            case 2:
                mySpriteRenderer.sprite = sprite2;
                break;
            case 3:
                mySpriteRenderer.sprite = sprite3;
                break;
            default:
                Debug.Log("Sprite Error");
                break;
        }

        mySpriteRenderer.enabled = true;
        myCollider.enabled = true;
    }

    public void Despawn()
    {
        mySpriteRenderer.enabled = false;
        myCollider.enabled = false;
    }
}
