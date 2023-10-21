using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public class DimensionSlash : SpawnObject
    {
        public int damage;
        public int lifeTime;
        public int Speed;

        public override void Start()
        {
            base.Start();
            direction = (Quaternion)data[2]*Vector3.forward;
        }

        IEnumerator Gone()
        {
            yield return new WaitForSeconds(lifeTime);

            if(PhotonNetwork.IsMasterClient)
            {

            }

            else
            {

            }

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
        public void DestorySlash()
        {
            if(PhotonNetwork.IsMasterClient)
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