using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class DimensionIO : Ability
    {
        private Aeterna Player;
        private Animator animator;
        private GameObject sword;

        [HideInInspector]
        public GameObject enemyProjectile;
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
                    OnDuration();
                    break;
                case 1:
                    OnHoldShoot();
                    break;
                case 2:
                    OnCooling();
                    break;

            }
        }

        private void Update()
        {

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

            enemyProjectile = sword.GetComponent<AeternaSword>().contactObject;
            sword.GetComponent<AeternaSword>().contactObject = null;
            enemyProjectile.layer = LayerMask.NameToLayer("Projectile" + Player.tag[4]); // 태그 이름 편하게 수정
            enemyProjectile.SetActive(false);

            StopCoroutine(Player.SkillTimer(2));

            Player.skill2Phase = 1;
            Player.playerActionDatas[(int)PlayerActionState.Skill2].isExecuting = false;

            Player.skillTimer[2] = Player.AeternaData.skill2HoldTime;
            StartCoroutine(Player.SkillTimer(2));
        }

        private void OnHoldShoot()
        {
            enemyProjectile.transform.position = Player.camera.transform.position;
            if (Player.camera != null)
            {
                enemyProjectile.GetComponent<Projectile>().direction = Player.camera.transform.localRotation * Vector3.forward;
            }
            enemyProjectile.SetActive(true);
            enemyProjectile = null;

            Player.skillButton = 0;

            Player.skillTimer[2] = Player.AeternaData.skill2CoolTime;
            StartCoroutine(Player.SkillTimer(2));
        }

        private void OnCooling()
        {

        }
    }
}