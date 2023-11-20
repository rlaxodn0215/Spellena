using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagnaEdgeObject : SpawnObject, IPunObservable
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
                        isReverse = true;
                }
                else
                {
                    cylinder.transform.localScale = new Vector3(cylinder.transform.localScale.x,
                        Mathf.Lerp(cylinder.transform.localScale.y, 0f, Time.deltaTime * 4), cylinder.transform.localScale.z);
                }
                cylinder.transform.localPosition = new Vector3(cylinder.transform.localPosition.x,
                    cylinder.transform.localScale.y, cylinder.transform.localPosition.z);
                currentCylinderLifeTime -= Time.deltaTime;
                if(currentCylinderLifeTime < 0f)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            else if(isFloorColliderOn == false)
            {
                isFloorColliderOn = true;
                for(int i = 0; i < 3; i++)
                {
                    floor.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
                }
            }
            else
            {
                currentFloorLifeTime -= Time.deltaTime;
                if(currentFloorLifeTime <= 0f)
                {
                    isCylinderColliderOn = true;
                    cylinder.SetActive(true);
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
        castingTime = elementalOrderData.ragnaEdgeCastingTime;
        floorLifeTime = elementalOrderData.ragnaEdgeFloorLifeTime;
        cylinderLifeTime = elementalOrderData.ragnaEdgeCylinderLifeTime;

        currentCastingTime = castingTime;
        currentFloorLifeTime = floorLifeTime;
        currentCylinderLifeTime = cylinderLifeTime;

        cylinder.SetActive(false);
        hitColliderObject.GetComponent<TriggerEventer>().hitTriggerEvent += triggerFloorEvent;
        cylinder.GetComponent<TriggerEventer>().hitTriggerEvent += triggerCylinderEvent;
    }

    void triggerFloorEvent(GameObject gameObject)
    {
        if (isFloorColliderOn)
        {
            Debug.Log("히트");
        }
    }

    void triggerCylinderEvent(GameObject gameObject)
    {
        if (isCylinderColliderOn)
        {
            Debug.Log("히트");
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
