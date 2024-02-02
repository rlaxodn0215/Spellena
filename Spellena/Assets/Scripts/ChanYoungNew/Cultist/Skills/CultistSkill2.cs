using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class CultistSkill2 : InstantiateObject
{
    public Camera camera;

    protected override void Start()
    {
        base.Start();
        if (photonView.IsMine)
            GetComponent<PlayerInput>().enabled = true;
    }

    protected void FixedUpdate()
    {
        if(photonView.IsMine)
        {
            GetComponent<Rigidbody>().velocity = transform.forward * 5f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject _temp = other.transform.root.gameObject;
            if (_temp.layer == 15 && _temp.tag != tag)
            {
                //playerPhotonView.RPC(); -> 텔레포트
                playerPhotonView.RPC("TeleportPlayer", RpcTarget.All, _temp.GetComponent<PhotonView>().ViewID);
            }
        }
    }


    private void OnMouseMove(InputValue inputValue)
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x - inputValue.Get<Vector2>().y / 5f, 
            transform.eulerAngles.y + inputValue.Get<Vector2>().x / 5f, 0);
    }



}
