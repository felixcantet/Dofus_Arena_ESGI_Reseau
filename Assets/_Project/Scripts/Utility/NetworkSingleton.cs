using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public abstract class NetworkSingleton<T> : MonoBehaviourPun where T : MonoBehaviourPun
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    Debug.LogError("There is no object of type " + typeof(T).ToString() + " in the scene");
                    return null;
                }
            }
            return instance;
        }
    }
}
