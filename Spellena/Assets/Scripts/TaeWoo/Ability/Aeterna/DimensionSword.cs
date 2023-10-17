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
            Debug.Log("DimensionSword!");
        }

        IEnumerator ShootSlash()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(1).Length/2);
            dimensionSlash.GetComponent<DimensionSlash>().owner = Player;
            PhotonNetwork.Instantiate("TaeWoo/Prefabs/Effect/" + dimensionSlash.name, Player.camera.transform.position, Player.transform.localRotation);
            Debug.Log("검기 소환");
        }

        IEnumerator EndAttack()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(1).Length);
            Sword.GetComponent<BoxCollider>().enabled = false;
            Debug.Log("DimensionSword finish");
        }
    }
}