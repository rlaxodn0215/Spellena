using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RagnaEdgeObject : SpawnObject
{
    public ElementalOrderData elementalOrderData;

    public GameObject floor;
    public GameObject cylinder;
    public GameObject hitColliderObject;

    float castingTime;
    float currentCastingTime = 0f;

    float floorLifeTime;
    float currentFloorLifeTime = 0f;
    float cylinderLifeTime;
    float currentCylinderLifeTime = 0f;

    bool isCylinderColliderOn = false;
    bool isFloorColliderOn = false;

    bool isReverse = false;

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
                        cylinder.transform.localScale.y + Time.deltaTime * cylinderLifeTime * 2, cylinder.transform.localScale.z);
                    if (cylinder.transform.localScale.y > 2f)
                        RequestRPC("ReverseCylinder");
                }
                else
                {
                    cylinder.transform.localScale = new Vector3(cylinder.transform.localScale.x,
                        Mathf.Lerp(cylinder.transform.localScale.y, 0f, Time.deltaTime * 4), cylinder.transform.localScale.z);
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
        castingTime = elementalOrderData.ragnaEdgeCastingTime;
        floorLifeTime = elementalOrderData.ragnaEdgeFloorLifeTime;
        cylinderLifeTime = elementalOrderData.ragnaEdgeCylinderLifeTime;

        cylinder.SetActive(false);
        hitColliderObject.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerFloorEvent;
        cylinder.GetComponent<TriggerEventer>().hitTriggerEvent += TriggerCylinderEvent;

        currentCastingTime = castingTime;
        currentFloorLifeTime = floorLifeTime;
        currentCylinderLifeTime = cylinderLifeTime;
    }

    void RequestRPC(string tunnelCommand)
    {
        object[] _tempData;
        if(tunnelCommand == "UpdateData")
        {
            _tempData = new object[6];
            _tempData[0] = tunnelCommand;
            _tempData[1] = currentCastingTime;
            _tempData[2] = currentFloorLifeTime;
            _tempData[3] = currentCylinderLifeTime;
            _tempData[4] = cylinder.transform.localScale;
            _tempData[5] = cylinder.transform.localPosition;
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

    void TriggerFloorEvent(GameObject gameObject)
    {
        if (isFloorColliderOn)
        {
            Debug.Log("히트");
        }
    }

    void TriggerCylinderEvent(GameObject gameObject)
    {
        if (isCylinderColliderOn)
        {
            Debug.Log("히트");
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
        cylinder.transform.localScale = (Vector3)data[4];
        cylinder.transform.localPosition = (Vector3)data[5];
    }
    
    void RequestDestroy()
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
