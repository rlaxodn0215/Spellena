using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidSound : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Player.Character>() && other.CompareTag("TeamA"))
        {
            audioSource.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player.Character>() && other.CompareTag("TeamA"))
        {
            audioSource.Stop();
        }
    }

}
