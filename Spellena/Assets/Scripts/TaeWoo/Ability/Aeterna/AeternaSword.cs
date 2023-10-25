using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class AeternaSword : MonoBehaviour
    {
        [HideInInspector]
        public string contactObjectName;
        [HideInInspector]
        public Aeterna player;

        public int damage;
        private LayerMask enemyLayerMask;
        private LayerMask enemyProjectileLayerMask;

        private void Start()
        {
            player = transform.root.gameObject.GetComponent<Aeterna>();   
        }

        [PunRPC]
        public void SetSwordLayer()
        {
            if (CompareTag("TeamA"))
            {
                enemyLayerMask = LayerMask.NameToLayer("TeamB");
                enemyProjectileLayerMask = LayerMask.NameToLayer("SpawnObjectB");
            }

            else if (CompareTag("TeamB"))
            {
                enemyLayerMask = LayerMask.NameToLayer("TeamA");
                enemyProjectileLayerMask = LayerMask.NameToLayer("SpawnObjectA");
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (player.playerActionDatas[(int)PlayerActionState.Skill2].isExecuting && player.skill2Phase == 1)
                {
                    if (other.gameObject.layer == enemyProjectileLayerMask)
                    {
                        contactObjectName = other.gameObject.name;

                        if (PhotonNetwork.IsMasterClient)
                        {
                            other.gameObject.GetComponent<SpawnObject>().DestorySpawnObject();
                        }

                        else
                        {
                            gameObject.GetComponent<PhotonView>().RPC("RequestDestorySpawnObject", RpcTarget.MasterClient);
                        }

                        player.dimensionIO.CheckHold();
                    }
                }

                else
                {
                    if (other.gameObject.layer == enemyLayerMask)
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            other.gameObject.GetComponent<Character>().PlayerDamaged(player.playerName, damage);
                        }

                        else
                        {
                            other.gameObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.MasterClient, player.playerName, damage);
                        }
                    }
                }
            }
        }
    }
}