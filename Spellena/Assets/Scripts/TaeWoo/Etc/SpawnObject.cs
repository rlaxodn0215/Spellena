using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class SpawnObject : MonoBehaviourPunCallbacks
    {
        public int ID;
        protected object[] data;
        public Vector3 direction;

        public virtual void Start()
        {
            Init();
        }

        void Init()
        {
            data = GetComponent<PhotonView>().InstantiationData;

            if (data != null)
            {
                ID = (int)data[0];
                tag = (string)data[1];
            }

            if(CompareTag("TeamA") || CompareTag("TeamB"))
            {
                gameObject.layer = LayerMask.NameToLayer("SpawnObject" + tag[4]); // tag[4] => A,B
            }

            gameObject.name = "Player_" + ID + "_Portal";
        }

        [PunRPC]
        public void RequestDestorySpawnObject()
        {
            if(PhotonNetwork.IsMasterClient)
                DestorySpawnObject(); 
        }

        public void DestorySpawnObject()
        {
            if(gameObject !=null)
                PhotonNetwork.Destroy(gameObject);
        }
    }
}