using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public event Action<GameObject> hitTriggerEvent;
    public event Action<GameObject, GameObject> hitTriggerEventWithMe;

    private void OnTriggerStay(Collider other)
    {
        if (hitTriggerEvent != null)
            hitTriggerEvent(other.gameObject);
        else if (hitTriggerEventWithMe != null)
            hitTriggerEventWithMe(other.gameObject, gameObject);
    }
}