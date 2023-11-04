using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorStrikeObject : SpawnObject, IPunObservable
{
    float castingTime = 3.5f;
    float currentCastingTime = 0f;
    float lifeTime = 1f;
    float currentLifeTime = 0f;
    bool isColliderOn = false;

    Collider hitCollider;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            OnEnable();
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CheckTimer();
        }
    }

    void CheckTimer()
    {
        if(currentCastingTime > 0f)
        {
            currentCastingTime -= Time.deltaTime;
        }
        else
        {
            if(isColliderOn == false)
            {
                isColliderOn = true;
                hitCollider.enabled = true;
            }
            if(currentLifeTime > 0f)
            {
                currentLifeTime -= Time.deltaTime;
                if(currentLifeTime <= 0f)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Init();
    }

    void Init()
    {
        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;
        hitCollider = GetComponent<Collider>();
        hitCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<Character>() != null)
        {
            other.gameObject.GetComponent<Character>().hp -= 5;
            Debug.Log(other.gameObject.GetComponent<Character>().hp);
        }
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting)
        {

        }
        else
        {

        }
    }
}
