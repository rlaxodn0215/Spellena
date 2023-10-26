using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class AeternaSword : MonoBehaviour
    {
        //[HideInInspector]
        //public SpawnObjectType contactObjectType;
        [HideInInspector]
        public string contactObjectName;
        [HideInInspector]
        public Aeterna player;

        public GameObject normalSword;
        public GameObject skill2BuffParticle;
        public GameObject skill3BuffParticle;
        public GameObject skill4AttackSword;
        public GameObject skill4HealingSword;

        public int damage;
        private string enemyTag;

        private void Start()
        {
            player = transform.root.gameObject.GetComponent<Aeterna>();
        }

        [PunRPC]
        public void SetSwordTag()
        {
            if (CompareTag("TeamA"))
            {
                enemyTag = "TeamB";  
            }

            else if (CompareTag("TeamB"))
            {
                enemyTag = "TeamA";
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.CompareTag(enemyTag))
            {
                if (player.playerActionDatas[(int)PlayerActionState.Skill2].isExecuting && player.skill2Phase == 1)
                {
                    if (other.transform.root.GetComponent<SpawnObject>())
                    {
                        if (other.transform.root.GetComponent<SpawnObject>().type == SpawnObjectType.Projectile)
                            contactObjectName = other.transform.root.GetComponent<SpawnObject>().objectName;

                        if (PhotonNetwork.IsMasterClient)
                        {
                            other.transform.root.GetComponent<SpawnObject>().DestorySpawnObject();
                        }

                        else
                        {
                            other.transform.root.GetComponent<PhotonView>().RPC("DestorySpawnObject", RpcTarget.MasterClient);
                        }

                        player.dimensionIO.CheckHold();
                    }

                }

                else if (player.playerActionDatas[(int)PlayerActionState.Skill3].isExecuting && player.skill3Phase == 1)
                {
                    if (other.transform.root.GetComponent<Character>())
                        player.dimensionTransport.Transport(other.transform.root.gameObject);
                }

                else
                {
                    if (other.transform.root.GetComponent<Character>())
                        other.transform.root.GetComponent<Character>().PlayerDamaged(player.playerName, damage);
                }
            }

        }
    }
}