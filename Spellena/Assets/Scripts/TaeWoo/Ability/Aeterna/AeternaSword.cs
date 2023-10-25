using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class AeternaSword : MonoBehaviour
    {
        [HideInInspector]
        public SpawnObjectName contactObjectName;
        [HideInInspector]
        public Aeterna player;

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
            if (PhotonNetwork.IsMasterClient && other.CompareTag(enemyTag))
            {
                if (player.playerActionDatas[(int)PlayerActionState.Skill2].isExecuting && player.skill2Phase == 1)
                {
                    if(other.gameObject.GetComponent<SpawnObject>().type !=SpawnObjectName.NoDamage)
                        contactObjectName = other.gameObject.GetComponent<SpawnObject>().type;

                   other.gameObject.GetComponent<SpawnObject>().DestorySpawnObject();
                   player.dimensionIO.CheckHold();
                }

                else
                {
                   other.gameObject.GetComponent<Character>().PlayerDamaged(player.playerName, damage);
                }
            }
        }
    }
}