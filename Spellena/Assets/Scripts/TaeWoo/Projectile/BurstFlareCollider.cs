using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstFlareCollider : MonoBehaviour
{

    public float speed = 15.5f;

    void Update()
    {
        transform.localPosition += Vector3.forward * speed * Time.deltaTime;
    }
}
