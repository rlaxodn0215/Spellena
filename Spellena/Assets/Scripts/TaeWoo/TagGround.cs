using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagGround : MonoBehaviour
{
    void Start()
    {
        Transform[] objs = transform.GetComponentsInChildren<Transform>();

        foreach(Transform obj in objs)
        {
            obj.tag = "Ground";
        }
    }


}
