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
                    OnDuration(ref phase);
                    break;
                case 1:
                    OnHold();
                    break;
                case 2:
                    OnCooling();
                    break;

            }
        }

        private void OnDuration(ref int phase)
        {
            animator.SetTrigger("BasicAttack");

            if (enemyProjectile == null)
            {
                sword.GetComponent<BoxCollider>().enabled = true;
                enemyProjectile = sword.GetComponent<AeternaSword>().contactObject;
                enemyProjectile.layer = LayerMask.NameToLayer("Projectile" + Player.tag[4]);
                enemyProjectile.SetActive(false);
            }   

        }

        private void OnHold()
        {

        }

        private void OnCooling()
        {

        }

        //IEnumerator DurationTimer(float time)
        //{
        //    sword.GetComponent<BoxCollider>().enabled = true;
        //    timerForShow = Player.AeternaData.skill2DurationTime;

        //    while (timerForShow > 0.0f)
        //    {
        //        if (enemyProjectile == null)
        //        {
        //            enemyProjectile = sword.GetComponent<AeternaSword>().contactObject;
        //        }

        //        timerForShow -= Time.deltaTime;
        //        yield return null;
        //    }

        //    sword.GetComponent<AeternaSword>().contactObject = null;
        //    sword.GetComponent<BoxCollider>().enabled = false;

        //    if(enemyProjectile)
        //    {
        //        enemyProjectile.layer = LayerMask.NameToLayer("Projectile" + Player.tag[4]);
        //        StartCoroutine(CoolTimer(Player.AeternaData.skill2HoldTime));
        //    }

        //    else
        //    {
        //        StartCoroutine(CoolTimer(Player.AeternaData.skill2CoolTime));
        //    }

        //}

        //IEnumerator HoldTimer(float time)
        //{
        //    timerForShow = Player.AeternaData.skill2HoldTime;

        //    while (timerForShow > 0.0f)
        //    {






        //        timerForShow -= Time.deltaTime;
        //        yield return null;
        //    }
        //}

        //IEnumerator CoolTimer(float time)
        //{
        //    timerForShow = Player.AeternaData.skill2CoolTime;
        //    bool finish = false;
        //    while (timerForShow > 0.0f)
        //    {
        //        timerForShow -= Time.deltaTime;
        //        yield return null;
        //    }
        //    yield return finish;
        //}

        private void Update()
        {
            
        }
    }
}