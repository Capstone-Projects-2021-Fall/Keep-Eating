using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Com.tuf31404.KeepEating
{
    public class BotMap : MonoBehaviour
    {

        public float[,] AdjMatrix { get; }
        private int dimensions;
        static string path = "Assets/Resources/deboog.txt";
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
            //AdjMatrix[_b, _a] = dist;
        }

        public void Dijkstra(int source, int target)
        {
            int[] Q = new int[36];
            float[] dist = new float[36];
            int[] prev = new int[36];
            for (int i = 0; i < 36; i++)
            {
                dist[i] = AdjMatrix[source, i];
                prev[i] = -1;
                Q[i] = i;
            }

            dist[source] = 0;
            Q[source] = -1;

            int u = source;
            int k = 0;
            while (!IsEmpty(Q))
            {
                u = GetMin(Q, dist);
                if (u == target)
                {
                    Debug.Log("Target Reached");
                    break;
                }
                Q[u] = -1;
                for (int i = 0; i < 36; i++)
                {
                    if (AdjMatrix[u, i] != Mathf.Infinity && Q[i] != -1)
                    {
                        float temp = dist[u] + AdjMatrix[u, i];
                        if (temp < dist[i])
                        {
                            dist[i] = temp;
                            prev[i] = u;
                        }
                    }
                }
                if (k++ >= 200000)
                {
                    Debug.Log("oh fuck");
                    break;
                }
            }

            int[] S = new int[36];
            u = target;

            if (prev[u] != -1 || u == source)
            {
                int j = 0;
                while (u != -1)
                {
                    S[j++] = u;
                    u = prev[u];
                }
            }

            StreamWriter writer = new StreamWriter(path, true);
            string print = "dist ";
            foreach (float d in dist)
            {
                print += d + " ";
            }
            writer.WriteLine(print);
            print = "prev ";
            foreach (int i in prev)
            {
                print += i + " ";
            }
            writer.WriteLine(print);
            print = "Q ";
            foreach (int q in Q)
            {
                print += q + " ";
            }
            writer.WriteLine(print);
            print = "S ";
            foreach (int s in S)
            {
                print += s + " ";
            }
            writer.WriteLine(print);
            writer.Close();
        }

        private int GetMin(int[] Q, float[] _dist)
        {
            int min = 0;
            float prev = Mathf.Infinity;
            for (int i = 1; i < 36; i++)
            {
                if (Q[i] != -1 && _dist[i] < prev)
                {
                    prev = _dist[i];
                    min = i;
                }
            }
            return min;
        }
        private bool IsEmpty(int[] arr)
        {
            foreach (int i in arr)
            {
                if (i != -1)
                {
                    return false;
                }
            }
            return true;
        }

        public void PrintMap(GameObject[] nodes)
        {
            
            StreamWriter writer = new StreamWriter(path, true);
            string print = "";

            for (int i = 0; i < dimensions; i++)
            {
                print = nodes[i].name;
                for (int j = 0; j < dimensions; j++)
                {
                    print += " " + AdjMatrix[i,j];
                }
                writer.WriteLine(print);
            }
            writer.WriteLine(" ");
            writer.Close();
        }

    }
}
