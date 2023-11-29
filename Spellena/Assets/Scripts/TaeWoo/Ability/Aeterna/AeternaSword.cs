using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class AeternaSword : MonoBehaviour
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
        public GameObject skill4OverChargeParticle;

        public int damage;
        private string enemyTag;
        private bool isHealSword;

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
            if(skillNum == 2)
            {
               skill2BuffParticle.SetActive(isActive);
               skill2HitParticle.SetActive(!isActive);
            }

            else if(skillNum == 3)
            {
                skill3BuffParticle.SetActive(isActive);
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
           isHealSword = isHealingSword;
        }

        [PunRPC]
        public void ActivateSkill4ChargeParticle(bool isActive)
        {
            skill4OverChargeParticle.SetActive(isActive);
            ChangeEffectColor(isHealSword);
        }

        void ChangeEffectColor(bool isHealingSword)
        {
            ParticleSystem[] systems = skill4OverChargeParticle.GetComponentsInChildren<ParticleSystem>(true);

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


        [PunRPC]
        public void DisActivateSkill4Sword()
        {
            normalSword.SetActive(true);
            skill4HealingSword.SetActive(false);
            skill4AttackSword.SetActive(false);
        }

        public void OnTriggerEnter(Collider other)
        {
            Debug.Log("Hit Collider");

            if (other.transform.root.CompareTag(enemyTag))
            {
                Debug.Log("Hit Enemy");

                if (player.playerActionDatas[(int)PlayerActionState.Skill2].isExecuting && player.skill2Phase == 1)
                {
                    if (other.transform.root.GetComponent<SpawnObject>())
                    {
                        if (other.transform.root.GetComponent<SpawnObject>().type == SpawnObjectType.Projectile)
                        {
                            contactObjectData = other.transform.root.GetComponent<SpawnObject>().data;
                            player.dimensionIO.CheckHold();
                            
                        }

                        if (PhotonNetwork.IsMasterClient)
                        {
                            other.transform.root.GetComponent<SpawnObject>().DestorySpawnObject();
                        }

                        else
                        {
                            other.transform.root.GetComponent<PhotonView>().RPC("DestorySpawnObject", RpcTarget.MasterClient);
                        }
                    }

                }

                else if (player.playerActionDatas[(int)PlayerActionState.Skill3].isExecuting && player.skill3Phase == 1)
                {
                    Debug.Log("When skill3");

                    if (other.transform.root.GetComponent<Character>())
                    {
                        Debug.Log("Do Skill3");
                        player.dimensionTransport.Transport(other.transform.root.gameObject);
                    }
                }

                else
                {
                    if (other.transform.root.GetComponent<Character>())
                        other.transform.root.GetComponent<Character>().PlayerDamaged(player.playerName, damage,null,new Vector3(0,0,0),0.0f);
                }
            }

        }
    }
}