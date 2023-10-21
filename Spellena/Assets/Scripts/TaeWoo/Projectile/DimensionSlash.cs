using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public class DimensionSlash : SpawnObject
    {
        public Aeterna owner;
        public int damage;
        public int lifeTime;
        public int Speed;

        //public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        //{
        //    base.OnPhotonSerializeView(stream, info);

        //    if (stream.IsWriting)
        //    {
        //        // 데이터를 보내는 부분
        //        ID = (int)stream.ReceiveNext();
        //        stream.SendNext(direction);
        //    }

        //    else
        //    {
        //        // 데이터를 받는 부분
        //        ID = (int)stream.ReceiveNext();
        //        direction = (Vector3)stream.ReceiveNext();
        //    }
        //}

        void Start()
        {
            if (owner != null)
            {
                ID = owner.GetComponent<PhotonView>().ViewID;

                if (owner.tag == "TeamA")
                {
                    this.gameObject.layer = LayerMask.NameToLayer("ProjectileA");
                }

                else if (owner.tag == "TeamB")
                {
                    this.gameObject.layer = LayerMask.NameToLayer("ProjectileB");
                }
            }

            if(owner.camera !=null)
                direction = owner.camera.transform.localRotation*Vector3.forward;

            StartCoroutine(Gone());

        }

        IEnumerator Gone()
        {
            yield return new WaitForSeconds(lifeTime);
            GetComponent<PhotonView>().RPC("Disappear", RpcTarget.AllBuffered);
        }

        // Update is called once per frame
        public void Update()
        {
            Move();
        }

        private void Move()
        {
            transform.Translate(direction * Speed * Time.deltaTime);
        }

        [PunRPC]
        public void Disappear()
        {
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (owner.tag == null || other.tag == null) return;

                if (gameObject.layer.ToString() == "ProjectileA" && other.tag == "TeamB" ||
                    gameObject.layer.ToString() == "ProjectileB" && other.tag == "TeamA")
                {
                    other.gameObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBuffered, owner, damage);
                    Debug.Log("검기 맞음");
                    Destroy(gameObject);
                }
            }
        }
    }
}