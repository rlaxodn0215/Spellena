using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class DimensionDoor : SpawnObject
    {
        public int lifeTime;
        public float range;
        public int deBuffNum;
        private LayerMask enemyLayerMask;
        private LayerMask enemyProjectileLayerMask;
        private List<string> playerInArea;

        public override void Start()
        {
            base.Start();

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

            playerInArea = new List<string>();
            GetComponent<SphereCollider>().radius = range;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (LayerMask.NameToLayer(other.tag) == enemyLayerMask)
            {
                if(other.gameObject.layer != enemyProjectileLayerMask)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        DeBuff(other.transform.root.GetComponent<Character>().ID);
                    }

                    else
                    {
                        photonView.RPC("RequestDeBuff", RpcTarget.AllBuffered, other.transform.root.GetComponent<Character>().ID);
                    }
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if ((LayerMask.NameToLayer(other.tag) & enemyLayerMask)
                == LayerMask.NameToLayer(other.tag))
            {
                  if (PhotonNetwork.IsMasterClient)
                  {
                      EnBuff(other.transform.root.GetComponent<Character>().ID);
                  }

                  else
                  {
                    photonView.RPC("RequestEnBuff", RpcTarget.AllBuffered, other.transform.root.GetComponent<Character>().ID);
                  }
                
            }
        }

        [PunRPC]
        public void RequestDeBuff(int Id)
        {
            DeBuff(Id);
        }

        [PunRPC]
        public void RequestEnBuff(int Id)
        {
            EnBuff(Id);
        }

        void DeBuff(int Id)
        {
            Debug.Log("DeBuff");
            GameObject temp = GameObject.Find("Player_" + Id);
            if (temp == null) return;

            foreach(string player in playerInArea)
            {
                if (player == temp.name) return;
            }

            playerInArea.Add(temp.name);
            temp.GetComponent<Character>().walkSpeed -= deBuffNum;
            temp.GetComponent<Character>().runSpeed -= deBuffNum;
            temp.GetComponent<Character>().jumpHeight -= deBuffNum;
        }

        void EnBuff(int Id)
        {
            Debug.Log("EnBuff");
            GameObject temp = GameObject.Find("Player_" + Id);
            if (temp == null) return;

            foreach (string player in playerInArea)
            {
                if (player == temp.name)
                {
                    temp.GetComponent<Character>().walkSpeed += deBuffNum;
                    temp.GetComponent<Character>().runSpeed += deBuffNum;
                    temp.GetComponent<Character>().jumpHeight += deBuffNum;
                    playerInArea.Remove(player);
                    if (playerInArea.Count == 0) return;
                }
            }
        }

    }
}