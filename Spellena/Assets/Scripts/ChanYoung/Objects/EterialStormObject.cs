using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EterialStormObject : SpawnObject,IPunObservable
{
    float lifeTime = 3.2f;
    float currentLifeTime = 0f;

    List<GameObject> hitObjects = new List<GameObject>();
    List<float> hitTimer = new List<float>();

    float generateTimer = 2.5f;
    float currentGenerateTimer = 0f;

    bool isColliderOn = false;
    Collider hitCollider;

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
        if(currentGenerateTimer > 0f)
        {
            currentGenerateTimer -= Time.deltaTime;
        }
        else
        {
            if(isColliderOn == false)
            {
                isColliderOn = true;
                hitCollider.enabled = true;
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
        currentGenerateTimer = generateTimer;
        currentLifeTime = lifeTime;
        hitCollider = GetComponent<Collider>();
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

    private void OnTriggerStay(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (other.gameObject.GetComponent<Character>() != null)
            {
                CheckHitCount(other);
                Debug.Log(other.GetComponent<Character>().hp);
            }
        }
    }

    void CheckHitCount(Collider other)
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
