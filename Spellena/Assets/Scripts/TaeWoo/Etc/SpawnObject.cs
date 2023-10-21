using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class SpawnObject : MonoBehaviourPunCallbacks
    {
        protected object[] data;
        public Vector3 direction;

        public virtual void Start()
        {
            Init();
        }

        void Init()
        {
            data = GetComponent<PhotonView>().InstantiationData;
            name = (string)data[0];
            tag = (string)data[1];
        }
    }
}