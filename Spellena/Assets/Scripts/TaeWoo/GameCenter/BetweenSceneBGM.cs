using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetweenSceneBGM : MonoBehaviour
{
    public static BetweenSceneBGM instance;

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
