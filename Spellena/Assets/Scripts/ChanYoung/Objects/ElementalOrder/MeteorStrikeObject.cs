using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeteorStrikeObject : SpawnObject
{
    public ElementalOrderData elementalOrderData;

    float castingTime;
    float currentCastingTime = 0f;
    float lifeTime;
    float currentLifeTime = 0f;
    float delay = 0.4f;

    public GameObject hitCollider;
    public GameObject hitEffect;
    public GameObject RangeArea;

    List<string> hitObjects = new List<string>();

    bool isColliderOn = false;

    AudioSource[] audioSources;
    void Start()
    {
        Init();
    }

    void FixedUpdate()
    {
        CheckTimer();
    }

    void CheckTimer()
    {
        if(currentCastingTime > 0f)
            currentCastingTime -= Time.deltaTime;
        else
        {
            if (isColliderOn == false)
            {
                delay -= Time.deltaTime;
                if (delay <= 0f)
                    ActiveCollider();
            }
            else
            {
                if (currentLifeTime > 0f)
                {
                    currentLifeTime -= Time.deltaTime;
                    if (currentLifeTime <= 0f)
                    {
                        if (photonView.IsMine)
                            PhotonNetwork.Destroy(gameObject);
                    }
                }
            }
        }
    }

    void Init()
    {
        audioSources = GetComponents<AudioSource>();

        for(int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
        }

        castingTime = elementalOrderData.meteorStrikeCastingTime;
        lifeTime = elementalOrderData.meteorStrikeLifeTime * 3;
        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;

        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;
        BalanceAnimation();
    }

    void BalanceAnimation()
    {
        float _tempLifeTime = elementalOrderData.meteorStrikeLifeTime;
        RangeArea.GetComponent<ParticleSystem>().startLifetime = castingTime * 0.85f;
        hitEffect.transform.GetChild(0).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime * 2;
        hitEffect.transform.GetChild(0).GetComponent<ParticleSystem>().startDelay = _tempLifeTime;
        hitEffect.transform.GetChild(1).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime * 1.5f;
        hitEffect.transform.GetChild(1).GetComponent<ParticleSystem>().startDelay = _tempLifeTime;
        hitEffect.transform.GetChild(2).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime;
        hitEffect.transform.GetChild(3).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime;
        hitEffect.transform.GetChild(4).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime;
        hitEffect.transform.GetChild(5).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime;
        hitEffect.transform.GetChild(5).GetComponent<ParticleSystem>().startDelay = _tempLifeTime;
        hitEffect.transform.GetChild(6).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime * 0.5f;
        hitEffect.transform.GetChild(6).GetComponent<ParticleSystem>().startDelay = _tempLifeTime;
        hitEffect.transform.GetChild(7).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime * 2;
        hitEffect.transform.GetChild(7).GetComponent<ParticleSystem>().startDelay = _tempLifeTime;
        hitEffect.transform.GetChild(7).GetChild(0).GetComponent<ParticleSystem>().startLifetime = _tempLifeTime * 1.25f;
        hitEffect.transform.GetChild(7).GetChild(0).GetComponent<ParticleSystem>().startDelay = _tempLifeTime * 0.6f;
    }

    void TriggerEvent(GameObject hitObject)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (isColliderOn)
            {
                if(hitObject.transform.root.gameObject.name != hitObject.name)
                {
                    GameObject _rootObject = hitObject.transform.root.gameObject;
                    if(_rootObject.tag != tag)
                    {
                        for(int i = 0; i < hitObjects.Count; i++)
                        {
                            if (_rootObject.name == hitObjects[i])
                                return;
                        }
                        hitObjects.Add(_rootObject.name);
                        _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All,
                         playerName, (int)(elementalOrderData.meteorStrikeDamage), hitObject.name, transform.forward, 20f);
                    }
                }
            }
        }
    }
    void ActiveCollider()
    {
        isColliderOn = true;
        hitEffect.SetActive(true);

        for(int i = 0; i < audioSources.Length; i++)
        {

            if(audioSources[i].clip.name == "EO-METEORA")
            {
                audioSources[i].volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
                audioSources[i].PlayDelayed(0.7f);
            }
        }
    }
    
    void RequestDestroy()
    {
        if(photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
    
    void UpdateData(object[] data)
    {
        currentCastingTime = (float)data[1];
        currentLifeTime = (float)data[2];
        hitObjects = ((string[])data[3]).ToList();
    }
}
