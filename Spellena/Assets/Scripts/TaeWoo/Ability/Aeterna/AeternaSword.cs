using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{

    public class AeternaSword : MonoBehaviour
    {
        [HideInInspector]
        public GameObject contactObject;
        [HideInInspector]
        public Aeterna player;

        private LayerMask layerMask;
        private void Start()
        {
            player = transform.root.gameObject.GetComponent<Aeterna>();

            if (CompareTag("TeamA"))
            {
                layerMask = 1 << LayerMask.NameToLayer("ProjectileB");
            }

            else if (CompareTag("TeamB"))
            {
                layerMask = 1 << LayerMask.NameToLayer("ProjectileA");
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (other.gameObject.layer == layerMask)
                {
                    contactObject = other.gameObject;
                    player.dimensionIO.CheckHold();
                    Debug.Log("적 투사체 충돌");
                }
            }
        }
    }
}