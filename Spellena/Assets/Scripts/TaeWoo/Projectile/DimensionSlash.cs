using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public class DimensionSlash : Projectile
    {
        [HideInInspector]
        public Aeterna Owner;

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
            transform.Translate(Owner.camera.transform.localRotation * Vector3.forward * Speed * Time.deltaTime);
        }

        IEnumerator Death(int lifetime)
        {
            yield return new WaitForSeconds(lifeTime);
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Owner.tag=="TeamA" && other.tag == "TeamB" ||
                Owner.tag == "TeamB" && other.tag == "TeamA")
            {
                other.gameObject.GetComponent<PhotonView>().RPC("PlayerDamaged", RpcTarget.AllBuffered,Owner,damage);
                Debug.Log("검기 맞음");
                Destroy(gameObject);
            }

        }
    }
}