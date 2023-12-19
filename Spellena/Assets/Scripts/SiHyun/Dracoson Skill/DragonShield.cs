using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonShield : SpawnObject
{
    public TriggerEventer triggerEventer;
    public DracosonData dracosonData;
    public Transform center;
    List<string> hitObjects = new List<string>();
    private float deleteTime;
    private float shieldGage;

    private void Start()
    {
        Init();
        deleteTime = 5f;
        shieldGage = dracosonData.skill3ShieldGage;
    }

    void FixedUpdate()
    {
        deleteTime -= Time.deltaTime;
        if (shieldGage <= 0f || deleteTime <= 0f)
        {
            if(PhotonNetwork.IsMasterClient)
                CallRPCTunnel("RequestDestroy");
        }
    }

    void Init()
    {
        triggerEventer.hitTriggerEvent += TriggerEvent;
    }



    void CallRPCTunnel(string tunnelCommand)
    {
        object[] _tempData;
        _tempData = new object[2];
        _tempData[0] = tunnelCommand;

        photonView.RPC("CallRPCTunnelDracosonDragonShield", RpcTarget.AllBuffered, _tempData);
    }

    [PunRPC]
    public void CallRPCTunnelDracosonDragonShield(object[] data)
    {
        if ((string)data[0] == "RequestDestroy")
            RequestDestroy();
    }

    void RequestDestroy()
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    void TriggerEvent(GameObject hitObject)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (hitObject.gameObject.layer == 16)
            {
                if(!IsInternalProjectile(hitObject.transform.position))
                {
                    photonView.RPC("DestroyObject", RpcTarget.AllBuffered, hitObject);
                }
            }
        }
    }

    bool IsInternalProjectile(Vector3 position)
    {
        // 내부에서 생성된 투사체의 생성 가능한 영역을 기준으로 확인
        // 예를 들어, 구체 콜라이더의 중심을 기준으로 내부 영역을 지정할 수 있습니다.

        float internalRadius = 2.5f; // 내부 영역의 반지름

        // 구체 콜라이더의 중심과 충돌 지점 간의 거리를 계산하여 내부인지 여부 판단
        float distanceToCenter = Vector3.Distance(center.position, position);

        return distanceToCenter <= internalRadius;
    }

    [PunRPC]
    void DestroyObject(GameObject hitObject)
    {
        // 모든 클라이언트에서 해당 오브젝트를 삭제
        PhotonView _pv = hitObject.GetComponent<PhotonView>();
        if (_pv != null && _pv.IsMine)
        {
            PhotonNetwork.Destroy(hitObject);
        }
    }

}


