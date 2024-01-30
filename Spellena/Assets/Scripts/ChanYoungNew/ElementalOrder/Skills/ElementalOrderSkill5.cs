using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalOrderSkill5 : InstantiateObject, IPunObservable
{
    private List<Transform> mainEffects = new List<Transform>();
    private List<Transform> colliders = new List<Transform>();


    private List<Transform> castingAuras = new List<Transform>();
    private Transform castingAura;
    private float skillCastingTime;
    private float skillLifeTime;
    private float hitTimer;
    private float currentHitTimer;
    private int colliderIndex = 0;
    private int hitCount = 5;

    List<GameObject> hitObjects = new List<GameObject>();

    protected override void Start()
    {
        base.Start();


        skillCastingTime = playerData.skillCastingTime[4];
        skillLifeTime = playerData.skillLifeTime[4];

        hitTimer = skillLifeTime / hitCount;
        currentHitTimer = hitTimer;

        for(int i = 0; i < hitCount; i++)
        {
            colliders.Add(transform.GetChild(1).GetChild(i));
            colliders[i].GetComponent<HitTrigger>().OnHit += HitEvent;
        }

        for(int i = 0; i < hitCount; i++)
        {
            mainEffects.Add(transform.GetChild(0).GetChild(1).GetChild(i));
            ParticleSystem[] _temp = mainEffects[i].GetComponentsInChildren<ParticleSystem>();
            for(int j = 0; j < _temp.Length; j++)
                _temp[j].startLifetime = hitTimer * 10f;
        }

        castingAura = transform.GetChild(0).GetChild(0);

        for(int i = 0; i < hitCount; i++)
        {
            castingAuras.Add(castingAura.GetChild(i));
            castingAuras[i].GetComponent<ParticleSystem>().startLifetime = skillCastingTime;
        }

    }

    private void HitEvent(GameObject hitBody)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GameObject _rootObject = hitBody.transform.root.gameObject;
            if (_rootObject.tag == tag)
                return;
            for (int i = 0; i < hitObjects.Count; i++)
            {
                if (hitObjects[i] == _rootObject)
                    return;
            }

            /*
              µ¥¹ÌÁö!
             */
            hitObjects.Add(_rootObject);
        }
    }

    private void FixedUpdate()
    {
        if(skillCastingTime > 0f)
        {
            skillCastingTime -= Time.fixedDeltaTime;

            if(skillCastingTime <= 0f)
            {
                mainEffects[colliderIndex].gameObject.SetActive(true);
                colliders[colliderIndex].gameObject.SetActive(true);
                colliderIndex++;
            }
        }
        else
        {
            skillLifeTime -= Time.fixedDeltaTime;
            currentHitTimer -= Time.fixedDeltaTime;
            if(currentHitTimer <= 0f)
            {
                currentHitTimer = hitTimer;
                if (colliderIndex < hitCount)
                {
                    colliders[colliderIndex - 1].gameObject.SetActive(false);

                    mainEffects[colliderIndex].gameObject.SetActive(true);
                    colliders[colliderIndex].gameObject.SetActive(true);
                    colliderIndex++;
                    hitObjects.Clear();
                }
            }

            if(skillLifeTime <= -hitTimer * 9f && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }
}
