using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class CultistUI : MonoBehaviour
    {
        public Cultist cultist;

        public Image hpImage;
        public Text hpText;
        public Text skill1CoolDownText;
        public Text skill2CoolDownText;
        public Text skill3CoolDownText;

        public GameObject skill1Lock;
        public GameObject skill2Lock;
        public GameObject skill3Lock;

        public GameObject skill1Active;
        public GameObject skill2Active;
        public GameObject skill3Active;
        public GameObject skill4Active;

        public GameObject[] circleEmpty;
        public GameObject[] circleFilled;

        void Update()
        {
            hpImage.fillAmount = (float)cultist.hp / cultist.dataHp;
            hpText.text = cultist.hp + " / " + cultist.dataHp;
            skill1CoolDownText.text = string.Format("{0:F1}", cultist.skillCoolDownTime[0]);
            skill2CoolDownText.text = string.Format("{0:F1}", cultist.skillCoolDownTime[1]);
            skill3CoolDownText.text = string.Format("{0:F1}", cultist.skillCoolDownTime[2]);

            if (cultist.skillCoolDownTime[0] <= 0f)
                skill1Lock.SetActive(false);
            else
                skill1Lock.SetActive(true);

            if (cultist.skillCoolDownTime[1] <= 0f)
                skill2Lock.SetActive(false);
            else
                skill2Lock.SetActive(true);

            if (cultist.skillCoolDownTime[2] <= 0f)
                skill3Lock.SetActive(false);
            else
                skill3Lock.SetActive(true);

            if (cultist.skillState == SkillStateCultist.Skill1Channeling)
                skill1Active.SetActive(true);
            else
                skill1Active.SetActive(false);

            if (cultist.skillState == SkillStateCultist.Skill2Channeling)
                skill2Active.SetActive(true);
            else
                skill2Active.SetActive(false);

            if (cultist.skillState == SkillStateCultist.Skill3Channeling)
                skill3Active.SetActive(true);
            else
                skill3Active.SetActive(false);

            if (cultist.skillState == SkillStateCultist.Skill4Casting)
                skill4Active.SetActive(true);
            else
                skill4Active.SetActive(false);

            for (int i = 0; i < circleEmpty.Length; i++)
            {
                if (i < cultist.ultimateCount)
                {
                    circleEmpty[i].SetActive(false);
                    circleFilled[i].SetActive(true);
                }
                else
                {
                    circleEmpty[i].SetActive(true);
                    circleFilled[i].SetActive(false);
                }
            }


        }
    }
}
