using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

namespace Player
{
    public enum SpawnObjectType
    {
        FixedObject,
        Projectile,     
    }

    public class SpawnObject : MonoBehaviourPunCallbacks, IPunObservable
    {
        public string playerName;
        public SpawnObjectType type;
        public string objectName;
        public object[] data;
        public string[] hitParticles;
        
        public virtual void OnEnable()
        {
            Init();
        }
        

        void Init()
        {
            data = GetComponent<PhotonView>().InstantiationData;

            if (data != null)
            {
                playerName = (string)data[0];
                tag = (string)data[1];
                objectName = (string)data[2];
            }

        }

        [PunRPC]
        public void InstanitateObject(string address, Vector3 pos)
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Instantiate(address, pos, Quaternion.identity);        
        }

        [PunRPC]
        public void DestoryObject()
        {
            Destroy(gameObject.transform.root.gameObject);  
        }

        [PunRPC]
        public void DestoryObject(float delayTime)
        {
            Destroy(gameObject.transform.root.gameObject,delayTime);
        }

        [PunRPC]
        public void DestorySpawnObject()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject.transform.root.gameObject);
            }
        }

        [PunRPC]
        public void DestorySpawnObject(Vector3 hitPos, int index, bool isTransparent)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if(!isTransparent)
                    PhotonNetwork.Destroy(gameObject.transform.root.gameObject);

                PhotonNetwork.Instantiate(hitParticles[index], hitPos, Quaternion.identity);
            }
        }

        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) { }
    }
}