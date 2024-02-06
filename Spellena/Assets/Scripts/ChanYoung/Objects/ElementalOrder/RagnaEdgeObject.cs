using Photon.Pun;
using Player;
using System.Collections.Generic;
using UnityEngine;

public class RagnaEdgeObject : SpawnObject
{
    public ElementalOrderData elementalOrderData;
    public GameObject rangeArea;
    public Transform explodes;
    public GameObject hitCollider;

    

    float damage;
    float castingTime;
    int hitCount = 5;
    int currentHitCount;
    float hitTimer;
    float currentHitTimer;
    float lifeTime;
    bool isColliderOn = false;

    List<GameObject> hitObjects = new List<GameObject>();
    AudioSource[] audioSources;

    void Start()
    {
        Init();
    }

    void FixedUpdate()
    {
        CheckTimer();
    }

    void Init()
    {
        audioSources = GetComponents<AudioSource>();

        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
        }

        damage = elementalOrderData.ragnaEdgeFloorDamage;
        castingTime = elementalOrderData.ragnaEdgeCastingTime;
        hitTimer = elementalOrderData.ragnaEdgeFloorLifeTime / 5f;
        lifeTime = elementalOrderData.ragnaEdgeFloorLifeTime;
        currentHitTimer = hitTimer;

        BalanceAnimation();

        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;
    }


    void CheckTimer()
    {
        if (castingTime > 0f)
        {
            castingTime -= Time.fixedDeltaTime;
            if (castingTime <= 0f)
            {
                PlayExplode();
                isColliderOn = true;
            }
        }
        else
        {
            lifeTime -= Time.fixedDeltaTime;
            currentHitTimer -= Time.fixedDeltaTime;
            if(currentHitTimer <= 0f)
                PlayExplode();

            if (lifeTime <= 0f && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }

    private void PlayExplode()
    {
        if (currentHitCount <= 4)
        {
            explodes.GetChild(currentHitCount).gameObject.SetActive(true);
            currentHitCount++;
            hitObjects.Clear();
            currentHitTimer = hitTimer;
        }
    }

    private void TriggerEvent(GameObject hitObject)
    {
        if (PhotonNetwork.IsMasterClient && isColliderOn)
        {
            GameObject _rootObject = hitObject.transform.root.gameObject;
            if(_rootObject.layer == 15 && _rootObject.tag == tag)
            {
                for(int i = 0; i < hitObjects.Count; i++)
                {
                    if (hitObjects[i] == _rootObject)
                        return;
                }

                Vector3 _direction = (_rootObject.transform.position - transform.position).normalized;
                hitObjects.Add(_rootObject);
                _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All,
                playerName, (int)(elementalOrderData.ragnaEdgeFloorDamage), hitObject.name, _direction, 20f);
            }
        }
    }

    void BalanceAnimation()
    {
        rangeArea.GetComponent<ParticleSystem>().startLifetime = castingTime * 0.85f;

        for (int i = 0; i < 5; i++)
        {
            ParticleSystem[] _temp = explodes.transform.GetChild(i).GetComponentsInChildren<ParticleSystem>();

            for (int j = 0; j < _temp.Length; j++)
            {
                _temp[j].startLifetime = hitTimer;
            }

            explodes.transform.GetChild(i).GetComponent<AudioSource>().volume =
                SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
        }
    }


}
