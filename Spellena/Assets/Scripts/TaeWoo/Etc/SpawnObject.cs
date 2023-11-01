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
        public  void DestorySpawnObject()
        {
            if (this !=null && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
        }
    }
}