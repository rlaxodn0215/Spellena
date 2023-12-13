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

    float judgementTime = 5f;
    float currentJudgementTime = 0f;

    GameObject targetObject;
    public NavMeshAgent agent;
    public GameObject judgement;
    public GameObject chaser;

    bool isChasing = true;

    private void Start()
    {
        targetViewID = (int)data[3];
        playerName = (string)data[4];
        targetObject = PhotonNetwork.GetPhotonView(targetViewID).gameObject;
        currentLifeTime = lifeTime;
        currentJudgementTime = judgementTime;
    }

    private void FixedUpdate()
    {
        if (isChasing)
        {
            agent.destination = targetObject.transform.position;
            currentLifeTime -= Time.deltaTime;
        }
        else
        {
            currentJudgementTime -= Time.deltaTime;
        }

        if(PhotonNetwork.IsMasterClient)
        {
            if (currentLifeTime <= 0f)
                CallRPC("RequestDestroy");
            else if (currentJudgementTime <= 0f)
                CallRPC("RequestDestroy");
        }
    }

    void CallRPC(string command)
    {
        photonView.RPC("CallRPCTunnelCultistChaser", RpcTarget.All, command);
    }

    [PunRPC]
    public void CallRPCTunnelCultistChaser(string command)
    {
        if (command == "RequestDestroy")
            RequestDestroy();
        else if (command == "Judgement")
            PlayJudgement();
    }

    void PlayJudgement()
    {
        judgement.SetActive(true);
        isChasing = false;
        agent.enabled = false;
        chaser.SetActive(false);
    }

    void RequestDestroy()
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient && isChasing)
        {
            if (other.gameObject == targetObject)
            {
                targetObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All, playerName, 100000, "",
                                   Vector3.zero, 0f);
                CallRPC("Judgement");
            }
        }
    }
}
