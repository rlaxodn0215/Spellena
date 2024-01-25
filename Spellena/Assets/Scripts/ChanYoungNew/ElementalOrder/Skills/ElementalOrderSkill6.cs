using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ElementalOrderSkill6 : InstantiateObject, IPunObservable
{
    private Transform castingAura;
    private Transform mainEffect;
    private HitTrigger hitTrigger;
    private ParticleSystem castingAuraEffect;
    private ParticleSystem[] mainEffects;

    private float skillCastingTime;
    private float skillLifeTime;
    private float hitTimer;
    private float currentHitTimer;
    private bool isColliderOn = false;
    private int hitCount = 8;
    private bool isLoop = true;
    private float power = 12f;


    List<GameObject> hitObjects = new List<GameObject>();

    protected override void Start()
    {
        base.Start();

        hitTrigger = transform.GetChild(1).GetComponent<HitTrigger>();

        hitTrigger.OnHit += HitEvent;

        skillCastingTime = playerData.skillCastingTime[5];
        skillLifeTime = playerData.skillLifeTime[5];
        castingAura = transform.GetChild(0).GetChild(0);
        mainEffect = transform.GetChild(0).GetChild(1);

        castingAuraEffect = castingAura.GetComponent<ParticleSystem>();
        mainEffects = mainEffect.GetComponentsInChildren<ParticleSystem>();

        castingAuraEffect.startLifetime = skillCastingTime;
        castingAuraEffect.Play();

        for (int i = 0; i < mainEffects.Length; i++)
            mainEffects[i].startLifetime = skillLifeTime;

        hitTimer = skillLifeTime / hitCount;
        currentHitTimer = hitTimer;



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

            float _magnitude = (_rootObject.transform.position - transform.position).magnitude;

            Photon.Realtime.Player _player = _rootObject.GetComponent<PhotonView>().Owner;
            Vector3 _direction = (_rootObject.transform.position - transform.position).normalized;
            _direction *= power;
            _rootObject.GetComponent<PhotonView>().RPC("AddPower", _player, _direction);

            if(_magnitude <= 3f)
            {
                //µ¥¹ÌÁö!
            }

            hitObjects.Add(_rootObject);
        }
    }

    private void FixedUpdate()
    {
        if(skillCastingTime > 0)
        {
            skillCastingTime -= Time.fixedDeltaTime;

            if (skillCastingTime <= 0f)
            {
                mainEffect.gameObject.SetActive(true);
                isColliderOn = true;
            }
        }
        else
        {
            skillLifeTime -= Time.fixedDeltaTime;
            currentHitTimer -= Time.fixedDeltaTime;
            if(currentHitTimer <= 0f)
            {
                currentHitTimer = hitTimer;
                hitObjects.Clear();
            }

            if(skillLifeTime <= playerData.skillLifeTime[5] * 0.1f && isLoop)
            {
                isLoop = false;
                for(int i = 0; i < mainEffects.Length; i++)
                    mainEffects[i].loop = false;
            }

            if (skillLifeTime <= 0f && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }
}
