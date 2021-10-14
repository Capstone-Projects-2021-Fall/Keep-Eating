using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiatePlayer : MonoBehaviour
{

    public GameObject eaterPrefab, enforcerPrefab;

    public GameObject NewPlayer(bool isEnforcer, Vector3 position)
    {
        if (isEnforcer)
        {
            return Instantiate(enforcerPrefab, position, Quaternion.identity);
        }
        else
        {
            return Instantiate(eaterPrefab, position, Quaternion.identity);
        }
    }
}
