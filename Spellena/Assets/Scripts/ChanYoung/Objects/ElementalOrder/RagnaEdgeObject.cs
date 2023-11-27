using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RagnaEdgeObject : SpawnObject
{
    public ElementalOrderData elementalOrderData;

    public GameObject floor;
    public GameObject cylinder;
    public GameObject hitColliderObject;
    public GameObject RangeArea;

    float floorDamage;
    float cylinderDamage;

    float castingTime;
    float currentCastingTime = 0f;
    float floorLifeTime;
    float currentFloorLifeTime = 0f;
    float cylinderLifeTime;
    float currentCylinderLifeTime = 0f;

    bool isCylinderColliderOn = false;
    bool isFloorColliderOn = false;
    bool isReverse = false;

    Vector3 localScaleLerp;
    Vector3 localPositionLerp;

    List<string> hitFloorObjects = new List<string>();
    List<string> hitCylinderObjects = new List<string>();

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            CheckTimer();
        LerpCylinder();
    }

    void LerpCylinder()
    {
        cylinder.transform.localScale = Vector3.Lerp(cylinder.transform.localScale, localScaleLerp, Time.deltaTime * 10);
        cylinder.transform.localPosition = Vector3.Lerp(cylinder.transform.localPosition, localPositionLerp, Time.deltaTime * 10);
    }

    void CheckTimer()
    {
        if (currentCastingTime > 0f)
        {
            currentCastingTime -= Time.deltaTime;
        }
        else
        {
            if(isCylinderColliderOn == true)
            {
                if (isReverse == false)
                {
                    cylinder.transform.localScale = new Vector3(cylinder.transform.localScale.x,
                        cylinder.transform.localScale.y + Time.deltaTime * 4 / cylinderLifeTime * 1.1f, cylinder.transform.localScale.z);
                    if (cylinder.transform.localScale.y > 2f)
                        RequestRPC("ReverseCylinder");
                }
                else
                {
                    if (cylinder.transform.localScale.y > 0f)
                    {
                        cylinder.transform.localScale = new Vector3(cylinder.transform.localScale.x,
                            cylinder.transform.localScale.y - Time.deltaTime * 4 / cylinderLifeTime * 1.1f, cylinder.transform.localScale.z);
                    }
                }
                cylinder.transform.localPosition = new Vector3(cylinder.transform.localPosition.x,
                    cylinder.transform.localScale.y, cylinder.transform.localPosition.z);
                currentCylinderLifeTime -= Time.deltaTime;

                if (currentCylinderLifeTime < 0f)
                    RequestRPC("RequestDestroy");
            }
            else if(isFloorColliderOn == false)
            {
                RequestRPC("ActiveFloor");
            }
            else
            {
                currentFloorLifeTime -= Time.deltaTime;
                if(currentFloorLifeTime <= 0f)
                    RequestRPC("ActiveCylinder");
            }
        }

        RequestRPC("UpdateData");
    }

    void Init()
    {
        floorDamage = elementalOrderData.ragnaEdgeFloorDamage;
        cylinderDamage = elementalOrderData.ragnaEdgeCylinderDamage;
        castingTime = elementalOrderData.ragnaEdgeCastingTime;
        floorLifeTime = elementalOrderData.ragnaEdgeFloorLifeTime;
        cylinderLifeTime = elementalOrderData.ragnaEdgeCylinderLifeTime;

        cylinder.SetActive(false);
        hitColliderObject.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerFloorEvent;
        cylinder.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerCylinderEvent;

        currentCastingTime = castingTime;
        currentFloorLifeTime = floorLifeTime;
        currentCylinderLifeTime = cylinderLifeTime;

        localPositionLerp = cylinder.transform.localPosition;
        localScaleLerp = cylinder.transform.localScale;

        BalanceAnimation();
    }

    void BalanceAnimation()
    {
        RangeArea.GetComponent<ParticleSystem>().startLifetime = castingTime * 0.85f;

        for (int i = 0; i < 3; i++)
        {
            floor.transform.GetChild(i).GetComponent<ParticleSystem>().startLifetime = floorLifeTime;
        }
    }

    void RequestRPC(string tunnelCommand)
    {
        object[] _tempData;
        if(tunnelCommand == "UpdateData")
        {
            _tempData = new object[8];
            _tempData[0] = tunnelCommand;
            _tempData[1] = currentCastingTime;
            _tempData[2] = currentFloorLifeTime;
            _tempData[3] = currentCylinderLifeTime;
            _tempData[4] = cylinder.transform.localScale;
            _tempData[5] = cylinder.transform.localPosition;
            _tempData[6] = hitFloorObjects.ToArray();
            _tempData[7] = hitCylinderObjects.ToArray();
        }
        else
        {
            _tempData = new object[2];
            _tempData[0] = tunnelCommand;
        }

        photonView.RPC("CallRPCTunnelElementalOrderSpell1", RpcTarget.AllBuffered, _tempData);
    }

    [PunRPC]
    public void CallRPCTunnelElementalOrderSpell1(object[] data)
    {
        if ((string)data[0] == "ReverseCylinder")
            ReverseCylinder();
        else if ((string)data[0] == "ActiveCylinder")
            ActiveCylinder();
        else if ((string)data[0] == "ActiveFloor")
            ActiveFloor();
        else if ((string)data[0] == "UpdateData")
            UpdateData(data);
        else if ((string)data[0] == "RequestDestroy")
            RequestDestroy();

    }

    void TriggerFloorEvent(GameObject hitObject)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (isFloorColliderOn)
            {
                if (hitObject.transform.root.gameObject.name != hitObject.name)
                {
                    GameObject _rootObject = hitObject.transform.root.gameObject;
                    if (_rootObject.GetComponent<Character>() != null)
                    {
                        if (tag != _rootObject.tag)
                        {
                            for (int i = 0; i < hitFloorObjects.Count; i++)
                            {
                                if (hitFloorObjects[i] == _rootObject.name)
                                    return;
                            }

                            hitFloorObjects.Add(_rootObject.name);
                            _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.MasterClient,
                                playerName, (int)elementalOrderData.ragnaEdgeFloorDamage, hitObject.name, transform.forward, 20f);
                        }   
                    }
                }
            }
        }
    }

    void TriggerCylinderEvent(GameObject hitObject)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (isCylinderColliderOn)
            {
                if (hitObject.transform.root.gameObject.name != hitObject.name)
                {
                    GameObject _rootObject = hitObject.transform.root.gameObject;
                    if (_rootObject.GetComponent<Character>() != null)
                    {
                        for (int i = 0; i < hitCylinderObjects.Count; i++)
                        {
                            if (hitCylinderObjects[i] == _rootObject.name)
                                return;
                        }

                        if (tag != _rootObject.tag)
                        {
                            hitCylinderObjects.Add(_rootObject.name);
                            _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.MasterClient,
                                playerName, (int)elementalOrderData.ragnaEdgeCylinderDamage, hitObject.name, transform.forward, 20f);
                        }
                    }
                }
            }
        }
    }
    
    void ReverseCylinder()
    {
        isReverse = true;
    }
    
    void ActiveCylinder()
    {
        isCylinderColliderOn = true;
        cylinder.SetActive(true);
        isFloorColliderOn = false;
    }
    
    void ActiveFloor()
    {
        isFloorColliderOn = true;
        for (int i = 0; i < 3; i++)
        {
            floor.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
        }
    }
    
    void UpdateData(object[] data)
    {
        currentCastingTime = (float)data[1];
        currentFloorLifeTime = (float)data[2];
        currentCylinderLifeTime = (float)data[3];
        localScaleLerp = (Vector3)data[4];
        localPositionLerp = (Vector3)data[5];
        hitFloorObjects = ((string[])data[6]).ToList();
        hitCylinderObjects = ((string[])data[7]).ToList();
    }
    
    void RequestDestroy()
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
