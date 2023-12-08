using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistHealEffect : MonoBehaviourPunCallbacks
{
    float effectTime = 2f;

    private void Start()
    {
        GetComponent<AudioSource>().volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
        if (!photonView.IsMine)
            GetComponent<AudioSource>().mute = true;
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            effectTime -= Time.deltaTime;
            if (effectTime <= 0f)
                PhotonNetwork.Destroy(gameObject);
        }
    }
}
