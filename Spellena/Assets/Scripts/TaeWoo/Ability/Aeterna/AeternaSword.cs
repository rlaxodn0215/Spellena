using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class AeternaSword : MonoBehaviour
    {
        [HideInInspector]
        public GameObject contactObject;
        [HideInInspector]
        public Aeterna player;

        public int damage;
        private LayerMask enemyLayerMask;

        private void Start()
        {
            player = transform.root.gameObject.GetComponent<Aeterna>();

            if(CompareTag("TeamA"))
            {
                enemyLayerMask = LayerMask.NameToLayer("TeamB");
            }

            else if(CompareTag("TeamB"))
            {
                enemyLayerMask = LayerMask.NameToLayer("TeamA");
            }
            
        }

        public void OnTriggerEnter(Collider other)
        {




            if (PhotonNetwork.IsMasterClient)
            {
                //player.GetComponent<PhotonView>().RPC("PlayerDamaged",)

                //if(player.skill2Phase==0)
                //{

                //}
                //if (other.gameObject.layer == layerMask)
                //{
                //    contactObject = other.gameObject;
                //    player.dimensionIO.CheckHold();
                //    Debug.Log("적 투사체 충돌");
                //}
            }

            else
            {

            }
        }

        public void Attack()
        {

        }

        public void CatchProjectile()
        {

        }
    }
}