using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class Aeterna : Charactor
    {
        public CharactorData AeternaData;
        public GameObject DimensionSword;
        public GameObject DimensionDoor;

        [HideInInspector]
        public int skillButton = -1;
        [HideInInspector]
        public float[] skillTimer;

        protected override void CharactorStart() 
        { 
            Initialize();
        }

        protected override void CharactorUpdate()
        {
            
        }

        protected override void CharactorFixedUpdate()
        {

        }

        void Initialize()
        {
            Skills["Basic_Attack"] = new DimensionSword(this);
            Skills["Skill_1"] = new DimensionOpen(this);
            Skills["Skill_2"] = new DimensionIO(this);

            skillTimer = new float[Skills.Count];

            for(int i = 0; i < Skills.Count; i++)
            {
                skillTimer[i] = -1;
            }

            Hp = AeternaData.Hp;
            moveSpeed = AeternaData.moveSpeed;
            jumpHeight = AeternaData.jumpHeight;
        }


        void OnSkill_1()
        {
            if (skillButton == 1) skillButton = -1;
            if (skillTimer[1] <= 0.0f)
            {
                if (skillButton == 1) skillButton = -1;
                else skillButton = 1;
            }
        }

        void OnSkill_2()
        {
            if (skillButton == 2) skillButton = -1;
            if (skillTimer[2] <= 0.0f)
            {
                if (skillButton == 2) skillButton = -1;
                else skillButton = 2;
            }
        }

        void OnSkill_3()
        {

        }

        void OnSkill_4()
        {

        }

        protected override void PlayerSkillInput()
        {
            // 입력된 스킬 슬롯 에 따라 해당 스킬 사용
            if (Input.GetMouseButtonDown(0))
            {
                if(skillButton==1 && skillTimer[1]<=0.0f)
                {
                    Skills["Skill_1"].Execution();
                    skillTimer[1] = AeternaData.skillTimer[1];
                    StartCoroutine(SkillTimer(1));
                }

                else if(skillButton==2 && skillTimer[2] <= 0.0f)
                {
                    Skills["Skill_2"].Execution();
                    skillTimer[2] = AeternaData.skillTimer[2];
                    StartCoroutine(SkillTimer(2));
                }

                else
                {
                    if(skillButton == -1)
                        Skills["Basic_Attack"].Execution();
                }
            }
        }

        IEnumerator SkillTimer(int skillIndex)
        {
            while (skillTimer[skillIndex] > 0.0f)
            {
                skillTimer[skillIndex] -= Time.deltaTime;
                yield return null;
            }
        }

        private void OnGUI()
        {
            GUI.TextField(new Rect(10, 10, 100, 30), skillTimer[1].ToString());
            GUI.TextField(new Rect(10, 30, 100, 50), skillTimer[2].ToString());
        }

    }
}