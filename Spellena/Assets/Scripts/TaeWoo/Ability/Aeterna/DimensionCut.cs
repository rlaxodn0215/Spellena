using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class DimensionCut : Ability
    {
        [HideInInspector]
        public bool isHealingSword = true;
        private Aeterna Player;
        private AeternaSword Sword;

        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
            Sword = Player.DimensionSword.GetComponent<AeternaSword>();
        }

        public override void IsActive()
        {
            Sword.GetComponent<PhotonView>().RPC("ActivateSkill4Sword", RpcTarget.AllBuffered, isHealingSword);

            Player.dimensionSwordForMe[1].SetActive(false);
            Player.dimensionSwordForMe[4].SetActive(isHealingSword);
            Player.dimensionSwordForMe[5].SetActive(!isHealingSword);

            for (int i = 0; i < 3; i++)
            {
                ParticleSystem[] systems = Player.dimensionSwordForMe[i + 7].GetComponentsInChildren<ParticleSystem>(true);

                foreach (ParticleSystem particle in systems)
                {
                    Color color;

                    if (isHealingSword)
                        ColorUtility.TryParseHtmlString("#FFFFFF", out color);
                    else
                        ColorUtility.TryParseHtmlString("#912AFF", out color);

                    ParticleSystem.MainModule module = particle.main;
                    module.startColor = color;
                }
            }


            isHealingSword = !isHealingSword;
            Player.soundManager.PlayAudioOverlap("GrandSwordSound", 1.0f, false, false,"EffectSound");
        }

        public override void IsDisActive()
        {
            Sword.GetComponent<PhotonView>().RPC("DisActivateSkill4Sword", RpcTarget.AllBuffered);

            Player.dimensionSwordForMe[1].SetActive(true);
            Player.dimensionSwordForMe[4].SetActive(false);
            Player.dimensionSwordForMe[5].SetActive(false);
        }

        public override void Execution(ref int chargeCount)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SpawnDimensionCut(chargeCount, Player.camera.transform.localRotation, !isHealingSword);
            }
            else
            {
                photonView.RPC("SpawnDimensionCut", RpcTarget.MasterClient, chargeCount, Player.camera.transform.localRotation, !isHealingSword);
            }

            Player.soundManager.PlayAudioOverlap("GrandSlashSound", 1.0f, false, false, "EffectSound");
        }

        [PunRPC]
        void SpawnDimensionCut(int charge, Quaternion rot, bool ishealing)
        {
            object[] data = new object[5];
            data[0] = Player.playerName;
            data[1] = gameObject.tag;
            data[2] = "DimensionSlash_" + charge;
            data[3] = rot;
            data[4] = ishealing;
            PhotonNetwork.Instantiate("Projectiles/" + (string)data[2],
                    Player.camera.transform.position, Player.transform.localRotation, 0, data);
        }
    }
}