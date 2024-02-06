using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameCenterTest0;

namespace Player
{
    public class ElementalOrderUI : MonoBehaviour
    {
        public ElementalOrder elementalOrder;

        [HideInInspector]
        public Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();
        [HideInInspector]
        public Dictionary<int, float> skillNumbers = new Dictionary<int, float>();

        Image hpBarImage;
        Text hpText;
        Image skill4ChargeBarImage;
        List<Text> skillCooltimes = new List<Text>();

        private void Start()
        {
            ConnectUI();
        }

        void ConnectUI()
        {
            UIObjects["hpBar"] = GameCenterTest.FindObject(gameObject, "HpBar");
            UIObjects["hpText"] = GameCenterTest.FindObject(gameObject, "HpText");

            for (int i = 1; i <= 6; i++)
            {
                UIObjects["skill_" + i + "_Active"] = GameCenterTest.FindObject(gameObject, "Skill_" + i + "_Active");
                UIObjects["skill_" + i + "_CoolTime"] = GameCenterTest.FindObject(gameObject, "Skill_" + i + "_CoolTime");

                skillCooltimes.Add(UIObjects["skill_" + i + "_CoolTime"].GetComponent<Text>());
            }

            for (int i = 1; i <= 6; i++)
            {
                UIObjects["skill_" + i + "_Lock"] = GameCenterTest.FindObject(gameObject, "Skill_" + i + "_Lock");
            }

            hpBarImage = UIObjects["hpBar"].GetComponent<Image>();
            hpText = UIObjects["hpText"].GetComponent<Text>();
        }

        private void Update()
        {
            ShowHp();
            ShowSkillActive();
            ShowCoolTime();
        }

        void ShowHp()
        {
            hpBarImage.fillAmount = (float)elementalOrder.hp / elementalOrder.dataHp;
            hpText.text = string.Format(elementalOrder.hp + " / " + elementalOrder.dataHp);
        }

        void ShowSkillActive()
        {
            switch (elementalOrder.skillState)
            {
                case ElementalOrder.SkillState.None:
                    SetActive(0);
                    break;
                case ElementalOrder.SkillState.RagnaEdge:
                    SetActive(1);
                    break;
                case ElementalOrder.SkillState.BurstFlare:
                    SetActive(2);
                    break;
                case ElementalOrder.SkillState.GaiaTied:
                    SetActive(3);
                    break;
                case ElementalOrder.SkillState.MeteorStrike:
                    SetActive(4);
                    break;
                case ElementalOrder.SkillState.TerraBreak:
                    SetActive(5);
                    break;
                case ElementalOrder.SkillState.EterialStorm:
                    SetActive(6);
                    break;

            }
        }

        void SetActive(int index)
        {
            for(int i = 1;  i <= 6; i++)
            {
                if(i == index)
                {
                    UIObjects["skill_" + i + "_Active"].SetActive(true);
                }

                else
                {
                    UIObjects["skill_" + i + "_Active"].SetActive(false);
                }
            }
        }

        void ShowCoolTime()
        {
            skillNumbers[1] = elementalOrder.ragnaEdgeCoolDownTime;
            skillNumbers[2] = elementalOrder.burstFlareCoolDownTime;
            skillNumbers[3] = elementalOrder.gaiaTiedCoolDownTime;
            skillNumbers[4] = elementalOrder.meteorStrikeCoolDownTime;
            skillNumbers[5] = elementalOrder.terraBreakCoolDownTime;
            skillNumbers[6] = elementalOrder.eterialStormCoolDownTime;

            foreach(var timer in skillNumbers)
            {
                if(timer.Value > 0.0f)
                {
                    skillCooltimes[timer.Key - 1].text = string.Format("{0:F1}", timer.Value);
                    UIObjects["skill_" + timer.Key + "_Lock"].SetActive(true);
                }

                else
                {
                    skillCooltimes[timer.Key - 1].text = "";
                    UIObjects["skill_" + timer.Key + "_Lock"].SetActive(false);
                }
            }
        }
    }
}