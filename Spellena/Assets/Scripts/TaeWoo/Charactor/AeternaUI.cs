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

        private void Start()
        {
            ConnectUI();
        }

        void ConnectUI()
        {
            UIObjects["hpBar"] = GlobalUI.FindObject(gameObject, "HpBar");
            UIObjects["hpText"] = GlobalUI.FindObject(gameObject, "HpText");
            UIObjects["chargeBar"] = GlobalUI.FindObject(gameObject, "ChargeBar");

            for (int i = 1; i <= 3; i++)
            {
                UIObjects["skill_" + i + "_Active"] = GlobalUI.FindObject(gameObject, "Skill_" + i + "_Active");
                UIObjects["skill_" + i + "_CoolTime"] = GlobalUI.FindObject(gameObject, "Skill_" + i + "_CoolTime");
                skillCooltimes.Add(UIObjects["skill_" + i + "_CoolTime"].GetComponent<Text>());
            }

            UIObjects["skill_4_Active"] = GlobalUI.FindObject(gameObject, "Skill_4_Active");

            for(int i = 1; i <=4; i++)
            {
                UIObjects["skill_"+i+"_Lock"] = GlobalUI.FindObject(gameObject, "Skill_"+i+"_Lock");
            }

            hpBarImage = UIObjects["hpBar"].GetComponent<Image>();
            hpText = UIObjects["hpText"].GetComponent<Text>();
            skill4ChargeBarImage = UIObjects["chargeBar"].GetComponent<Image>();

            for (int i = 1; i <= 10; i++)
            {
                UIObjects["circleAble_" + i] = GlobalUI.FindObject(gameObject, "CircleAble_" + i);
            }

        }

        private void Update()
        {
            ShowHp();
            ShowSkillActive();
            ShowCoolTime();
            ShowKill();
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

        public void ShowKill()
        {

        }

        void ShowChargeBar()
        {

        }

        void ShowCircleAble()
        {

        }
    }
}