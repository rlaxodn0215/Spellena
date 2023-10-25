using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

namespace Player
{
    public enum SpawnObjectName
    {
        NoDamage,
        DimensionSlash,        
    }

    public class SpawnObject : MonoBehaviourPunCallbacks
    {
        public string playerName;
        public SpawnObjectName type;
        protected object[] data;

        public virtual void Start()
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
                type = (SpawnObjectName)data[2];
            }

        }
        public void DestorySpawnObject()
        {
            if(gameObject != null)
                PhotonNetwork.Destroy(gameObject);
        }
    }
}