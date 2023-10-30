using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class DimensionDoor : SpawnObject
    {
        public AeternaData aeternaData;
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
            GetComponent<SphereCollider>().radius = aeternaData.skill1DoorRange;

            StartCoroutine(Gone());
        }

        IEnumerator Gone()
        {
            yield return new WaitForSeconds(aeternaData.skill1Time);

            if (PhotonNetwork.IsMasterClient)
            {
                foreach (string _player in playerInArea)
                    EnBuffWhenDestory(_player);
                DestorySpawnObject();
            }

            else
            {
                foreach (string _player in playerInArea)
                    photonView.RPC("EnBuffWhenDestory", RpcTarget.AllBuffered, _player);
                photonView.RPC("DestorySpawnObject", RpcTarget.MasterClient);
            }

        }

        

        public void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.CompareTag(enemyTag))
            {
                if(other.transform.root.GetComponent<Character>())
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        DeBuff(other.transform.root.GetComponent<Character>().playerName);
                    }

                    else
                    {
                        photonView.RPC("DeBuff", RpcTarget.AllBuffered, other.transform.root.GetComponent<Character>().playerName);
                    }
                }

                else if(other.transform.root.GetComponent<SpawnObject>())
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        other.transform.root.GetComponent<SpawnObject>().DestorySpawnObject();
                    }

                    else
                    {
                        other.transform.root.GetComponent<PhotonView>().RPC("DestorySpawnObject", RpcTarget.MasterClient);
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
                        photonView.RPC("EnBuff", RpcTarget.AllBuffered, other.transform.root.GetComponent<Character>().playerName);
                    }
                }
            }
        }

        [PunRPC]
        void DeBuff(string playerName)
        {
            GameObject temp = GameObject.Find("Player_" + playerName);
            if (temp == null) return;

            foreach(string player in playerInArea)
            {
                if (player == temp.name) return;
            }

            playerInArea.Add(temp.name);

            temp.GetComponent<Character>().sitSpeed *= aeternaData.skill1DeBuffRatio;
            temp.GetComponent<Character>().walkSpeed *= aeternaData.skill1DeBuffRatio;
            temp.GetComponent<Character>().runSpeed *= aeternaData.skill1DeBuffRatio;
            temp.GetComponent<Character>().jumpHeight *= aeternaData.skill1DeBuffRatio;
        }

        [PunRPC]
        void EnBuff(string playerName)
        {
            GameObject temp = GameObject.Find("Player_" + playerName);
            if (temp == null) return;

            foreach (string player in playerInArea)
            {
                if (player == temp.name)
                {
                    temp.GetComponent<Character>().sitSpeed = temp.GetComponent<Character>().dataSitSpeed;
                    temp.GetComponent<Character>().walkSpeed = temp.GetComponent<Character>().dataWalkSpeed;
                    temp.GetComponent<Character>().runSpeed = temp.GetComponent<Character>().dataRunSpeed;
                    temp.GetComponent<Character>().jumpHeight = temp.GetComponent<Character>().dataJumpHeight;

                    playerInArea.Remove(player);
                    if (playerInArea.Count == 0) return;
                }
            }
        }

        [PunRPC]
        void EnBuffWhenDestory(string player)
        {
            GameObject temp = GameObject.Find(player);

            if (temp != null)
            {
                Debug.Log("EnBuffWhenDestory");
                temp.GetComponent<Character>().sitSpeed = temp.GetComponent<Character>().dataSitSpeed;
                temp.GetComponent<Character>().walkSpeed = temp.GetComponent<Character>().dataWalkSpeed;
                temp.GetComponent<Character>().runSpeed = temp.GetComponent<Character>().dataRunSpeed;
                temp.GetComponent<Character>().jumpHeight = temp.GetComponent<Character>().dataJumpHeight;
            }
            
        }

    }
}