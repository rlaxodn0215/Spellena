using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryEffect : MonoBehaviour
{
    public float destoryTime = 1.5f;

    IEnumerator Start()
    {
        //GetComponent<AudioSource>().volume = 1.0f * SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
        //GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(destoryTime);

        Destroy(gameObject);
    }
}
