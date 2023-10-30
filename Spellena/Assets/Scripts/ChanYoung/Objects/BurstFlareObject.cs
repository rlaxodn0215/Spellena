using Photon.Pun;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstFlareObject : SpawnObject, IPunObservable
{
    Vector3 direction;
    Rigidbody rigidbody;

    float coolDownTime = 4f;
    float currentcoolDownTime = 0f;

    public override void OnEnable()
    {
        base.OnEnable();
        if (PhotonNetwork.IsMasterClient)
        {
            Init();
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            MovePosition();
            CheckTime();
        }
    }

    void MovePosition()
    {
        rigidbody.MovePosition(this.transform.position + direction * Time.deltaTime * 100);
    }

    void CheckTime()
    {
        currentcoolDownTime -= Time.deltaTime;
        if(currentcoolDownTime <= 0f)
        {
            DestorySpawnObject();
        }
    }

    void Init()
    {
        if (data != null)
        {
            this.gameObject.transform.position = (Vector3)data[3];
            direction = (Vector3)data[4];
        }
        currentcoolDownTime = coolDownTime;
        rigidbody = GetComponent<Rigidbody>();
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
