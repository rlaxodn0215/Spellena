using Photon.Pun;
using Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistPhlegmHorror : SpawnObject
{
    int creatorViewNum;
    bool isCheck = false;

    int audioIndex = 0;

    AudioSource[] audioSource;

    private void Start()
    {
        creatorViewNum = (int)data[3];
        audioSource = GetComponents<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (audioSource[audioIndex].isPlaying == false)
        {
            audioIndex = 1 - audioIndex;
            audioSource[audioIndex].volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
            audioSource[audioIndex].Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient && isCheck == false)
        {
            if (other.transform.root.GetComponent<Player.Character>() != null)
            {
                GameObject _rootObject = other.transform.root.gameObject;
                if(_rootObject.tag != tag)
                {
                    PhotonView[] photonViews = PhotonNetwork.PhotonViews;
                    for (int i = 0; i < photonViews.Length; i++)
                    {
                        if (creatorViewNum == photonViews[i].ViewID)
                        {
                            //이벤트 발생
                            photonViews[i].RPC("TeleportToPoint", RpcTarget.All, _rootObject.transform.position, _rootObject.transform.forward);
                            photonViews[i].RPC("CancelSkill2", RpcTarget.MasterClient, _rootObject.transform.position + _rootObject.transform.forward * 2);


                            _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All,
                               playerName, 150, other.name, transform.forward, 10f);

                            isCheck = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}
