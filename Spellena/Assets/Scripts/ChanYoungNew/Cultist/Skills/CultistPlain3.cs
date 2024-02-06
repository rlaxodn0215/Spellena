using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistPlain3 : InstantiateObject
{
    private Vector3 direction;
    private float lifeTime;

    protected override void Start()
    {
        base.Start();

        direction = (Vector3)data[2];
        transform.LookAt(direction);
        lifeTime = playerData.plainLifeTime[2];
    }

    private void FixedUpdate()
    {
        lifeTime -= Time.fixedDeltaTime;

        if(photonView.IsMine)
        {
            Vector3 _foward = transform.forward;

            GetComponent<Rigidbody>().velocity = _foward;

            if(lifeTime <= 0f)
                PhotonNetwork.Destroy(gameObject);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GameObject _rootObject = other.transform.root.gameObject;
            if(_rootObject.layer == 15 && _rootObject.tag != tag)
            {

                //µ¥¹ÌÁö
                photonView.RPC("DestroyObject", photonView.Owner);
            }
        }
    }

}
