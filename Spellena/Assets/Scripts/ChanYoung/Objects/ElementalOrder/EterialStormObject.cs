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

    float hitCoolDownTime;
    float impulsePower = 4f;


    void Start()
    {
        Init();
    }

    void FixedUpdate()
    {
        CheckTimer();
        CheckHitCoolDownTime();
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
            if (isColliderOn == false)
                ActiveCollider();
            else
            {
                currentLifeTime -= Time.deltaTime;

                if (currentLifeTime < 0f)
                {
                    if (PhotonNetwork.IsMasterClient)
                        RequestRPC("RequestDestroy");
                }
            }
        }
    }

    void Init()
    {
        castingTime = elementalOrderData.eterialStormCastingTime;
        lifeTime = elementalOrderData.eterialStormLifeTime;
        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;
        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;

        hitCoolDownTime = lifeTime / 8;

        BalanceAnimation();
    }

    void BalanceAnimation()
    {
        rangeArea.GetComponent<ParticleSystem>().startLifetime = elementalOrderData.eterialStormCastingTime;
        for(int i = 0; i < 14; i++)
        {
            hitEffect.transform.GetChild(i).GetComponent<ParticleSystem>().startLifetime = castingTime;
        }
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

        photonView.RPC("CallRPCTunnelElementalOrderSpell6", RpcTarget.All, _tempData);
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
        else if ((string)data[0] == "PlayForceSound")
            PlayForceSound();
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
                if (hitObject.transform.root.gameObject.name != hitObject.name)
                {
                    if (hitObject.transform.root.GetComponent<Character>() != null)
                    {
                        CheckHitCount(hitObject.transform.gameObject);
                    }
                }
            }
        }
    }

    void CheckHitCount(GameObject other)
    {
        GameObject _rootObject = other.transform.root.gameObject;

        if(tag != _rootObject.tag)
        {
            return;
        }

        float _xPos = other.transform.position.x - transform.position.x;
        float _zPos = other.transform.position.z - transform.position.z;
        float _distance = _xPos * _xPos + _zPos * _zPos;
        Vector3 _outsideVector = new Vector3(_xPos, 0, _zPos).normalized;

        int _check = 0;
        int _index = -1;

        for(int i = 0; i < hitObjects.Count; i++)
        {
            if (hitObjects[i] == _rootObject.name)
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
                _rootObject.GetComponent<PhotonView>().RPC("PlayerKnockBack", RpcTarget.All, _outsideVector, impulsePower);
                //_rootObject.GetComponent<Rigidbody>().AddForce(_outsideVector * impulsePower, ForceMode.Impulse);
                if(_distance <= 3.0f)
                {
                    _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All,
                         playerName, (int)(elementalOrderData.eterialStormDamage / 8), other.name, _outsideVector, 20f);
                    RequestRPC("PlayForceSound");
                }
            }
        }
        else
        {
            hitObjects.Add(_rootObject.name);
            hitTimer.Add(hitCoolDownTime);
            _rootObject.GetComponent<PhotonView>().RPC("PlayerKnockBack", RpcTarget.All, _outsideVector, impulsePower);
            //_rootObject.GetComponent<Rigidbody>().AddForce(_outsideVector * impulsePower, ForceMode.Impulse);
            if (_distance <= 3.0f)
            {
                _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All,
                         playerName, (int)(elementalOrderData.eterialStormDamage / 8), other.name, _outsideVector, 20f);
                RequestRPC("PlayForceSound");
            }
        }
    }

    void PlayForceSound()
    {
        hitCollider.GetComponent<AudioSource>().volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
        hitCollider.GetComponent<AudioSource>().PlayOneShot(hitCollider.GetComponent<AudioSource>().clip);
    }

}
