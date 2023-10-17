using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class DimensionDoor : MonoBehaviour
    {
        public int lifeTime;
        public int range;

        [HideInInspector]
        public Aeterna owner;

        private LayerMask layer;

        IEnumerator Start()
        {
            if(owner != null)
                layer = owner.gameObject.layer;

            yield return new WaitForSeconds(lifeTime);
            Destroy(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            if (layer == LayerMask.NameToLayer("ProjectileA"))
            {
                Collider[] obj = Physics.OverlapSphere(transform.position, range, LayerMask.NameToLayer("ProjectileB"));

            }

            if (layer == LayerMask.NameToLayer("ProjectileB"))
            {
                Collider[] obj = Physics.OverlapSphere(transform.position, range, LayerMask.NameToLayer("ProjectileA"));
            }

        }


    }
}