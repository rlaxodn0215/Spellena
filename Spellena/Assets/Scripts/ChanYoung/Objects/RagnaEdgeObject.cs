using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagnaEdgeObject : SpawnObject, IPunObservable
{

    float castingTime = 2f;
    float currentCastingTime = 0f;

    float floorLifeTime = 1f;
    float currentFloorLifeTime = 0f;
    float cylinderLifeTime = 2f;
    float currentCylinderLifeTime = 0f;

    bool isCylinderColliderOn = false;
    bool isFloorColliderOn = false;
    Collider hitFloorCollider;
    MeshRenderer floorRenderer;
    Collider hitCylinderCollider;
    MeshRenderer cylinderRenderer;

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
                currentCylinderLifeTime -= Time.deltaTime;
                if(currentCylinderLifeTime < 0f)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            else if(isFloorColliderOn == false)
            {
                isFloorColliderOn = true;
                hitFloorCollider.enabled = true;
                floorRenderer.enabled = true;
            }
            else
            {
                currentFloorLifeTime -= Time.deltaTime;
                if(currentFloorLifeTime <= 0f)
                {
                    isCylinderColliderOn = true;
                    hitFloorCollider.enabled = false;
                    floorRenderer.enabled = false;
                    hitCylinderCollider.enabled = true;
                    cylinderRenderer.enabled = true;
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
        currentCastingTime = castingTime;
        currentFloorLifeTime = floorLifeTime;
        currentCylinderLifeTime = cylinderLifeTime;
        hitFloorCollider = gameObject.transform.GetChild(0).GetComponent<Collider>();
        hitCylinderCollider = gameObject.transform.GetChild(1).GetComponent<Collider>();
        floorRenderer = gameObject.transform.GetChild(0).GetComponent<MeshRenderer>();
        cylinderRenderer = gameObject.transform.GetChild(1).GetComponent<MeshRenderer>();
        hitFloorCollider.enabled = false;
        hitCylinderCollider.enabled = false;
        floorRenderer.enabled = false;
        cylinderRenderer.enabled = false;
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
