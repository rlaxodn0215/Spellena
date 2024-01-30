using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalOrderSkill3 : InstantiateObject, IPunObservable
{
    private Transform castingAura;
    private Transform mainEffect;
    private HitTrigger hitTrigger;

    List<GameObject> hitObjects = new List<GameObject>();

    private float skillCastingTime;
    private float skillLifeTime;
    private float hitTimer;
    private float currentHitTimer;

    private bool isColliderOn = false;
    private int hitCount = 10;
    private int hitIndex = 0;
    private bool isOnce = true;

    protected override void Start()
    {
        base.Start();
        hitTrigger = transform.GetChild(1).GetComponent<HitTrigger>();
        hitTrigger.OnHit += HitEvent;

        skillCastingTime = playerData.skillCastingTime[2];
        skillLifeTime = playerData.skillLifeTime[2];
        mainEffect = transform.GetChild(0).GetChild(1);
        castingAura = transform.GetChild(0).GetChild(0);
        for (int i = 0; i < 9; i++)
        {
            castingAura.GetChild(i).GetComponent<ParticleSystem>().startLifetime = skillCastingTime;
        }

        hitTimer = skillLifeTime / hitCount;
        currentHitTimer = hitTimer;
        
    }


    private void HitEvent(GameObject hitBody)
    {
        if (isColliderOn && PhotonNetwork.IsMasterClient)
        {
            GameObject _rootObject = hitBody.transform.root.gameObject;

            if (_rootObject.tag == tag)
                return;

            for (int i = 0; i < hitObjects.Count; i++)
            {
                if (hitObjects[i] == _rootObject)
                    return;
            }

            //µ¥¹ÌÁö

            hitObjects.Add(_rootObject);
        }
    }

    private void FixedUpdate()
    {
        if (skillCastingTime > 0)
        {
            skillCastingTime -= Time.fixedDeltaTime;

            if (skillCastingTime <= 0f)
            {
                mainEffect.gameObject.SetActive(true);
                isColliderOn = true;
                hitIndex++;
            }
        }
        else
        {
            skillLifeTime -= Time.fixedDeltaTime;
            currentHitTimer -= Time.fixedDeltaTime;
            if (currentHitTimer <= 0f)
            {
                if (hitIndex < hitCount)
                {
                    currentHitTimer = hitTimer;
                    hitObjects.Clear();
                    hitIndex++;
                }
            }

            if (skillLifeTime <= playerData.skillLifeTime[2] * 0.1f && isOnce)
            {
                isOnce = false;

                for(int i = 0; i < 9; i++)
                {
                    ParticleSystem[] _temp = mainEffect.GetChild(i).GetComponentsInChildren<ParticleSystem>();
                    for(int j = 0; j < _temp.Length; j++)
                    {
                        _temp[j].loop = false;
                    }
                }
            }

            if(skillLifeTime <= 0f && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }
}
