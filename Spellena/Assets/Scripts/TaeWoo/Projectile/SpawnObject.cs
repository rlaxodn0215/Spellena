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
        public GameObject hitParticle;
        
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

        public void DestorySpawnObject()
        {
            if (this != null && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject.transform.root.gameObject);
            }
        }

        public void DestorySpawnObject(Vector3 hitPos)
        {
            if (this !=null && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject.transform.root.gameObject);
                PhotonNetwork.Instantiate("TaeWoo/Prefabs/Effect/DimensionSlash_0_Hit", hitPos, Quaternion.identity);
            }
        }

        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) { }
    }
}