using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerraBreakObject : SpawnObject
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

    List<string> hitPlayers = new List<string>();

    void Start()
    {
        Init();
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
            if (currentCastingTime <= 0f)
                RequestRPC("ActiveHitEffect");
        }
        else
        {
            if (currentHitCountTimer <= 0f)
            {
                currentHitCountTimer = hitCountTimer;
                currentColliderTimer = colliderTimer;
                RequestRPC("ActiveCollider");
            }
            else
            {
                currentHitCountTimer -= Time.deltaTime;
                if (isColliderOn)
                {
                    currentColliderTimer -= Time.deltaTime;

                    if(currentColliderTimer <= 0f)
                    {
                        RequestRPC("InactiveCollider");
                        hitPlayers.Clear();
                    }
                }
            }
            currentLifeTime -= Time.deltaTime;
            if(currentLifeTime <= 0f)
                RequestRPC("RequestDestroy");
            
        }
    }

    void Init()
    {
        hitCountTimer = lifeTime / hitCount;
        castingTime = elementalOrderData.terraBreakCastingTime;
        lifeTime = elementalOrderData.terraBreakLifeTime;
        hitCollider.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerEvent;

        currentCastingTime = castingTime;
        currentLifeTime = lifeTime;
    }

    void RequestRPC(string tunnelCommand)
    {
        object[] _tempData;
        if(tunnelCommand == "UpdateData")
        {
            _tempData = new object[5];
            _tempData[0] = tunnelCommand;
            _tempData[1] = currentCastingTime;
            _tempData[2] = currentLifeTime;
            _tempData[3] = currentHitCountTimer;
            _tempData[4] = currentColliderTimer;
        }
        else
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
        }

        photonView.RPC("CallRPCTunnelElementalOrderSpell5", RpcTarget.AllBuffered, _tempData);
    }
    [PunRPC]
    public void CallRPCTunnelElementalOrderSpell5(object[] data)
    {
        if ((string)data[0] == "UpdateData")
            UpdateData(data);
        else if ((string)data[0] == "ActiveCollider")
            ActiveCollider();
        else if ((string)data[0] == "InactiveCollider")
            InactiveCollider();
        else if ((string)data[0] == "RequestDestroy")
            RequestDestroy();
        else if ((string)data[0] == "ActiveHitEffect")
            ActiveHitEffect();
    }

    void ActiveHitEffect()
    {
        hitEffect.SetActive(true);
    }

    void RequestDestroy()
    {
        if(photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    void UpdateData(object[] data)
    {
        currentCastingTime = (float)data[1];
        currentLifeTime = (float)data[2];
        currentHitCountTimer = (float)data[3];
        currentColliderTimer = (float)data[4];
    }

    void ActiveCollider()
    {
        isColliderOn = true;
    }

    void InactiveCollider()
    {
        isColliderOn = false;
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
                        if (hitPlayers[i] == other.gameObject.name)
                        {
                            _isCheck = true;
                        }
                    }

                    if (_isCheck)
                        return;
                    else
                    {
                        hitPlayers.Add(other.gameObject.name);
                        Debug.Log("데미지");
                    }
                }
            }
        }
    }
}
