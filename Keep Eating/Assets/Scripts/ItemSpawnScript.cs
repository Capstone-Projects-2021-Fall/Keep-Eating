using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.tuf31404.KeepEating
{
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

        public Items ItemType { get; set; }

        private void Awake()
        {
            mySpriteRenderer.enabled = false;
            myCollider.enabled = false;
        }

        private void Start()
        {
            if (this.gameObject.name.Contains("Taser"))
            {
                mySpriteRenderer.enabled = true;
                myCollider.enabled = true;
            }
        }
        public void Spawn(int spriteId, Items item)
        {
            this.ItemType = item;

            mySpriteRenderer.enabled = true;
            myCollider.enabled = true;

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

            //Debug.Log("Spawning " + this.gameObject.name);
            //Debug.Log("SpriteRenderer enabled = " + mySpriteRenderer.enabled);
        }

        public void Despawn()
        {
            mySpriteRenderer.enabled = false;
            myCollider.enabled = false;
        }
    }
}
