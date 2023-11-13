using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Player
{
    public class DimensionSlash : SpawnObject
    {
        public AeternaData aeternaData;
        public int chargeNum;

        [HideInInspector]
        public int damage;
        [HideInInspector]
        public float lifeTime;
        [HideInInspector]
        public float speed;
        [HideInInspector]
        public int healing;
        [HideInInspector]
        public bool isHealingSword = false;

        public override void OnEnable()
        {
            base.OnEnable();
            Init();     
        }

        void Init()
        {
            damage = aeternaData.DimenstionSlash_0_Damage;
            lifeTime = aeternaData.DimenstionSlash_0_lifeTime;
            speed = aeternaData.DimenstionSlash_0_Speed;

            name = playerName + "_" + objectName;
            type = SpawnObjectType.Projectile;

            if (data != null)
            {
                transform.rotation *= (Quaternion)data[3];

                if (data[4] != null)
                {
                    switch(chargeNum)
                    {
                        case 0:
                            healing = aeternaData.DimenstionSlash_0_Healing;
                            break;
                        case 1:
                            damage = aeternaData.DimenstionSlash_1_Damage;
                            lifeTime = aeternaData.DimenstionSlash_1_lifeTime;
                            speed = aeternaData.DimenstionSlash_1_Speed;
                            healing = aeternaData.DimenstionSlash_1_Healing;
                            break;
                        case 2:
                            damage = aeternaData.DimenstionSlash_2_Damage;
                            lifeTime = aeternaData.DimenstionSlash_2_lifeTime;
                            speed = aeternaData.DimenstionSlash_2_Speed;
                            healing = aeternaData.DimenstionSlash_2_Healing;
                            break;
                        case 3:
                            damage = aeternaData.DimenstionSlash_3_Damage;
                            lifeTime = aeternaData.DimenstionSlash_3_lifeTime;
                            speed = aeternaData.DimenstionSlash_3_Speed;
                            healing = aeternaData.DimenstionSlash_3_Healing;
                            break;
                    }


                    ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
                    Light[] lights = GetComponentsInChildren<Light>();

                    isHealingSword = (bool)data[4];

                    foreach (ParticleSystem particle in systems)
                    {
                        Color color;

                        if (isHealingSword)
                            ColorUtility.TryParseHtmlString("#19FF2B", out color);
                        else
                            ColorUtility.TryParseHtmlString("#E700C7", out color);

                        ParticleSystem.MainModule module = particle.main;
                        module.startColor = color;
                    }

                    foreach (Light light in lights)
                    {
                        Color color;

                        if (isHealingSword)
                            ColorUtility.TryParseHtmlString("#19FF2B", out color);
                        else
                            ColorUtility.TryParseHtmlString("#E700C7", out color);

                        light.color = color;
                    }

                }
            }

            StartCoroutine(Gone());
        }


        IEnumerator Gone()
        {
            yield return new WaitForSeconds(lifeTime);

            if(PhotonNetwork.IsMasterClient)
            {
                DestorySpawnObject();
            }

        }
        
        // Update is called once per frame
        public void Update()
        {
            Move();
        }

        private void Move()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (isHealingSword)
                {
                    if (CompareTag("TeamA") && other.transform.root.CompareTag("TeamA") ||
                        CompareTag("TeamB") && other.transform.root.CompareTag("TeamB"))
                    {

                        if (other.transform.root.GetComponent<Character>() && other.gameObject.layer == LayerMask.NameToLayer("Other"))
                        {
                            other.transform.root.GetComponent<Character>().PlayerDamaged(playerName, -healing);                    
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
                    if (CompareTag("TeamA") && other.transform.root.CompareTag("TeamB") ||
                        CompareTag("TeamB") && other.transform.root.CompareTag("TeamA"))
                    {
                        if (other.transform.root.GetComponent<Character>())
                        {
                            other.transform.root.GetComponent<Character>().PlayerDamaged(playerName, damage);
                            Debug.Log("ATTACK!!!!!");
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