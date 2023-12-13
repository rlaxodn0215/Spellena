using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class AeternaSword : MonoBehaviourPunCallbacks
    {
        [HideInInspector]
        public object[] contactObjectData;

        [HideInInspector]
        public Aeterna player;

        public GameObject normalSword;

        public GameObject skill2BuffParticle;
        public GameObject skill2HitParticle;
        public GameObject skill3BuffParticle;
        public GameObject skill4AttackSword;
        public GameObject skill4HealingSword;
        public GameObject[] skill4OverChargeParticles;

        public int damage;
        public string enemyTag;

        private void Start()
        {
            player = transform.root.gameObject.GetComponent<Aeterna>();
        }

        [PunRPC]
        public void SetSwordTag()
        {
            if (CompareTag("TeamA"))
            {
                enemyTag = "TeamB";
            }

            else if (CompareTag("TeamB"))
            {
                enemyTag = "TeamA";
            }
        }

        [PunRPC]
        public void ActivateParticle(int skillNum, bool isActive)
        {
            if (skillNum == 2)
            {
                skill2BuffParticle.SetActive(isActive);

                if (contactObjectData != null)
                    skill2HitParticle.SetActive(!isActive);
            }

            else if (skillNum == 3)
            {
                skill3BuffParticle.SetActive(isActive);
            }

            else if (skillNum == 4)
            {
                for (int i = 0; i < 3; i++)
                {
                    skill4OverChargeParticles[i].SetActive(isActive);
                }
            }

            else
            {
                Debug.LogError("No SkillNum");
                return;
            }

        }

        [PunRPC]
        public void ActivateSkill4Sword(bool isHealingSword)
        {
            normalSword.SetActive(false);
            skill4HealingSword.SetActive(isHealingSword);
            skill4AttackSword.SetActive(!isHealingSword);

            for (int i = 0; i < 3; i++)
            {
                ParticleSystem[] systems = skill4OverChargeParticles[i].GetComponentsInChildren<ParticleSystem>(true);

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

        }

        [PunRPC]
        public void DisActivateSkill4Sword()
        {
            normalSword.SetActive(true);
            skill4HealingSword.SetActive(false);
            skill4AttackSword.SetActive(false);

        }
    }
}