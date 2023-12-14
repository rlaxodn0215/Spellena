using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Player
{
    public class AeternaUI : MonoBehaviour
    {
        public Aeterna aeterna;

        [HideInInspector]
        public Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();

        Image hpBarImage;
        Text hpText;
        Image skill4ChargeBarImage;
        List<Text> skillCooltimes = new List<Text>();

        int skillNum = 0;

        int assistCount = 0;
        int ultimateCount = 0;

        private void Start()
        {
            ConnectUI();

            assistCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["AsisstCount"];
            ultimateCount = aeterna.ultimateCount;
        }

        void ConnectUI()
        {
            UIObjects["hpBar"] = GameCenterTest.FindObject(gameObject, "HpBar");
            UIObjects["hpText"] = GameCenterTest.FindObject(gameObject, "HpText");
            UIObjects["chargeBar"] = GameCenterTest.FindObject(gameObject, "ChargeBar");

            for (int i = 1; i <= 4; i++)
            {
                UIObjects["skill_" + i + "_Active"] = GameCenterTest.FindObject(gameObject, "Skill_" + i + "_Active");
                UIObjects["skill_" + i + "_CoolTime"] = GameCenterTest.FindObject(gameObject, "Skill_" + i + "_CoolTime");
                skillCooltimes.Add(UIObjects["skill_" + i + "_CoolTime"].GetComponent<Text>());
            }

            UIObjects["skill_2_Image_Nohold"] = GameCenterTest.FindObject(gameObject, "Skill_2_Image_Nohold");
            UIObjects["skill_2_Image_Hashold"] = GameCenterTest.FindObject(gameObject, "Skill_2_Image_Hashold");

            for (int i = 1; i <= 4; i++)
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

            UIObjects["skill_3_Image"] = GameCenterTest.FindObject(gameObject, "Skill_3_Image");
            UIObjects["skill_4_White"] = GameCenterTest.FindObject(gameObject, "Skill_4_Image_White");
            UIObjects["skill_4_Dark"] = GameCenterTest.FindObject(gameObject, "Skill_4_Image_Dark");
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
            if (aeterna.skillButton == skillNum && skillNum > 0 && skillNum < 4)
            {
                UIObjects["skill_" + skillNum + "_Active"].SetActive(false);
                return;
            }

            for(int i = 1; i <= 4; i++)
            {
                 if (aeterna.skillButton == i)
                 {
                        UIObjects["skill_" + i + "_Active"].SetActive(true);
                        if(i == 4) UIObjects["skill_4_Lock"].SetActive(true);
                        skillNum = i;
                 }

                 else
                 {
                        UIObjects["skill_" + i + "_Active"].SetActive(false);
                        skillNum = 0;
                 }
            }

            if (aeterna.skillButton == 4)
            {
                UIObjects["skill_4_Lock"].SetActive(false);
                Debug.Log("aeterna.skillButton == 4");

                if (!aeterna.dimensionCut.isHealingSword)
                {
                    UIObjects["skill_4_White"].SetActive(true);
                    UIObjects["skill_4_Dark"].SetActive(false);
                }

                else
                {
                    UIObjects["skill_4_White"].SetActive(false);
                    UIObjects["skill_4_Dark"].SetActive(true);
                }
            }
        }

        void ShowCoolTime()
        {
            for (int i = 0; i <= 3; i++)
            {
                if (aeterna.skillTimer[i + 1] >= 0.0f)
                {
                    skillCooltimes[i].text = string.Format("{0:F1}", aeterna.skillTimer[i + 1]);
                    UIObjects["skill_" + (i + 1) + "_Lock"].SetActive(true);
                }

                else
                {
                    skillCooltimes[i].text = "";

                    if(i == 3)
                    {
                        if(ultimateCount>=aeterna.doUltimateNum)
                        {
                            UIObjects["skill_" + (i + 1) + "_Lock"].SetActive(true);
                        }
                    }

                    else
                    {
                        UIObjects["skill_" + (i + 1) + "_Lock"].SetActive(false);                   
                    }
                }
            }
        }


        void ShowChargeBar()
        {
            if (assistCount != (int)PhotonNetwork.LocalPlayer.CustomProperties["AsisstCount"])
            {
                assistCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["AsisstCount"];
                skill4ChargeBarImage.fillAmount = 0.25f * assistCount;
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