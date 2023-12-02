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

    private void Start()
    {
        creatorViewNum = (int)data[3];
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
                            isCheck = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}
