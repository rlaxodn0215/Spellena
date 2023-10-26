using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class DimensionCut : Ability
    {
        private bool isHealingSword = true;
        private Aeterna Player;
        private AeternaSword Sword;

        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
            Sword = Player.DimensionSword.GetComponent<AeternaSword>();
        }

        public override void IsActive()
        {
            //if (isHealingSword)
            //{
            //    Debug.Log("Skill4_HealingSword Ready");  
            //}

            //else
            //{
            //    Debug.Log("Skill4_DamageSword Ready");
            //}

            Sword.skill4HealingSword.SetActive(isHealingSword);
            Sword.skill4AttackSword.SetActive(!isHealingSword);
            Sword.normalSword.SetActive(false);

            isHealingSword = !isHealingSword;
        }

        public override void IsDisActive()
        {
            Sword.skill4HealingSword.SetActive(false);
            Sword.skill4AttackSword.SetActive(false);
            Sword.normalSword.SetActive(true);
        }

        public override void Execution(ref int chargeCount)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SpawnDimensionCut(chargeCount);
            }
            else
            {
                photonView.RPC("RequestSpawnDimensionCut", RpcTarget.MasterClient, chargeCount);
            }
        }

        [PunRPC]
        public void RequestSpawnDimensionCut(int charge)
        {
            SpawnDimensionCut(charge);
        }

        void SpawnDimensionCut(int charge)
        {
            object[] data = new object[5];
            data[0] = Player.playerName;
            data[1] = gameObject.tag;
            data[2] = "DimensionSlash_" + charge;
            data[3] = Player.camera.transform.localRotation;
            data[4] = !isHealingSword;

            PhotonNetwork.Instantiate("TaeWoo/Prefabs/Effect/" + (string)data[2],
                Player.camera.transform.position, Player.transform.localRotation, 0, data);
        }
    }
}