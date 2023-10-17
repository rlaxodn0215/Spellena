using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public class DimensionSlash : MonoBehaviour
    {
        public int damage;
        public int lifeTime;
        public int Speed;

        [HideInInspector]
        public Aeterna owner;
        [HideInInspector]
        public Vector3 dir;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            if(owner !=null)
            {
                if(owner.tag == "TeamA")
                {
                    this.gameObject.layer = LayerMask.NameToLayer("ProjectileA");
                }

                else if(owner.tag == "TeamB")
                {
                    this.gameObject.layer = LayerMask.NameToLayer("ProjectileB");
                }
            }

            if(owner.camera !=null)
                dir = owner.camera.transform.localRotation*Vector3.forward;

            yield return new WaitForSeconds(lifeTime);
            GetComponent<PhotonView>().RPC("Disappear", RpcTarget.AllBuffered);

        }

        // Update is called once per frame
        void Update()
        {
            Move();
        }

        private void Move()
        {
            transform.Translate(dir * Speed * Time.deltaTime);
        }

        [PunRPC]
        public void Disappear()
        {
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (owner.tag == null || other.tag == null) return; 

            if (gameObject.layer.ToString()== "ProjectileA" && other.tag == "TeamB" ||
                gameObject.layer.ToString() == "ProjectileB" && other.tag == "TeamA")
            {
                other.gameObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBuffered,owner,damage);
                Debug.Log("검기 맞음");
                Destroy(gameObject);
            }

        }
    }
}