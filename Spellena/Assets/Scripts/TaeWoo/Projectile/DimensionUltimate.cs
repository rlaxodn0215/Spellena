using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class DimensionUltimate : DimensionSlash
    {
        public int healing;
        public bool isHealingSword = false;
        public override void Start()
        {
            base.Start();

            if (data[4] != null)
            {
                ParticleSystem[] system = GetComponentsInChildren<ParticleSystem>();
                isHealingSword = (bool)data[4];

                foreach(ParticleSystem particle in system)
                {
                    Color color;

                    if (isHealingSword)
                        ColorUtility.TryParseHtmlString("#19FF2B", out color);
                    else
                        ColorUtility.TryParseHtmlString("#E700C7", out color);

                    ParticleSystem.MainModule module = particle.main;
                    module.startColor = color;
                }

            }

            
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if(isHealingSword)
                {
                    if (CompareTag("TeamA") && other.CompareTag("TeamA") || CompareTag("TeamB") && other.CompareTag("TeamB"))
                    {
                        if (other.GetComponent<Character>() && other.gameObject.layer == LayerMask.NameToLayer("Other"))
                        {
                            other.gameObject.GetComponent<Character>().PlayerDamaged(playerName, -healing);
                            DestorySpawnObject();
                        }
                    }

                    else if (other.CompareTag("Ground"))
                    {
                        DestorySpawnObject();
                    }

                }

                else
                {
                    if (CompareTag("TeamA") && other.CompareTag("TeamB") || CompareTag("TeamB") && other.CompareTag("TeamA"))
                    {
                        if (other.GetComponent<Character>())
                        {
                            other.gameObject.GetComponent<Character>().PlayerDamaged(playerName, damage);
                            DestorySpawnObject();
                        }
                    }

                    else if (other.CompareTag("Ground"))
                    {
                        DestorySpawnObject();
                    }
                }

            }
        }
    }
}