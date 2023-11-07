using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerraBreakObject : SpawnObject, IPunObservable
{
    float castingTime = 4.5f;
    float currentCastingTime = 0f;

    float lifeTime = 3f;
    float currentLifeTime = 0f;

    float hitCountTimer;
    float currentHitCountTimer = 0f;

    float colliderTimer = 0.1f;
    float currentColliderTimer = 0f;

    int hitCount = 5;

    bool isColliderOn = false;

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
        }
        else
        {
            if(currentHitCountTimer <= 0f)
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
        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;
        hitCountTimer = lifeTime / hitCount;
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
                        Debug.Log("µ¥¹ÌÁö");
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
