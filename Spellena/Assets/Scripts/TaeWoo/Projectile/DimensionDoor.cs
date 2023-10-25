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

        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(enemyTag))
            {
                if(other.gameObject.GetComponent<Character>())
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        DeBuff(other.transform.root.GetComponent<Character>().playerName);
                    }
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(enemyTag))
            {
                if (other.gameObject.GetComponent<Character>())
                { 
                    if (PhotonNetwork.IsMasterClient)
                    {
                        EnBuff(other.transform.root.GetComponent<Character>().playerName);
                    }
                }
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