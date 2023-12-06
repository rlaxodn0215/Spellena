using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class DimensionDoor : SpawnObject
    {
        public AeternaData aeternaData;
        public GameObject soundManger;
        private float InnerForce;

        private string enemyTag;
        private List<string> playerInArea;
        private List<Rigidbody> playerInAreaObject = new List<Rigidbody>();

        private PhotonView soundView;

        public override void OnEnable()
        {
            base.OnEnable();

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
            InnerForce = aeternaData.skill1InnerForce;

            soundView = soundManger.GetComponent<PhotonView>();

            soundManger.GetComponent<SoundManager>().InitAudioSettings();
            soundManger.GetComponent<SoundManager>().PlayAudio("Idle", 1.0f, true, false);

            StartCoroutine(Gone());
        }

        IEnumerator Gone()
        {
            yield return new WaitForSeconds(aeternaData.skill1Time);

            foreach (string _player in playerInArea)
                photonView.RPC("EnBuffWhenDestory", RpcTarget.AllBuffered, _player);

            if (PhotonNetwork.IsMasterClient)
            {
                DestorySpawnObject();
                InstanitateObject("SpawnObjects/PortalDestorySound", transform.position);
            }

            else
            {
                photonView.RPC("DestorySpawnObject", RpcTarget.MasterClient); 
                photonView.RPC("InstanitateObject", RpcTarget.MasterClient, "SpawnObjects/PortalDestorySound", transform.position); 
            }

        }

        private void FixedUpdate()
        {
            GiveGravity();
        }

        void GiveGravity()
        {
            Vector3 direction;

            if (playerInAreaObject == null) return;
            foreach(Rigidbody player in playerInAreaObject)
            {
                direction = transform.position - player.transform.position;
                direction.Normalize();
                direction *= InnerForce;
                player.MovePosition(player.transform.position + direction*Time.deltaTime);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                if (other.transform.root.CompareTag(enemyTag))
                {
                    if (other.transform.root.GetComponent<Character>())
                    {
                       photonView.RPC("DeBuff", RpcTarget.AllBuffered, other.transform.root.GetComponent<Character>().playerName);  
                    }

                    else if (other.transform.root.GetComponent<SpawnObject>() && type == SpawnObjectType.Projectile)
                    {
                        other.transform.root.GetComponent<PhotonView>().RPC("DestoryObject", RpcTarget.AllBuffered);
                        soundManger.GetComponent<PhotonView>().RPC("PlayAudioOverlap", RpcTarget.AllBuffered, "Absorb", 1.0f, false, false);
                        Debug.Log("Play Absorb Audio");
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
            GameObject temp = GameObject.Find(playerName);
            if (temp == null) return;

            foreach(string player in playerInArea)
            {
                if (player == temp.name) return;
            }

            playerInArea.Add(temp.name);
            playerInAreaObject.Add(temp.GetComponent<Rigidbody>());
        }

        [PunRPC]
        void EnBuff(string playerName)
        {
            GameObject temp = GameObject.Find(playerName);
            if (temp == null) return;

            foreach (string player in playerInArea)
            {
                if (player == temp.name)
                {

                    playerInArea.Remove(player);
                    playerInAreaObject.Remove(temp.GetComponent<Rigidbody>());

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
            }
            
        }

    }
}