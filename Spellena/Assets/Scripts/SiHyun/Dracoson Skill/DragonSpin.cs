using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonSpin : SpawnObject
{
    public TriggerEventer triggerEventer;
    public DracosonData dracosonData;

    public GameObject centerObject;
    public GameObject redDragon;
    public GameObject blueDragon;
    public GameObject greenDragon;

    public float distance = 2.5f;
    public float rotationSpeed = 360.0f;
    public float knockbackForce = 10f;

    private float totalRotation = 0.0f;

    

    private void Start()
    {
        SetDragonPositionAndRotation(redDragon, 60);
        SetDragonPositionAndRotation(blueDragon, -60);
        SetDragonPositionAndRotation(greenDragon, 180);
        centerObject.GetComponent<SphereCollider>().radius = distance;
        Init();
    }

    void Update()
    {
        /*SetDragonPositionAndRotation(redDragon, 60);
        SetDragonPositionAndRotation(blueDragon, -60);
        SetDragonPositionAndRotation(greenDragon, 180);*/
        totalRotation += rotationSpeed * Time.deltaTime;

        if (totalRotation < 1080.0f)
        {
            float rotationAmount = rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.down, rotationAmount);
        }
        else
        {
            CallRPCTunnel("RequestDestroy");
        }
    }

    void Init()
    {
        triggerEventer.hitTriggerEvent += TriggerEvent;
    }



    void SetDragonPositionAndRotation(GameObject dragon, float angle)
    {
        dragon.transform.localPosition = Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;
        dragon.transform.localRotation = Quaternion.Euler(0, angle - 90, 0);
    }

    void CallRPCTunnel(string tunnelCommand)
    {
        object[] _tempData;
        _tempData = new object[2];
        _tempData[0] = tunnelCommand;

        photonView.RPC("CallRPCTunnelDracosonDragonSpin", RpcTarget.AllBuffered, _tempData);
    }

    [PunRPC]
    public void CallRPCTunnelDracosonDragonSpin(object[] data)
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
        if(PhotonNetwork.IsMasterClient)
        {
            if (hitObject.gameObject.layer == 16)
            {
                photonView.RPC("DestoryObject", RpcTarget.AllBuffered, hitObject);
            }
            if (hitObject.transform.root.gameObject.name != hitObject.name)
            {
                GameObject _rootObject = hitObject.transform.root.gameObject;
                if(_rootObject.GetComponent<Character>() != null)
                {
                    float _xPos = hitObject.transform.position.x - transform.position.x;
                    float _zPos = hitObject.transform.position.z - transform.position.z;
                    float _distance = _xPos * _xPos + _zPos * _zPos;
                    Vector3 _outsideVector = new Vector3(_xPos, 0, _zPos).normalized;
                    //if(_rootObject.tag != tag)
                    {
                        Vector3 _knockbackDirection =
                            (_rootObject.transform.position - transform.position).normalized;
                        //_rootObject.GetComponent<Rigidbody>().AddForce(_knockbackDirection * knockbackForce, ForceMode.Impulse);
                        _rootObject.GetComponent<Rigidbody>().AddForce(_outsideVector * knockbackForce, ForceMode.Impulse);

                        _rootObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBuffered,
                        playerName, (int)(dracosonData.skill1Damage), hitObject.name, transform.forward, 20f);
                    }
                }
            }
        }
    }

    [PunRPC]
    void DestoryObject(GameObject hitObject)
    {
        // 모든 클라이언트에서 해당 오브젝트를 삭제
        PhotonView _pv = hitObject.GetComponent<PhotonView>();
        if (_pv != null && _pv.IsMine)
        {
            PhotonNetwork.Destroy(hitObject);
        }
    }

    
}
