using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TerraBreakObject : SpawnObject
{
    public ElementalOrderData elementalOrderData;

    public GameObject rangeArea;
    public GameObject hitEffect;

    float castingTime;
    float lifeTime;
    bool isColliderOn = false;

    public GameObject hitCollider;

    List<GameObject> hitObjects = new List<GameObject>();

    void Start()
    {
        Init();
    }

    void FixedUpdate()
    {
        CheckTimer();
    }

    void CheckTimer()
    {
        if(castingTime > 0f)
        {
            castingTime -= Time.deltaTime;
            if (castingTime <= 0f)
            {
                isColliderOn = true;
                hitEffect.SetActive(true);
            }
        }
        else
        {
            lifeTime -= Time.deltaTime;
            if(lifeTime <= 0f && photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    void Init()
    {
        GetComponent<AudioSource>().volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
        hitEffect.GetComponent<AudioSource>().volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
        castingTime = elementalOrderData.terraBreakCastingTime;
        lifeTime = elementalOrderData.terraBreakLifeTime;
        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;

        BalanceAnimation();
    }

    void BalanceAnimation()
    {
        rangeArea.GetComponent<ParticleSystem>().startLifetime = castingTime;
        for(int i = 0; i < 7; i++)
        {
            hitEffect.transform.GetChild(i).GetComponent<ParticleSystem>().startLifetime = lifeTime;
        }
    }

    void TriggerEvent(GameObject hitObject)
    {
        if(PhotonNetwork.IsMasterClient && isColliderOn)
        {
            GameObject _rootObject = hitObject.transform.root.gameObject;
            if(_rootObject != hitObject && _rootObject.layer == 15 && _rootObject.tag != tag)
            {
                for(int i = 0; i < hitObjects.Count; i++)
                {
                    if (_rootObject == hitObjects[i])
                        return;
                }
                PhotonView _photonView = _rootObject.GetComponent<PhotonView>();
                hitObjects.Add(_rootObject);
                _photonView.RPC("PlayerDamaged", RpcTarget.All,
                    playerName, (int)(elementalOrderData.terraBreakDamage), hitObject.name, Vector3.up, 20f);

                _photonView.RPC("PlayerKnockBack", _photonView.Owner, Vector3.up, 20f);
            }
        }
    }    
}
