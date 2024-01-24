using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitTrigger : MonoBehaviour
{
    public event Action<GameObject> OnHit;

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.root.gameObject.layer == 15)//플레이어면
        {
            OnHit?.Invoke(other.gameObject);
        }
    }
}
