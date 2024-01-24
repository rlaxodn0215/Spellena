using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class ElementalOrderSkill4 : InstantiateObject, IPunObservable
{
    private Transform castingAura;
    private List<Transform> mainEffects = new List<Transform>();
    private HitTrigger hitTrigger;
    private int chargeCount = 3;

    float skillCastingTime;
    float skillLifeTime;
    float hitTimer;
    float currentHitTimer;
    int hitCount = 0;

    bool isColliderOn = false;

    List<GameObject> hitObjects = new List<GameObject>();

    protected override void Start()
    {
        base.Start();

        hitTrigger = transform.GetChild(1).GetComponent<HitTrigger>();

        hitTrigger.OnHit += HitEvent;

        skillCastingTime = playerData.skillCastingTime[3];
        skillLifeTime = playerData.skillLifeTime[3];

        hitTimer = skillLifeTime / (chargeCount + 1);
        currentHitTimer = hitTimer;

        castingAura = transform.GetChild(0).GetChild(0);
        castingAura.GetComponent<ParticleSystem>().startLifetime = skillCastingTime;

        for(int i = 0; i < chargeCount + 1; i++)
        {
            mainEffects.Add(transform.GetChild(0).GetChild(1).GetChild(i));


            ParticleSystem[] _temp = mainEffects[i].GetComponentsInChildren<ParticleSystem>();
            for (int j = 0; j < _temp.Length; j++)
            {
                _temp[j].startLifetime = hitTimer;
            }
        }

    }


    private void HitEvent(GameObject hitBody)
    {
        if (isColliderOn && PhotonNetwork.IsMasterClient)
        {
            GameObject _rootObject = hitBody.transform.root.gameObject;
            /*
            if (_rootObject.tag == tag)
                return;
            */
            for (int i = 0; i < hitObjects.Count; i++)
            {
                if (hitObjects[i] == _rootObject)
                    return;
            }

            /*
              플레이어 데미지
             */
            Photon.Realtime.Player _player = _rootObject.GetComponent<PhotonView>().Owner;

            Vector3 _direction;

            if (hitCount <= chargeCount)
            {
                _direction = (transform.position - _rootObject.transform.position) * 7f;
                _rootObject.GetComponent<PhotonView>().RPC("AddPower", _player, _direction);
            }
            else
            {
                _rootObject.GetComponent<PhotonView>().RPC("AddYPower", _player, 10f);
            }
            hitObjects.Add(_rootObject);
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
                mainEffects[hitCount].gameObject.SetActive(true);
                hitCount++;
            }
        }
        else
        {
            skillLifeTime -= Time.fixedDeltaTime;
            currentHitTimer -= Time.fixedDeltaTime;
            if(currentHitTimer <= 0f)
            {
                if(hitCount <= chargeCount)
                {
                    currentHitTimer = hitTimer;
                    mainEffects[hitCount].gameObject.SetActive(true);
                    hitCount++;
                    hitObjects.Clear();
                }
            }

            if(skillLifeTime <= 0f && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }

}
