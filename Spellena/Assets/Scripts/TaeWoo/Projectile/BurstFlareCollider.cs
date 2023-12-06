using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstFlareCollider : MonoBehaviour
{
    public BurstFlareObject burstFlareObject;
    public float speed = 15.5f;

    void FixedUpdate()
    {
        if(!burstFlareObject.isHit)
        transform.localPosition += Vector3.forward * speed * Time.deltaTime;
    }
}
