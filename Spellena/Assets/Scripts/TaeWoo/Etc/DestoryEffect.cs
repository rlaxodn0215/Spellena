using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryEffect : MonoBehaviour
{
    public float destoryTime = 1.5f;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(destoryTime);

        Destroy(gameObject);
    }
}
