using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLoop : MonoBehaviour
{
    private AudioSource audioSource;
    void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(LoopSound());
    }

    IEnumerator LoopSound()
    {
        for(int i = 0; i < 45; i++)
        {
            audioSource.Play();
            yield return new WaitForSeconds(0.075f);
        }
    }

}
