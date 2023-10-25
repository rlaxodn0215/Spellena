using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class DimensionTransport : Ability
    {
        public GameObject parentPoint;
        private List<Transform> trasportPoints;
        private Aeterna Player;
        private Animator animator;
        private GameObject Sword;

        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
            animator = player.GetComponent<Animator>();
            Sword = Player.DimensionSword;

            if (parentPoint == null) return;

            for(int i = 0; i < parentPoint.transform.childCount; i++)
            {
                trasportPoints.Add(parentPoint.transform.GetChild(i));
            }
        }

        public override void Execution()
        {
            animator.SetTrigger("BasicAttack");
            Sword.GetComponent<BoxCollider>().enabled = true;
            StartCoroutine(EndAttack());
        }

        IEnumerator EndAttack()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(1).Length);
            Sword.GetComponent<BoxCollider>().enabled = false;
        }
    }
}
