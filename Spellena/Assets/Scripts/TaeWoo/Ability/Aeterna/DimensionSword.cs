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
        //private GameObject dimensionSlash;
        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
            animator = player.GetComponent<Animator>();
            Sword = Player.DimensionSword;
            //dimensionSlash = Player.DimensionSlash;
        }

        public override void Execution()
        {
            Player.GetComponent<PhotonView>().RPC("BasicAttackTrigger", RpcTarget.AllBufferedViaServer);
            Sword.GetComponent<BoxCollider>().enabled = true;
            StartCoroutine(EndAttack());
            StartCoroutine(ShootSlash());
        }

        IEnumerator ShootSlash()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(1).Length / 2.0f);

            if (PhotonNetwork.IsMasterClient)
            {
                SpawnSlash(Player.camera.transform.localRotation);
            }

            else
            {
                photonView.RPC("SpawnSlash", RpcTarget.MasterClient, Player.camera.transform.localRotation);
            }

            Player.soundManager.PlayRandomAudioOverlap("SlashSound", 1.0f, false, false, 0, 1);
        }

        [PunRPC]
        public void SpawnSlash(Quaternion rot)
        {
            object[] data = new object[5];
            data[0] = Player.playerName;
            data[1] = gameObject.tag;
            data[2] = "DimensionSlash_0";
            data[3] = rot;

             PhotonNetwork.Instantiate("Projectiles/" + (string)data[2],
                Player.camera.transform.position, Player.transform.localRotation, 0, data);
        }

        IEnumerator EndAttack()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(1).Length);
            Sword.GetComponent<BoxCollider>().enabled = false;
        }
    }
}