using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class DimensionIO : Ability
    {
        private Aeterna Player;
        private Animator animator;
        private GameObject sword;

        [HideInInspector]
        public SpawnObjectName enemyProjectileName;
        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
            animator = Player.GetComponent<Animator>();
            sword = Player.DimensionSword;
        }

        public override void Execution(ref int phase)
        {
            switch (phase)
            {
                case 0:
                    break;
                case 1:
                    OnDuration();
                    break;
                case 2:
                    OnHoldShoot();
                    break;
                case 3:
                    OnCooling();
                    break;

            }
        }

        private void OnDuration()
        {
            animator.SetTrigger("BasicAttack");
            sword.GetComponent<BoxCollider>().enabled = true;
            StartCoroutine(EndAttack());
        }

        IEnumerator EndAttack()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(1).Length);
            sword.GetComponent<BoxCollider>().enabled = false;
        }

        public void CheckHold()
        {
            sword.GetComponent<BoxCollider>().enabled = false;

            enemyProjectileName = sword.GetComponent<AeternaSword>().contactObjectName;

            StopCoroutine(Player.SkillTimer(2));

            Player.skill2Phase = 2;
            Player.playerActionDatas[(int)PlayerActionState.Skill2].isExecuting = false;

            Player.skillTimer[2] = Player.AeternaData.skill2HoldTime;
            StartCoroutine(Player.SkillTimer(2));
        }

        private void OnHoldShoot()
        {
            if(PhotonNetwork.IsMasterClient)
            {
                RequestShootProjectile();
            }

            else
            {
                photonView.RPC("RequestShootProjectile", RpcTarget.AllBuffered);
            }

            Player.skillTimer[2] = Player.AeternaData.skill2CoolTime;
            StartCoroutine(Player.SkillTimer(2));
        }

        public void RequestShootProjectile()
        {
            if(PhotonNetwork.IsMasterClient)
                ShootProjectile();
        }

        [PunRPC]
        public void ShootProjectile()
        {
            object[] data = new object[4];
            data[0] = Player.playerName;
            data[1] = gameObject.tag;
            data[2] = enemyProjectileName;
            data[3] = Player.camera.transform.localRotation;

            PhotonNetwork.Instantiate("TaeWoo/Prefabs/Effect/" + enemyProjectileName,
                Player.camera.transform.position, Player.transform.localRotation, 0, data);
        }

        private void OnCooling()
        {

        }
    }
}