using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class AeternaUI : MonoBehaviour
    {
        public Aeterna aeterna;

        Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();

        Image hpBarImage;
        Text hpText;
        Image skill4ChargeBarImage;
        List<Text> skillCooltimes = new List<Text>();

        int skillNum = 0;

        int chargeCount = 0;
        int ultimateCount = 0;

        private void Start()
        {
            ConnectUI();

            chargeCount = aeterna.chargeCount;
            ultimateCount = aeterna.ultimateCount;
        }

        void ConnectUI()
        {
            UIObjects["hpBar"] = GameCenterTest.FindObject(gameObject, "HpBar");
            UIObjects["hpText"] = GameCenterTest.FindObject(gameObject, "HpText");
            UIObjects["chargeBar"] = GameCenterTest.FindObject(gameObject, "ChargeBar");

            for (int i = 1; i <= 3; i++)
            {
                UIObjects["skill_" + i + "_Active"] = GameCenterTest.FindObject(gameObject, "Skill_" + i + "_Active");
                UIObjects["skill_" + i + "_CoolTime"] = GameCenterTest.FindObject(gameObject, "Skill_" + i + "_CoolTime");
                skillCooltimes.Add(UIObjects["skill_" + i + "_CoolTime"].GetComponent<Text>());
            }

            UIObjects["skill_4_Active"] = GameCenterTest.FindObject(gameObject, "Skill_4_Active");

            for(int i = 1; i <= 4; i++)
            {
                UIObjects["skill_"+i+"_Lock"] = GameCenterTest.FindObject(gameObject, "Skill_"+i+"_Lock");
            }

            hpBarImage = UIObjects["hpBar"].GetComponent<Image>();
            hpText = UIObjects["hpText"].GetComponent<Text>();
            skill4ChargeBarImage = UIObjects["chargeBar"].GetComponent<Image>();

            for (int i = 1; i <= 10; i++)
            {
                UIObjects["circleAble_" + i] = GameCenterTest.FindObject(gameObject, "CircleAble_" + i);
            }

        }

        private void Update()
        {
            ShowHp();
            ShowSkillActive();
            ShowCoolTime();
            ShowChargeBar();
            ShowCircleAble();
        }

        void ShowHp()
        {
            hpBarImage.fillAmount = (float)aeterna.hp / aeterna.dataHp;
            hpText.text = string.Format(aeterna.hp + " / " + aeterna.dataHp);
        }

        void ShowSkillActive()
        {
            if (aeterna.skillButton == skillNum) return;

            for(int i = 1; i <= 4; i++)
            {
                 if (aeterna.skillButton == i)
                 {
                        UIObjects["skill_" + i + "_Active"].SetActive(true);
                         skillNum = i;
                 }

                 else
                 {
                        UIObjects["skill_" + i + "_Active"].SetActive(false);
                        skillNum = 0;
                 }
            }
        }

        void ShowCoolTime()
        {
            for (int i = 0; i <= 2; i++)
            {
                if (aeterna.skillTimer[i + 1] >= 0.0f)
                {
                    skillCooltimes[i].text = string.Format("{0:F1}", aeterna.skillTimer[i + 1]);
                    UIObjects["skill_" + (i + 1) + "_Lock"].SetActive(true);
                }

                else
                {
                    skillCooltimes[i].text = "";
                    UIObjects["skill_" + (i + 1) + "_Lock"].SetActive(false);
                }
            }
        }

        void ShowChargeBar()
        {
            if (chargeCount != aeterna.chargeCount)
            {
                chargeCount = aeterna.chargeCount;
                skill4ChargeBarImage.fillAmount = 0.25f * aeterna.chargeCount;
            }
        }

        void ShowCircleAble()
        {
            if (ultimateCount != aeterna.ultimateCount)
            {
                ultimateCount = aeterna.ultimateCount;

                for (int i = 1; i <= ultimateCount; i++)
                {
                    UIObjects["circleAble_" + i].SetActive(true);
                }

                for(int i = ultimateCount + 1; i <=10; i++)
                {
                    UIObjects["circleAble_" + i].SetActive(false);
                }
            }
        }
    }
}