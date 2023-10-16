using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{

    public class HitCollision : MonoBehaviour
    {
        [HideInInspector]
        public GameObject TopParent;

        void Start()
        {
            TopParent = transform.root.gameObject;
        }

        private void OnTriggerEnter(Collider other)
        {
            //TopParent.GetComponent<Charactor>().TriggerEnter(other);
        }
        private void OnTriggerStay(Collider other)
        {
            //TopParent.GetComponent<Charactor>().TriggerStay(other);
        }
        private void OnTriggerExit(Collider other)
        {
            //TopParent.GetComponent<Charactor>().TriggerExit(other);
        }
        private void OnCollisionEnter(Collision collision)
        {
            //TopParent.GetComponent<Charactor>().CollisionEnter(collision);
        }
        private void OnCollisionStay(Collision collision)
        {
            //TopParent.GetComponent<Charactor>().CollisionStay(collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            //TopParent.GetComponent<Charactor>().CollisionExit(collision);
        }

    } }
