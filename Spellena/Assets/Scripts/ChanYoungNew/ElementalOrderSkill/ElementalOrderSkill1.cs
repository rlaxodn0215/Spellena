using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalOrderSkill1 : InstantiateObject, IPunObservable
{
    private Transform castingAura;
    private Transform mainEffect;
    private ParticleSystem[] castingAuraEffects;
    private ParticleSystem[] mainEffects;

    float skillCastingTime;
    float skillLifeTime;
    bool isColliderOn = false;

    protected override void Start()
    {
        base.Start();

        skillCastingTime = playerData.skillCastingTime[0];
        skillLifeTime = playerData.skillLifeTime[0];
        castingAura = transform.GetChild(0).GetChild(0);
        mainEffect = transform.GetChild(0).GetChild(1);

        castingAuraEffects = castingAura.GetComponentsInChildren<ParticleSystem>();
        mainEffects = mainEffect.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < castingAuraEffects.Length; i++)
        {
            castingAuraEffects[i].startLifetime = skillCastingTime;
            castingAuraEffects[i].Play();
        }

        for (int i = 0; i < mainEffects.Length; i++)
        {
            mainEffects[i].startLifetime = skillLifeTime;
        }
    }

    private void FixedUpdate()
    {
        if (skillCastingTime > 0f)
        {
            skillCastingTime -= Time.fixedDeltaTime;
            //¶³¾îÁö´Â È¿°ú ÄÑÁü
        }
        else
        {
            if (skillLifeTime > 0f)
            {
                skillLifeTime -= Time.fixedDeltaTime;
            }
        }
    }
}
