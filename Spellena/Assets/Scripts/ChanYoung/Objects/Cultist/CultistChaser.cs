using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CultistChaser : SpawnObject
{
    int targetViewID = -1;

    string playerName;

    float currentLifeTime = 0f;
    float lifeTime = 10f;

    GameObject targetObject;
    public NavMeshAgent agent;
    private void Start()
    {
        targetViewID = (int)data[3];
        playerName = (string)data[4];
        targetObject = PhotonNetwork.GetPhotonView(targetViewID).gameObject;
        currentLifeTime = lifeTime;
    }

    private void FixedUpdate()
    {
        agent.destination = targetObject.transform.position;
        currentLifeTime -= Time.deltaTime;

        if(photonView.IsMine)
        {
            if(currentLifeTime <= 0f)
                PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == targetObject)
        {
            targetObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All, playerName, 10, "",
                               Vector3.zero, 0f);
        }
    }
}
