using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class DracosonUI : MonoBehaviour
    {
        public Dracoson dracoson;

        public Image hpImage;
        public Text hpText;
        public Text skill1CoolDownText;
        public Text skill2CoolDownText;
        public Text skill3CoolDownText;

        public GameObject skill1Lock;
        public GameObject skill2Lock;
        public GameObject skill3Lock;
        public GameObject skill4Lock;

        public GameObject skill1Active;
        public GameObject skill2Active;
        public GameObject skill3Active;
        public GameObject skill4Active;

        public GameObject[] circleEmpty;
        public GameObject[] circleFilled;

        public Image chargeBar;

        void FixedUpdate()
        {
            hpImage.fillAmount = (float)dracoson.hp / dracoson.dataHp;
            hpText.text = dracoson.hp + " / " + dracoson.dataHp;
            skill1CoolDownText.text = string.Format("{0:F1}", dracoson.skillCoolDownTime[0]);
            skill2CoolDownText.text = string.Format("{0:F1}", dracoson.skillCoolDownTime[1]);
            skill3CoolDownText.text = string.Format("{0:F1}", dracoson.skillCoolDownTime[2]);

            if (dracoson.skillCoolDownTime[0] <= 0f)
                skill1Lock.SetActive(false);
            else
                skill1Lock.SetActive(true);

            if (dracoson.skillCoolDownTime[1] <= 0f)
                skill2Lock.SetActive(false);
            else
                skill2Lock.SetActive(true);

            if (dracoson.skillCoolDownTime[2] <= 0f)
                skill3Lock.SetActive(false);
            else
                skill3Lock.SetActive(true);

            if (dracoson.ultimateCount >= 3)
                skill4Lock.SetActive(false);
            else
                skill4Lock.SetActive(true);



            if (dracoson.skillState == SkillStateDracoson.Skill1Channeling)
                skill1Active.SetActive(true);
            else
                skill1Active.SetActive(false);

            if (dracoson.skillState == SkillStateDracoson.Skill2Channeling)
                skill2Active.SetActive(true);
            else
                skill2Active.SetActive(false);

            if (dracoson.skillState == SkillStateDracoson.Skill3Holding)
                skill3Active.SetActive(true);
            else
                skill3Active.SetActive(false);

            if (dracoson.skillState == SkillStateDracoson.Skill4Casting)
                skill4Active.SetActive(true);
            else
                skill4Active.SetActive(false);

            for (int i = 0; i < circleEmpty.Length; i++)
            {
                if (i < dracoson.ultimateCount)
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

            chargeBar.fillAmount = dracoson.chargeCount / 4;
        }


    }
}
