using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CultistSkill4 : InstantiateObject
{
    NavMeshAgent agent;
    GameObject _target;

    private float lifeTime;

    protected override void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lifeTime = playerData.skillLifeTime[3];
        PhotonView _targetView = PhotonNetwork.GetPhotonView((int)data[2]);

        _target = _targetView.gameObject;
        if(photonView.IsMine)
        {
            agent.enabled = true;
            agent.SetDestination(_target.transform.position);
        }

    }

    private void FixedUpdate()
    {
        if(photonView.IsMine)
            agent.SetDestination(_target.transform.position);

        lifeTime -= Time.deltaTime;

        if (photonView.IsMine)
        {
            agent.SetDestination(_target.transform.position);
            if(lifeTime <= 0f)
                PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject _rootObject = other.transform.root.gameObject;

            if (_rootObject.layer == 15)
            {
                if (_rootObject.tag != tag)
                {
                    //µ¥¹ÌÁö -> 
                    photonView.RPC("DestroyObject", photonView.Owner);
                }
            }
        }
    }


}
