using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;

public class BetweenSceneBGM : MonoBehaviour
{
    private static BetweenSceneBGM instance;

    public static BetweenSceneBGM Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
        
    }

}
