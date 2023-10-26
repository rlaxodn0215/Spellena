using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public class DimensionSlash : SpawnObject
    {
        public Vector3 direction;
        public int damage;
        public int lifeTime;
        public int Speed;

        public override void Start()
        {
            base.Start();

            name = playerName + "_" + objectName;
            type = SpawnObjectType.Projectile;

            if (data !=null)
                direction = (Quaternion)data[3]*Vector3.forward;
            StartCoroutine(Gone());
        }


        IEnumerator Gone()
        {
            yield return new WaitForSeconds(lifeTime);

            if(PhotonNetwork.IsMasterClient)
            {
                DestorySpawnObject();
            }

        }
        
        // Update is called once per frame
        public void Update()
        {
            Move();
        }

        private void Move()
        {
            transform.Translate(direction * Speed * Time.deltaTime);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (CompareTag("TeamA") && other.transform.root.CompareTag("TeamB") ||
                    CompareTag("TeamB") && other.transform.root.CompareTag("TeamA"))
                {
                    if (other.transform.root.GetComponent<Character>())
                    {
                        other.transform.root.GetComponent<Character>().PlayerDamaged(playerName, damage);
                    }

                    DestorySpawnObject();
                }

                else if (other.transform.root.CompareTag("Ground"))
                {
                    DestorySpawnObject();
                }

            }
        }
    }
}