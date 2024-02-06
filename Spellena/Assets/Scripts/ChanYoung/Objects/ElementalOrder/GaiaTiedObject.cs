using Photon.Pun;
using Player;
using System.Collections.Generic;
using UnityEngine;

public class GaiaTiedObject : SpawnObject
{
    public ElementalOrderData elementalOrderData;

    float castingTime;

    float lifeTime;
    float currentLifeTime = 0f;

    float cylinderCheckTime;
    float currentCylinderCheckTime = 0f;

    int hitCount = 5;
    int currentHitCount = 0;
    float hitTimer;
    float currentHitTimer;

    List<GameObject> cylinders = new List<GameObject>();
    List<GameObject> hitObjects = new List<GameObject>();

    void Start()
    {
        Init();
    }
    void Init()
    {
        castingTime = elementalOrderData.gaiaTiedCastingTime;
        lifeTime = elementalOrderData.gaiaTiedLifeTime;
        hitTimer = elementalOrderData.gaiaTiedLifeTime / hitCount / 4;
        currentHitTimer = hitTimer;

        Vector3 _target = (Vector3)data[3];
        transform.rotation = Quaternion.LookRotation(_target);
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);

        for (int i = 0; i < hitCount; i++)
        {
            cylinders.Add(transform.GetChild(i).gameObject);
            cylinders[i].GetComponent<TriggerEventer>().hitTriggerEvent += TriggerCylinderEvent;
            cylinders[i].GetComponent<AudioSource>().volume = SettingManager.Instance.effectVal * SettingManager.Instance.soundVal;
            ParticleSystem[] _temp = cylinders[i].transform.GetComponentsInChildren<ParticleSystem>();

            for(int j = 0; j < _temp.Length; j++)
            {
                _temp[j].startLifetime = hitTimer * 4;
            }
        }
    }

    void FixedUpdate()
    {
        CheckTimer();
    }

    void CheckTimer()
    {
        if (castingTime > 0f)
        {
            castingTime -= Time.fixedDeltaTime;
            if (castingTime <= 0f)
                PlayNext();
        }
        else
        {
            lifeTime -= Time.fixedDeltaTime;
            currentHitTimer -= Time.fixedDeltaTime;
            if(currentHitTimer <= 0f)
                PlayNext();

            if(lifeTime <= 0f && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }

    private void PlayNext()
    {
        if (currentHitCount < hitCount)
        {
            hitObjects.Clear();
            cylinders[currentHitCount].gameObject.SetActive(true);
            if (currentHitCount > 0)
            {
                cylinders[currentHitCount - 1].GetComponent<Collider>().enabled = false;
            }
        }
        currentHitCount++;
        currentHitTimer = hitTimer;
    }


    void TriggerCylinderEvent(GameObject hitObject)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject _rootObject = hitObject.transform.root.gameObject;
            if (_rootObject.layer == 15 && _rootObject.tag == tag)
            {
                for(int i = 0; i < hitObjects.Count; i++)
                {
                    if (hitObjects[i] == _rootObject)
                        return;
                }

                hitObjects.Add(_rootObject);
                _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.All,
                    playerName, (int)(elementalOrderData.gaiaTiedDamage), hitObject.name, Vector3.up, 20f);
            }
        }
    }
}
