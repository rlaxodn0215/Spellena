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
            name = playerName + "_DimensionSlash";
            if(data !=null)
                direction = (Quaternion)data[2]*Vector3.forward;
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

        private void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if(CompareTag("TeamA") && other.CompareTag("TeamB") || CompareTag("TeamB") && other.CompareTag("TeamA"))
                {
                    if(other.GetComponent<Character>())
                        other.gameObject.GetComponent<Character>().PlayerDamaged(playerName,damage);
                }

                DestorySpawnObject();
            }
        }
    }
}