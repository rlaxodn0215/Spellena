using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class DimensionTransport : Ability
    {
        private List<Transform> trasportPoints = new List<Transform>();
        private Aeterna Player;
        private Animator animator;
        private GameObject Sword;
        private GameObject swordTrigger;

        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
            animator = player.GetComponent<Animator>();
            Sword = Player.DimensionSword;
            swordTrigger = Player.swordTrigger;

            for(int i = 0; i < Player.teleportPoints.transform.childCount; i++)
            {
                trasportPoints.Add(Player.teleportPoints.transform.GetChild(i));
            }
        }

        public override void Execution()
        {
            Player.GetComponent<PhotonView>().RPC("BasicAttackTrigger", RpcTarget.AllBufferedViaServer);
            StartCoroutine(ActiveTirgger());
            StartCoroutine(EndAttack());
        }

        IEnumerator ActiveTirgger()
        {
            yield return new WaitForSeconds(0.8f);
            swordTrigger.GetComponent<BoxCollider>().enabled = true;
        }

        public override void IsDisActive()
        {
            if (Player.playerActionDatas[(int)PlayerActionState.Skill3].isExecuting && Player.skill3Phase == 1)
            {
                Sword.GetComponent<PhotonView>().RPC("ActivateParticle", RpcTarget.AllBuffered, 3, false);
                Player.dimensionSwordForMe[3].SetActive(false);
                Player.skillTimer[3] = 0.1f;
            }
        }

        IEnumerator EndAttack()
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(1).Length);
            swordTrigger.GetComponent<BoxCollider>().enabled = false;
        }

        public void Transport(GameObject enemy)
        {
            Player.soundManager.PlayAudio("SweepSound", 1.0f, false, false, "EffectSound");

            int randomIndex = Random.Range(0, trasportPoints.Count);

            Player.teleportManager.GetComponent<PhotonView>().RPC("UseTeleportManager",RpcTarget.AllBuffered, enemy.transform.position ,trasportPoints[randomIndex].position, enemy.GetComponent<PhotonView>().OwnerActorNr);
            enemy.GetComponent<PhotonView>().RPC("PlayerTeleport", RpcTarget.AllBuffered, trasportPoints[randomIndex].position);

            swordTrigger.GetComponent<BoxCollider>().enabled = false;

            Player.skillTimer[3] = Player.aeternaData.skill3CoolTime;
            Player.skill3Phase = 2;

            if (Sword.GetComponent<AeternaSword>().skill3BuffParticle)
            {
                Sword.GetComponent<PhotonView>().RPC("ActivateParticle", RpcTarget.AllBuffered, 3, false);
                Player.dimensionSwordForMe[3].SetActive(false);
            }
        }
    }
}
