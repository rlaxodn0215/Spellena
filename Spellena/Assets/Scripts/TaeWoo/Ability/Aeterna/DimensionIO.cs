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

        private int skill2Phase = 0; //0 : none, 1: duration, 2: hold, 3: cool


        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
            animator = Player.GetComponent<Animator>();
            sword = Player.DimensionSword;
        }

        public override void Execution()
        {
            //Debug.Log("DimensionIO");

            switch (skill2Phase)
            {
                case 0:
                    animator.SetTrigger("BasicAttack");

                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;

            }


        }

        IEnumerator DurationTimer(float time)
        {
            sword.GetComponent<BoxCollider>().enabled = true;
            timerForShow = Player.AeternaData.skill2DurationTime;

            while (timerForShow > 0.0f)
            {
                if (enemyProjectile == null)
                {
                    enemyProjectile = sword.GetComponent<AeternaSword>().contactObject;
                }

                timerForShow -= Time.deltaTime;
                yield return null;
            }

            sword.GetComponent<AeternaSword>().contactObject = null;
            sword.GetComponent<BoxCollider>().enabled = false;

            if(enemyProjectile)
            {
                enemyProjectile.layer = LayerMask.NameToLayer("Projectile" + Player.tag[4]);
                StartCoroutine(CoolTimer(Player.AeternaData.skill2HoldTime));
            }

            else
            {
                StartCoroutine(CoolTimer(Player.AeternaData.skill2CoolTime));
            }

        }

        IEnumerator HoldTimer(float time)
        {
            timerForShow = Player.AeternaData.skill2HoldTime;

            while (timerForShow > 0.0f)
            {






                timerForShow -= Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator CoolTimer(float time)
        {
            timerForShow = Player.AeternaData.skill2CoolTime;
            bool finish = false;
            while (timerForShow > 0.0f)
            {
                timerForShow -= Time.deltaTime;
                yield return null;
            }
            yield return finish;
        }

        private void Update()
        {
            
        }
    }
}