using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorStrikeObject : SpawnObject, IPunObservable
{
    public ElementalOrderData elementalOrderData;

    float castingTime;
    float currentCastingTime = 0f;
    float lifeTime;
    float currentLifeTime = 0f;

    public GameObject hitCollider;
    public GameObject hitEffect;

    bool isColliderOn = false;
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
                hitEffect.SetActive(true);
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
        castingTime = elementalOrderData.meteorStrikeCastingTime;
        lifeTime = elementalOrderData.meteorStrikeLifeTime;
        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;
        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += triggerEvent;
    }

    void triggerEvent(GameObject gameObject)
    {
        if (isColliderOn)
        {
            Debug.Log(gameObject);
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
