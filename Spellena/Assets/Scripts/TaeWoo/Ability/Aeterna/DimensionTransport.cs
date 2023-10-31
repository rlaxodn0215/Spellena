using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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

        public override void IsDisActive()
        {
            if (Player.playerActionDatas[(int)PlayerActionState.Skill3].isExecuting && Player.skill3Phase == 1)
            {
                Sword.GetComponent<PhotonView>().RPC("ActivateParticle", RpcTarget.AllBuffered, 3, false);
                Player.skillTimer[3] = 0.1f;
            }
        }

        IEnumerator EndAttack()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(1).Length);
            Sword.GetComponent<BoxCollider>().enabled = false;
        }

        public void Transport(GameObject enemy)
        {
            int randomIndex = Random.Range(0, trasportPoints.Count);
            enemy.transform.position = trasportPoints[randomIndex].position;
            Sword.GetComponent<BoxCollider>().enabled = false;
            Player.skillTimer[3] = Player.aeternaData.skill3CoolTime;
            Player.skill3Phase = 2;

            if(Sword.GetComponent<AeternaSword>().skill3BuffParticle)
                Sword.GetComponent<AeternaSword>().skill3BuffParticle.SetActive(false);
        }
    }
}
