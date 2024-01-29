using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalOrderSkill2 : InstantiateObject, IPunObservable
{
    private Transform castingAura;
    private ParticleSystem castingAuraEffect;
    private List<Transform> mainEffects = new List<Transform>();
    private HitTrigger hitTrigger;

    private int hitCount = 5;
    private float skillCastingTime;
    private float skillLifeTime;
    private float hitTimer;
    private float currentHitTimer;
    private bool isColliderOn;
    private int explodeIndex = 0;

    private List<GameObject> hitObjects = new List<GameObject>();

    protected override void Start()
    {
        base.Start();
        hitTrigger = transform.GetChild(1).GetComponent<HitTrigger>();
        skillCastingTime = playerData.skillCastingTime[1];
        skillLifeTime = playerData.skillLifeTime[1];

        castingAura = transform.GetChild(0).GetChild(0);

        hitTimer = skillLifeTime / hitCount;
        currentHitTimer = hitTimer;

        hitTrigger.OnHit += HitEvent;

        castingAuraEffect = castingAura.GetComponent<ParticleSystem>();
        castingAuraEffect.startLifetime = skillCastingTime;
        castingAuraEffect.Play();
        for (int i = 0; i < hitCount; i++)
        {
            mainEffects.Add(transform.GetChild(0).GetChild(1).GetChild(i));
        }
    }

    private void HitEvent(GameObject hitBody)
    {
        if(isColliderOn && PhotonNetwork.IsMasterClient)
        {
            GameObject _rootObject = hitBody.transform.root.gameObject;
            if (_rootObject.tag == tag)
                return;
            for(int i = 0; i < hitObjects.Count; i++)
            {
                if (hitObjects[i] == _rootObject)
                    return;
            }

            hitObjects.Add(_rootObject);
            //µ¥¹ÌÁö!
        }
    }

    private void FixedUpdate()
    {
        if(skillCastingTime > 0f)
        {
            skillCastingTime -= Time.fixedDeltaTime;
            if (skillCastingTime <= 0f)
            {
                isColliderOn = true;
                mainEffects[explodeIndex].gameObject.SetActive(true);
                explodeIndex++;
            }
        }
        else
        {
            skillLifeTime -= Time.fixedDeltaTime;
            currentHitTimer -= Time.fixedDeltaTime;
            if (currentHitTimer <= 0f)
            {
                currentHitTimer = hitTimer;
                if (mainEffects[explodeIndex] != null)
                {
                    hitObjects.Clear();
                    mainEffects[explodeIndex].gameObject.SetActive(true);
                    explodeIndex++;
                }
            }

            if (skillLifeTime <= 0f && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }
}

