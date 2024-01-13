using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GlobalOperation : MonoBehaviour
{

    private static GlobalOperation instance;

    public static GlobalOperation Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject globalOperation = new GameObject("GlobalOperation");
                instance = globalOperation.AddComponent<GlobalOperation>();
                DontDestroyOnLoad(globalOperation);
            }
            return instance;
        }
    }

    public float NormalizeAngle(float angle)
    {
        angle %= 360;
        if(angle > 180)
            angle -= 360;

        return angle;
    }
}
