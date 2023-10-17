using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class DimensionDoor : MonoBehaviour
    {
        public int lifeTime;
        public int range;
        public int deBuffNum;

        [HideInInspector]
        public Aeterna owner;

        private LayerMask layerMask;

        IEnumerator Start()
        {
            layerMask = ((1 << LayerMask.NameToLayer("Me")) |
                        (1 << LayerMask.NameToLayer("Other")));
            layerMask = ~layerMask;

            yield return new WaitForSeconds(lifeTime);
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            Collider[] objs = Physics.OverlapSphere(transform.position, range,layerMask);

            if (objs != null)
            {
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
}