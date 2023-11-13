using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerraBreakObject : SpawnObject, IPunObservable
{
    public ElementalOrderData elementalOrderData;

    public GameObject rangeArea;
    public GameObject hitEffect;

    float castingTime;
    float currentCastingTime = 0f;

    float lifeTime;
    float currentLifeTime = 0f;

    float hitCountTimer;
    float currentHitCountTimer = 0f;

    float colliderTimer = 0.1f;
    float currentColliderTimer = 0f;

    int hitCount = 5;

    bool isColliderOn = false;

    public GameObject hitCollider;

    List<GameObject> hitPlayers = new List<GameObject>();

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
        }
    }

    void CheckTimer()
    {
        if(currentCastingTime > 0f)
        {
            currentCastingTime -= Time.deltaTime;

            if(currentCastingTime <= 0f)
            {
                hitEffect.SetActive(true);
            }
        }
        else
        {
            if (currentHitCountTimer <= 0f)
            {
                currentHitCountTimer = hitCountTimer;
                currentColliderTimer = colliderTimer;
                isColliderOn = true;
            }
            else
            {
                currentHitCountTimer -= Time.deltaTime;
                if (isColliderOn)
                {
                    currentColliderTimer -= Time.deltaTime;

                    if(currentColliderTimer <= 0f)
                    {
                        isColliderOn = false;
                        hitPlayers.Clear();
                    }
                }
            }
            currentLifeTime -= Time.deltaTime;
            if(currentLifeTime <= 0f)
            {
                PhotonNetwork.Destroy(gameObject);
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
        castingTime = elementalOrderData.terraBreakCastingTime;
        lifeTime = elementalOrderData.terraBreakLifeTime;

        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;
        hitCountTimer = lifeTime / hitCount;
        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;
    }

    void TriggerEvent(GameObject gameObject)
    {
        if(isColliderOn)
        {
            Debug.Log("히트");
        }
    }    

    private void OnTriggerStay(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (isColliderOn == true)
            {
                if (other.gameObject.GetComponent<Character>() != null)
                {
                    bool _isCheck = false;
                    for (int i = 0; i < hitPlayers.Count; i++)
                    {
                        if (hitPlayers[i] == other.gameObject)
                        {
                            _isCheck = true;
                        }
                    }

                    if (_isCheck)
                        return;
                    else
                    {
                        hitPlayers.Add(other.gameObject);
                        Debug.Log("데미지");
                    }
                }
            }
        }
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);

        if(stream.IsWriting)
        {

        }
        else
        {

        }
    }
}
