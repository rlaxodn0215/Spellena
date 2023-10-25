using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public class DimensionSword : Ability
    {
        private Aeterna Player;
        private Animator animator;
        private GameObject Sword;
        private GameObject dimensionSlash;
        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
            animator = player.GetComponent<Animator>();
            Sword = Player.DimensionSword;
            dimensionSlash = Player.DimensionSlash;
        }

        public override void Execution()
        {
            animator.SetTrigger("BasicAttack");
            Sword.GetComponent<BoxCollider>().enabled = true;
            StartCoroutine(EndAttack());
            StartCoroutine(ShootSlash());
        }

        IEnumerator ShootSlash()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(1).Length / 2.0f);

            if (PhotonNetwork.IsMasterClient)
            {
                SpawnSlash();
            }

            else
            {
                photonView.RPC("RequestSpawnSlash", RpcTarget.MasterClient);
            }
        }

        [PunRPC]
        public void RequestSpawnSlash()
        {
            SpawnSlash(); 
        }

        void SpawnSlash()
        {
            object[] data = new object[3];
            data[0] = Player.playerName;
            data[1] = gameObject.tag;
            data[2] = SpawnObjectName.DimensionSlash;
            data[3] = Player.camera.transform.localRotation;
            PhotonNetwork.Instantiate("TaeWoo/Prefabs/Effect/" + dimensionSlash.name,
                Player.camera.transform.position, Player.transform.localRotation, 0, data);
        }

        IEnumerator EndAttack()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(1).Length);
            Sword.GetComponent<BoxCollider>().enabled = false;
        }
    }
}