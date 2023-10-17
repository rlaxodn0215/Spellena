using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AeternaSword : MonoBehaviour
{
    [HideInInspector]
    public GameObject contactObject;

    private LayerMask layerMask;
    private void Start()
    {
        if(CompareTag("TeamA"))
        {
            layerMask = 1 << LayerMask.NameToLayer("ProjectileB");
        }

        else if(CompareTag("TeamB"))
        {
            layerMask = 1 << LayerMask.NameToLayer("ProjectileA");
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == layerMask)
        {
            contactObject = other.gameObject;
        }
    }
}
