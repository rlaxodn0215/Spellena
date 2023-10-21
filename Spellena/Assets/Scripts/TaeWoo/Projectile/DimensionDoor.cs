using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class DimensionDoor : SpawnObject
    {
        public Aeterna owner;
        public int lifeTime;
        public int range;
        public int deBuffNum;

        private LayerMask layerMask;

        IEnumerator Start()
        {
            //ID = owner.GetComponent<PhotonView>().ViewID;
            object[] data = GetComponent<PhotonView>().InstantiationData;
            name = (string)data[0];

            layerMask = ((1 << LayerMask.NameToLayer("Me")) |
                        (1 << LayerMask.NameToLayer("Other")));
            layerMask = ~layerMask;

            yield return new WaitForSeconds(lifeTime);

            //transform.parent = owner.transform;
            //gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            Collider[] objs = Physics.OverlapSphere(transform.position, range,layerMask);

            if (objs == null || owner == null) return;
            
            if (owner.CompareTag("TeamA"))
            {
                foreach(Collider obj in objs)
                {
                   if(obj.CompareTag("TeamB"))
                   {
                        obj.GetComponent<PhotonView>().RPC("Disappear", RpcTarget.AllBuffered);
                   }

                   if(obj.gameObject.layer == LayerMask.NameToLayer("ProjectileB"))
                   {
                        obj.GetComponent<PhotonView>().RPC("SlowDown", RpcTarget.AllBuffered,deBuffNum);
                   }
                }

            }

            else if (owner.CompareTag("TeamB"))
            {
                foreach (Collider obj in objs)
                {
                    if (obj.CompareTag("TeamA"))
                    {
                        obj.GetComponent<PhotonView>().RPC("Disappear", RpcTarget.AllBuffered);
                    }

                    if (obj.gameObject.layer == LayerMask.NameToLayer("ProjectileA"))
                    {
                        obj.GetComponent<PhotonView>().RPC("SlowDown", RpcTarget.AllBuffered,deBuffNum);
                    }
                }
            }
            
        }

    }
}