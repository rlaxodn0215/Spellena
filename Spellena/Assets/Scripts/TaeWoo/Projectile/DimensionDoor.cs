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
        private string enemyTag;
        private List<string> playerInArea;

        public override void Start()
        {
            base.Start();

            name = playerName + "_Portal";
            type = SpawnObjectType.FixedObject;
            objectName = "Portal";

            if (CompareTag("TeamA"))
            {
                enemyTag = "TeamB";
            }

            else if (CompareTag("TeamB"))
            {
                enemyTag = "TeamA";
            }

            playerInArea = new List<string>();
            GetComponent<SphereCollider>().radius = range;

            StartCoroutine(Gone());
        }

        IEnumerator Gone()
        {
            yield return new WaitForSeconds(lifeTime);

            if (PhotonNetwork.IsMasterClient)
            {
                DestorySpawnObject();
            }

            else
            {
                photonView.RPC("RequestDestorySpawnObject", RpcTarget.AllBuffered);
            }

        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.CompareTag(enemyTag))
            {
                if(other.transform.root.gameObject.GetComponent<Character>())
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        DeBuff(other.transform.root.GetComponent<Character>().playerName);
                    }

                    else
                    {
                        photonView.RPC("RequestDeBuff", RpcTarget.AllBuffered, other.transform.root.GetComponent<Character>().playerName);
                    }
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.transform.root.CompareTag(enemyTag))
            {
                if (other.transform.root.gameObject.GetComponent<Character>())
                { 
                    if (PhotonNetwork.IsMasterClient)
                    {
                        EnBuff(other.transform.root.GetComponent<Character>().playerName);
                    }

                    else
                    {
                        photonView.RPC("RequestEnBuff", RpcTarget.AllBuffered, other.transform.root.GetComponent<Character>().playerName);
                    }
                }
            }
        }

        [PunRPC]
        public void RequestDeBuff(string playerName)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                DeBuff(playerName);
            }    
        }

        [PunRPC]
        public void RequestEnBuff(string playerName)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                EnBuff(playerName);
            }
        }

        void DeBuff(string playerName)
        {
            GameObject temp = GameObject.Find("Player_" + playerName);
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

        void EnBuff(string playerName)
        {
            Debug.Log("EnBuff");
            GameObject temp = GameObject.Find("Player_" + playerName);
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