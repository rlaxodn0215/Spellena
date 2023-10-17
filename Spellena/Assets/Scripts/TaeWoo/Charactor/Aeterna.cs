using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class Aeterna : Character
    {
        public CharacterData AeternaData;
        public GameObject DimensionSword;
        public GameObject DimensionSlash;
        public GameObject DimensionDoor;
        public GameObject DimensionDoorGUI;

        private DimensionSword dimensionSword;
        private DimensionOpen dimensionOpen;
        private DimensionIO dimensionIO;

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
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        void Initialize()
        {
            DimensionSword.tag = tag;

            dimensionSword = this.gameObject.AddComponent<DimensionSword>();
            dimensionSword.AddPlayer(this);
            Skills["BasicAttack"] = dimensionSword;

            dimensionOpen = this.gameObject.AddComponent<DimensionOpen>();
            dimensionOpen.AddPlayer(this);
            dimensionOpen.maxDistance = AeternaData.skill1DoorRange;
            Skills["Skill1"] = dimensionOpen;

            dimensionIO = this.gameObject.AddComponent<DimensionIO>();
            dimensionIO.AddPlayer(this);
            Skills["Skill2"] = dimensionIO;

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
            foreach (KeyValuePair<string, Ability> keyValue in Skills)
            {
                Ability ability = keyValue.Value;
                ability.IsDisActive();
            }

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
                    Skills["Skill1"].IsActive();
                    Debug.Log("Skill1 Ready");
                }
            }

            else if(skillTimer[0] <=0.0f)
            {
                skillButton = -1;
                Debug.Log("BasicAttack Ready");
            }
        }

        void OnSkill2()
        {
            foreach (KeyValuePair<string, Ability> keyValue in Skills)
            {
                Ability ability = keyValue.Value;
                ability.IsDisActive();
            }

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

            else if(skillTimer[0] <= 0.0f)
            {
                skillButton = -1;
                Debug.Log("BasicAttack Ready");
            }
        }

        void OnSkill3()
        {
            foreach (KeyValuePair<string, Ability> keyValue in Skills)
            {
                Ability ability = keyValue.Value;
                ability.IsDisActive();
            }
        }

        void OnSkill4()
        {
            foreach (KeyValuePair<string, Ability> keyValue in Skills)
            {
                Ability ability = keyValue.Value;
                ability.IsDisActive();
            }
        }

        void OnMouseButton()
        {
            if (skillButton == 1 && skillTimer[1] <= 0.0f)
            {
                Skills["Skill1"].Execution();
                playerActionDatas[(int)PlayerActionState.Skill1].isExecuting = true;
                skillTimer[1] = AeternaData.skill1Time;
                StartCoroutine(SkillTimer(1));
            }

            else if (skillButton == 2)
            {
                if (skillTimer[2] <= 0.0f)
                {
                    //skillTimer[2] = 1.0f;
                    Skills["Skill2"].Execution();
                    playerActionDatas[(int)PlayerActionState.Skill2].isExecuting = true;
                    StartCoroutine(ShowTimer(2));
                }

                else
                {
                    skillButton = 1;
                }
            }
            else
            {
                if (skillButton == -1 && skillTimer[0] <= 0.0f)
                {
                    Skills["BasicAttack"].Execution();
                    skillTimer[0] = AeternaData.basicAttackTime;
                    StartCoroutine(SkillTimer(0));
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

        IEnumerator ShowTimer(int index)
        {
            while(skillButton ==index)
            {
                skillTimer[index] = dimensionIO.timerForShow;
                yield return null;
            }
        }

        private void OnGUI()
        {
            GUI.TextField(new Rect(10, 10, 100, 30), skillTimer[2].ToString());
            //GUI.TextField(new Rect(10, 30, 100, 50), skillTimer[2].ToString());
        }

    }
}