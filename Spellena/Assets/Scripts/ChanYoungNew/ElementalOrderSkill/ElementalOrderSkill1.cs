using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalOrderSkill1 : InstantiateObject, IPunObservable
{
    private Transform castingAura;
    private Transform mainEffect;
    private HitTrigger hitTrigger;
    private ParticleSystem castingAuraEffect;
    private ParticleSystem[] mainEffects;

    float skillCastingTime;
    float skillLifeTime;
    bool isColliderOn = false;
    bool isOnce = false;

    List<GameObject> hitObjects = new List<GameObject>();

    protected override void Start()
    {
        base.Start();

        hitTrigger = transform.GetChild(1).GetComponent<HitTrigger>();

        hitTrigger.OnHit += HitEvent;

        skillCastingTime = playerData.skillCastingTime[0];
        skillLifeTime = playerData.skillLifeTime[0];
        castingAura = transform.GetChild(0).GetChild(0);
        mainEffect = transform.GetChild(0).GetChild(1);

        castingAuraEffect = castingAura.GetComponent<ParticleSystem>();
        mainEffects = mainEffect.GetComponentsInChildren<ParticleSystem>();

        castingAuraEffect.startLifetime = skillCastingTime;
        castingAuraEffect.Play();

        for (int i = 0; i < mainEffects.Length; i++)
            mainEffects[i].startLifetime = skillLifeTime;//딜레이 고려
    }

    private void HitEvent(GameObject hitBody)
    {
        if (isColliderOn && PhotonNetwork.IsMasterClient)
        {
            GameObject _rootObject = hitBody.transform.root.gameObject;
            if (_rootObject.tag == tag)
                return;
            for(int i = 0; i < hitObjects.Count; i++)
            {
                if (hitObjects[i] == _rootObject)
                    return;
            }

            /*
              플레이어 데미지
             */
            hitObjects.Add(_rootObject);
        }
    }

    private void FixedUpdate()
    {
        if (skillCastingTime > 0f)
        {
            skillCastingTime -= Time.fixedDeltaTime;
            //떨어지는 효과 켜짐
            if (skillCastingTime <= 0f)
                mainEffect.gameObject.SetActive(true);
        }
        else
        {
            skillLifeTime -= Time.fixedDeltaTime;
            if (skillLifeTime <= playerData.skillLifeTime[0] * 0.7f && !isColliderOn && !isOnce)
            {
                isColliderOn = true;
                isOnce = true;
            }
            else if (skillLifeTime <= playerData.skillLifeTime[0] * 0.5f && isColliderOn)
                isColliderOn = false;

            if (skillLifeTime <= -1f && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);

        }
    }
}
