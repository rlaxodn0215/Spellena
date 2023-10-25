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
        public SpawnObjectType enemyProjectileType;
        [HideInInspector]
        public string enemyObjectName;

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
                case 1:
                    OnDuration();
                    break;
                case 2:
                    OnHoldShoot();
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
            Player.DimensionSword.GetComponent<AeternaSword>().skill2BuffParticle.SetActive(false);

            enemyObjectName = sword.GetComponent<AeternaSword>().contactObjectName;

            Player.skill2Phase = 2;
            Player.playerActionDatas[(int)PlayerActionState.Skill2].isExecuting = false;

            Player.skillTimer[2] = Player.AeternaData.skill2HoldTime;
            Player.skillButton = 0;
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

            Player.skill2Phase = 3;
            Player.skillTimer[2] = Player.AeternaData.skill2CoolTime;
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
            data[2] = enemyObjectName;
            data[3] = Player.camera.transform.localRotation;

            PhotonNetwork.Instantiate("TaeWoo/Prefabs/Effect/" + enemyObjectName,
                Player.camera.transform.position, Player.transform.localRotation, 0, data);
        }
    }
}