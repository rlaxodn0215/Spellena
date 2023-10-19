using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Player
{
    public class ElementalOrder : Character
    {
        public CharacterData elementalOrderData;

        [HideInInspector]
        public int skillButton = -1;
        [HideInInspector]
        public float[] skillTimer;

        //스킬 순서 11, 12, 13, 22, 23, 33 총 6개
        List<int> commands = new List<int>();
        bool isReadyToUseSkill = false;


        
        protected override void Start()
        {
            base.Start();
            Initialize();
        }

        protected override void Update()
        {
            base.Update();
            ChangeAnimationParameter();
            PlayerSkillInput();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        protected void ChangeAnimationParameter()
        {
            if(moveVec.z == 0 && moveVec.x > 0)
            {
                animator.SetBool("Mirror", true);
            }
            else if(moveVec.z == 0 && moveVec.x < 0)
            {
                animator.SetBool("Mirror", false);
            }
        }


        private void Initialize()
        {
            RagnaEdge ragnaEdge = this.gameObject.AddComponent<RagnaEdge>();
            Skills["RagnaEdge"] = ragnaEdge;

            skillTimer = new float[Skills.Count];

            for(int i = 0; i < Skills.Count; i++)
            {
                skillTimer[i] = -1;
            }

            hp = elementalOrderData.Hp;
            walkSpeed = elementalOrderData.moveSpeed;
            jumpHeight = elementalOrderData.jumpHeight;
        }

        void OnSkill1()
        {
            if(commands.Count < 2)
            {
                commands.Add(1);
            }
        }

        void OnSkill2()
        {
            if (commands.Count < 2)
            {
                commands.Add(2);
            }
        }

        void OnSkill3()
        {
            if (commands.Count < 2)
            {
                commands.Add(3);
            }
        }

        void OnSkill4()
        {
            if(commands.Count >= 2)
            {
                isReadyToUseSkill = true;
            }
        }

        protected void PlayerSkillInput()
        {
            if(isReadyToUseSkill == true)
            {

            }
        }
    }
}
