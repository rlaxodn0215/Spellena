using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EterialStormObject : SpawnObject,IPunObservable
{
    public ElementalOrderData elementalOrderData;

    float lifeTime;
    float currentLifeTime = 0f;

    List<string> hitObjects = new List<string>();
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
        Init();
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
                RequestRPC("ActiveCollider");
            else
            {
                currentLifeTime -= Time.deltaTime;

                if(currentLifeTime < 0f)
                    RequestRPC("RequestDestroy");
            }
        }
        RequestRPC("UpdateData");
    }

    void Init()
    {
        castingTime = elementalOrderData.eterialStormCastingTime;
        lifeTime = elementalOrderData.eterialStormLifeTime;
        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;
        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;
    }

    void RequestRPC(string tunnelCommand)
    {
        object[] _tempData;
        if (tunnelCommand == "UpdateData")
        {
            _tempData = new object[5];
            _tempData[0] = tunnelCommand;
            _tempData[1] = currentCastingTime;
            _tempData[2] = currentLifeTime;
            _tempData[3] = hitObjects.ToArray();
            _tempData[4] = hitTimer.ToArray();
        }
        else
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
        }

        photonView.RPC("CallRPCTunnelElementalOrderSpell6", RpcTarget.AllBuffered, _tempData);
    }

    [PunRPC]
    public void CallRPCTunnelElementalOrderSpell6(object[] data)
    {
        if ((string)data[0] == "UpdateData")
            UpdateData(data);
        else if ((string)data[0] == "ActiveCollider")
            ActiveCollider();
        else if ((string)data[0] == "RequestDestroy")
            RequestDestroy();
    }

    void UpdateData(object[] data)
    {
        currentCastingTime = (float)data[1];
        currentLifeTime = (float)data[2];
        hitObjects = ((string[])data[3]).ToList();
        hitTimer = ((float[])data[4]).ToList();
    }

    void RequestDestroy()
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    void ActiveCollider()
    {
        isColliderOn = true;
        for (int i = 0; i < hitEffect.transform.childCount; i++)
        {
            hitEffect.transform.GetChild(i).GetComponent<ParticleSystem>().Play(true);
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
            if (hitObjects[i] == other.gameObject.name)
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
            hitObjects.Add(other.gameObject.name);
            hitTimer.Add(hitCoolDownTime);
            other.GetComponent<Rigidbody>().AddForce(_outsideVector * impulsePower, ForceMode.Impulse);
            if (_distance <= 3.0f)
            {
                other.GetComponent<Character>().hp -= 5;
            }
        }
    }

}
