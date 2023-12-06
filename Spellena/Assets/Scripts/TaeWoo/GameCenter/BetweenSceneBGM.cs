using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;

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

    //private void OnApplicationFocus(bool focus)
    //{
    //    if(!focus)
    //    {
    //        FirebaseLoginManager.Instance.SignOut();
    //    }

    //}

    //private void OnApplicationQuit()
    //{
    //    // ·Î±× ¾Æ¿ô
    //    FirebaseLoginManager.Instance.SignOut();
    //}

}
