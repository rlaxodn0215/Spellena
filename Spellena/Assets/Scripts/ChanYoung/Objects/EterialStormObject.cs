using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EterialStormObject : SpawnObject,IPunObservable
{
    public ElementalOrderData elementalOrderData;

    float lifeTime;
    float currentLifeTime = 0f;

    List<GameObject> hitObjects = new List<GameObject>();
    List<float> hitTimer = new List<float>();

    float castingTime;
    float currentCastingTime = 0f;

    bool isColliderOn = false;

    public GameObject hitCollider;
    public GameObject hitEffect;
    public GameObject rangeArea;

    float hitCoolDownTime = 0.4f;
    float impulsePower = 4f;


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
            CheckHitCoolDownTime();
        }
    }    

    void CheckHitCoolDownTime()
    {
        for(int i = 0; i < hitObjects.Count; i++)
        {
            if (hitTimer[i] > 0f)
            {
                hitTimer[i] -= Time.deltaTime;
            }
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
                for(int i = 0; i < hitEffect.transform.childCount; i++)
                {
                    hitEffect.transform.GetChild(i).GetComponent<ParticleSystem>().Play(true);
                }
            }
            else
            {
                currentLifeTime -= Time.deltaTime;

                if(currentLifeTime < 0f)
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
        castingTime = elementalOrderData.eterialStormCastingTime;
        lifeTime = elementalOrderData.eterialStormLifeTime;

        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;
        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;
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

    void TriggerEvent(GameObject hitObject)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (isColliderOn)
            {
                if (hitObject.GetComponent<Character>() != null)
                {
                    CheckHitCount(hitObject);
                    Debug.Log(hitObject.GetComponent<Character>().hp);
                }
            }
        }
    }

    void CheckHitCount(GameObject other)
    {
        float _xPos = other.transform.position.x - transform.position.x;
        float _zPos = other.transform.position.z - transform.position.z;
        float _distance = _xPos * _xPos + _zPos * _zPos;
        Vector3 _outsideVector = new Vector3(_xPos, 0, _zPos).normalized;

        int _check = 0;
        int _index = -1;

        for(int i = 0; i < hitObjects.Count; i++)
        {
            if (hitObjects[i] == other.gameObject)
            {
                _check = 1;
                _index = i;
                break;
            }
        }

        if(_check == 1)
        {
            if(hitTimer[_index] <= 0f)
            {
                hitTimer[_index] = hitCoolDownTime;
                other.GetComponent<Rigidbody>().AddForce(_outsideVector * impulsePower, ForceMode.Impulse);
                if(_distance <= 3.0f)
                {
                    other.GetComponent<Character>().hp -= 5;
                }
            }
        }
        else
        {
            hitObjects.Add(other.gameObject);
            hitTimer.Add(hitCoolDownTime);
            other.GetComponent<Rigidbody>().AddForce(_outsideVector * impulsePower, ForceMode.Impulse);
            if (_distance <= 3.0f)
            {
                other.GetComponent<Character>().hp -= 5;
            }
        }
    }

}
