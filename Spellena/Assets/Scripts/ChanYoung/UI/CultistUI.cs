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
        public GameObject skill4Lock;

        public GameObject skill1Active;
        public GameObject skill2Active;
        public GameObject skill3Active;
        public GameObject skill4Active;

        public GameObject[] circleEmpty;
        public GameObject[] circleFilled;

        public GameObject screenEffect;
        Image hitEffect;
        Image healEffect;
        public Image chargeBar;

        class ScreenEffect
        {
            public bool isUp = true;
            public string type;
            public int frame = 10;//fixedUpdate �� 10������
        }

        List<ScreenEffect> screenEffects = new List<ScreenEffect>();



        private void Start()
        {
            hitEffect = screenEffect.transform.Find("HitEffect").GetComponent<Image>();
            healEffect = screenEffect.transform.Find("HealEffect").GetComponent<Image>();
        }


        private void FixedUpdate()
        {
            PlayEffect();
        }

        private void PlayEffect()
        {
            for(int i = 0; i < screenEffects.Count; i++)
            {
                Color _tempColor;
                if (screenEffects[i].type == "Hit")
                {
                    _tempColor = hitEffect.color;
                    if (screenEffects[i].isUp)
                    {
                        _tempColor.a += 255 / 5;
                        screenEffects[i].frame--;
                        hitEffect.color = _tempColor;
                        if (screenEffects[i].frame <= 5)
                            screenEffects[i].isUp = false;
                    }
                    else
                    {
                        _tempColor.a -= 255 / 5;
                        screenEffects[i].frame--;
                        hitEffect.color = _tempColor;
                    }
                }
                else if(screenEffects[i].type == "Heal")
                {
                    _tempColor = healEffect.color;
                    if (screenEffects[i].isUp)
                    {
                        _tempColor.a += 255 / 5;
                        screenEffects[i].frame--;
                        healEffect.color = _tempColor;
                        if (screenEffects[i].frame <= 5)
                            screenEffects[i].isUp = false;
                    }
                    else
                    {
                        _tempColor.a -= 255 / 5;
                        screenEffects[i].frame--;
                        healEffect.color = _tempColor;
                    }
                }
            }

            for(int i = 0; i < screenEffects.Count; i++)
            {
                if (screenEffects[i].frame <= 0)
                {
                    screenEffects.RemoveAt(i);
                    i = -1;
                }
            }
        }

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

            if (cultist.ultimateCount >= 3)
                skill4Lock.SetActive(false);
            else
                skill4Lock.SetActive(true);



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

            chargeBar.fillAmount = cultist.chargeCount / 4;
        }


        public void PlayDamageEffect(int damage)
        {
            ScreenEffect _tempEffect = new ScreenEffect();
            if (damage == 0)
                return;
            else if (damage > 0)
                _tempEffect.type = "Hit";
            else if (damage < 0)
                _tempEffect.type = "Heal";
            screenEffects.Add(_tempEffect);
        }


    }
}
