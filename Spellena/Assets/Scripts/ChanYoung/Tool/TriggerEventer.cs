using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEventer : MonoBehaviour
{
    public event Action<GameObject> hitTriggerEvent;

    private void OnTriggerStay(Collider other)
    {
        if(hitTriggerEvent != null)
            hitTriggerEvent(other.gameObject);
    }
}
