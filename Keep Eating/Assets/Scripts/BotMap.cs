using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Com.tuf31404.KeepEating
{
    public class BotMap : MonoBehaviour
    {

        public float[,] AdjMatrix { get; }
        private int dimensions;
        public BotMap(int _dimensions)
        {
            dimensions = _dimensions;
            AdjMatrix = new float[dimensions, dimensions];

            for (int i = 0; i < dimensions; i++)
            {
                for (int j = 0; j < dimensions; j++)
                {
                    AdjMatrix[i, j] = Mathf.Infinity;
                }
            }
        }

        public void Add(Vector3 a, Vector3 b, int _a, int _b)
        {

            //Debug.Log("Adding to Matrix");
            float dist = Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2));
            AdjMatrix[_a, _b] = dist;
            AdjMatrix[_b, _a] = dist;
        }

        public void PrintMap()
        {
            string print = "";

            for (int i = 0; i < dimensions; i++)
            {
                for (int j = 0; j < dimensions; j++)
                {
                    print += " " + AdjMatrix[i,j];
                }
                Debug.Log(print);
                print = "";
            }
        }

    }
}
