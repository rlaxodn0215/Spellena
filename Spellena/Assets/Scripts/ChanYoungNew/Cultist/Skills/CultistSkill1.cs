using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistSkill1 : InstantiateObject, IPunObservable
{
    private float lifeTime;

    protected override void Start()
    {
        base.Start();
        lifeTime = 1f;

        transform.parent = playerPhotonView.transform;
    }

    private void FixedUpdate()
    {
        lifeTime -= Time.deltaTime;

        if(lifeTime <= 0f && photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
