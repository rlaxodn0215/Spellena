using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public class DimensionSlash : Projectile
    {
        public int Speed;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            StartCoroutine(Death(lifeTime));
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            Move();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

        }

        private void Move()
        {
            transform.Translate(transform.forward * Speed * Time.deltaTime);
        }

        IEnumerator Death(int lifetime)
        {
            yield return new WaitForSeconds(lifeTime);
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (teamTag=="TeamA" && other.tag == "TeamB" ||
                teamTag == "TeamB" && other.tag == "TeamA")
            {
                other.gameObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBuffered,Owner,damage);
                Debug.Log("검기 맞음");
                Destroy(gameObject);
            }

        }
    }
}