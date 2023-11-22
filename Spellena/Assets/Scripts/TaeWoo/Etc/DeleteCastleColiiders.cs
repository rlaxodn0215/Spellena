using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteCastleColiiders : MonoBehaviour
{
    void Awake()
    {
        Collider[] obj = GetComponentsInChildren<Collider>();

        foreach(Collider temp in obj)
        {
            Destroy(temp);
        }
    }


}
