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

        protected override void Start() 
        {
            base.Start();
            Initialize();
        }

        protected override void Update()
        {
            base.Update();
            PlayerSkillInput();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        void Initialize()
        {
            Skills["BasicAttack"] = new DimensionSword(this);
            Skills["Skill1"] = new DimensionOpen(this);
            Skills["Skill2"] = new DimensionIO(this);

            skillTimer = new float[Skills.Count];

            for(int i = 0; i < Skills.Count; i++)
            {
                skillTimer[i] = -1;
            }

            Hp = AeternaData.Hp;
            moveSpeed = AeternaData.moveSpeed;
            jumpHeight = AeternaData.jumpHeight;
        }


        void OnSkill1()
        {
            if (skillTimer[1] <= 0.0f)
            {
                if (skillButton == 1)
                {
                    skillButton = -1;
                    Debug.Log("BasicAttack Ready");
                }

                else
                {
                    skillButton = 1;
                    Debug.Log("Skill1 Ready");
                }
            }

            else
            {
                skillButton = -1;
                Debug.Log("BasicAttack Ready");
            }
        }

        void OnSkill2()
        {
            if (skillTimer[2] <= 0.0f)
            {
                if (skillButton == 2)
                {
                    skillButton = -1;
                    Debug.Log("BasicAttack Ready");
                }

                else
                {
                    skillButton = 2;
                    Debug.Log("Skill2 Ready");
                }
            }

            else
            {
                skillButton = -1;
                Debug.Log("BasicAttack Ready");
            }
        }

        void OnSkill3()
        {

        }

        void OnSkill4()
        {

        }

        protected void PlayerSkillInput()
        {
            // �Էµ� ��ų ���� �� ���� �ش� ��ų ���
            if (Input.GetMouseButtonDown(0))
            {
                if(skillButton==1 && skillTimer[1]<=0.0f)
                {
                    Skills["Skill1"].Execution();
                    //playerActionDatas[(int)PlayerActionState.Skill1].isExecuting = true;
                    skillTimer[1] = AeternaData.skillTimer[1];
                    StartCoroutine(SkillTimer(1));
                }

                else if(skillButton==2 && skillTimer[2] <= 0.0f)
                {
                    Skills["Skill2"].Execution();
                    //playerActionDatas[(int)PlayerActionState.Skill2].isExecuting = true;
                    skillTimer[2] = AeternaData.skillTimer[2];
                    StartCoroutine(SkillTimer(2));
                }

                else
                {
                    if(skillButton == -1)
                        Skills["BasicAttack"].Execution();
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