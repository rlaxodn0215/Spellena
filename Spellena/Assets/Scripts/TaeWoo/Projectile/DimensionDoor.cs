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
        private List<string> playerInArea;

        public override void Start()
        {
            base.Start();

            if(CompareTag("TeamA"))
                enemyLayerMask = LayerMask.NameToLayer("TeamB");
            else if(CompareTag("TeamB"))
                enemyLayerMask = LayerMask.NameToLayer("TeamA");

            playerInArea = new List<string>();
            GetComponent<SphereCollider>().radius = range;
        }

        public void OnTriggerEnter(Collider other)
        {
            Debug.Log("Portal_Enter");

            if (other.tag == LayerMask.LayerToName(enemyLayerMask))
            {
                if (other.gameObject.layer == enemyLayerMask)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        //other.gameObject.GetComponent<SpawnObject>().DestorySpawnObject();
                    }

                    else
                    {
                        //other.gameObject.GetComponent<PhotonView>().RPC("DestorySpawnObject", RpcTarget.MasterClient);
                    }
                }

                else
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        DeBuff(ID);
                    }

                    else
                    {
                        photonView.RPC("RequestDeBuff", RpcTarget.MasterClient, ID);
                    }
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            Debug.Log("Portal_Exit");

            if (other.tag == LayerMask.LayerToName(enemyLayerMask))
            {
                  if (PhotonNetwork.IsMasterClient)
                  {
                      EnBuff(ID);
                  }

                  else
                  {
                      photonView.RPC("RequestEnBuff", RpcTarget.MasterClient, ID);
                  }
                
            }
        }

        [PunRPC]
        public void RequestDeBuff(int Id)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                DeBuff(Id);
            }
        }

        [PunRPC]
        public void RequestEnBuff(int Id)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                EnBuff(Id);
            }
        }

        void DeBuff(int Id)
        {
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
                }
            }
        }

    }
}