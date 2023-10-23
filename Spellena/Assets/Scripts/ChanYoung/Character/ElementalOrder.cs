using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Player
{
    public class ElementalOrder : Character
    {
        public CharacterData elementalOrderData;

        [HideInInspector]
        public int skillButton = -1;
        [HideInInspector]
        public float[] skillTimer;
        public GameObject overlayCamera;
        Vector3 overlayCameraDefaultPos;

        Animator overlayAnimator;


        //스킬 순서 11, 12, 13, 22, 23, 33 총 6개
        List<int> commands = new List<int>();

        bool isReadyToUseSkill = false;

        bool isSpell1 = false;
        bool isSpell2 = false;
        bool isSpell3 = false;
        bool isSpell4 = false;
        bool isSpell5 = false;
        bool isSpell6 = false;

        
        protected override void Start()
        {
            base.Start();
            Initialize();
            overlayCameraDefaultPos = overlayCamera.transform.localPosition;
        }

        protected override void Update()
        {
            base.Update();
            PlayerSkillInput();
            CheckOverlayAnimator();
            Debug.Log(commands.Count);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        private void Initialize()
        {
            hp = elementalOrderData.Hp;
            walkSpeed = elementalOrderData.moveSpeed;
            jumpHeight = elementalOrderData.jumpHeight;
            overlayAnimator = transform.GetChild(1).GetComponent<Animator>();
        }

        void CheckOverlayAnimator()
        {
            if(overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell1"))
            {
                overlayCamera.transform.localPosition = overlayCameraDefaultPos + new Vector3(0, 0.383f, 0);
                overlayAnimator.SetBool("Spell1", false);
            }
            else if(overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell2"))
            {
                overlayCamera.transform.localPosition = overlayCameraDefaultPos + new Vector3(0, 0.1f, 0);
                overlayAnimator.SetBool("Spell2", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell3"))
            {
                overlayCamera.transform.localPosition = overlayCameraDefaultPos + new Vector3(0, 0.1f, 0);
                overlayAnimator.SetBool("Spell3", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell4"))
            {
                overlayCamera.transform.localPosition = overlayCameraDefaultPos + new Vector3(0, 0.1f, 0);
                overlayAnimator.SetBool("Spell4", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell5"))
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition, overlayCameraDefaultPos + new Vector3(0, 0.383f, 0), Time.deltaTime / 2);
                overlayAnimator.SetBool("Spell5", false);
            }
            else
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition, overlayCameraDefaultPos, Time.deltaTime * 3);
            }


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
        void OnMouseButton()
        {
            if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Idle")
                && isReadyToUseSkill == true)
            {
                if (commands.Count == 2)
                {
                    if (commands[0] == 1 && commands[1] == 1)
                    {
                        reverseAnimatorBool("Spell1");
                        isSpell1 = true;
                    }
                    else if ((commands[0] == 1 && commands[1] == 2)
                        || (commands[0] == 2 && commands[1] == 1))
                    {
                        reverseAnimatorBool("Spell2");
                        isSpell2 = true;
                    }
                    else if ((commands[0] == 1 && commands[1] == 3)
                        || (commands[0] == 3 && commands[1] == 1))
                    {
                        reverseAnimatorBool("Spell3");
                        isSpell3 = true;
                    }
                    else if (commands[0] == 2 && commands[1] == 2)
                    {
                        reverseAnimatorBool("Spell4");
                        isSpell4 = true;
                    }
                    else if ((commands[0] == 2 && commands[1] == 3) || (commands[0] == 3 && commands[1] == 2))
                    {
                        reverseAnimatorBool("Spell5");
                        isSpell5 = true;
                    }
                }
            }
        }
        protected void PlayerSkillInput()
        {
            if(isReadyToUseSkill == true)
            {
                if(isSpell1 == true)
                {
                    commands.Clear();
                    //스킬1이 발사된다.
                    isSpell1 = false;
                    isReadyToUseSkill = false;
                }
                else if(isSpell2 == true)
                {
                    commands.Clear();
                    isSpell2 = false;
                    isReadyToUseSkill = false;
                }
                else if(isSpell3 == true)
                {
                    commands.Clear();
                    isSpell3 = false;
                    isReadyToUseSkill = false;
                }
                else if(isSpell4 == true)
                {
                    commands.Clear();
                    isSpell4 = false;
                    isReadyToUseSkill = false;
                }
                else if (isSpell5 == true)
                {
                    commands.Clear();
                    isSpell5 = false;
                    isReadyToUseSkill = false;
                }
            }
        }

        private void reverseAnimatorBool(string parameter)
        {
            overlayAnimator.SetBool(parameter, !overlayAnimator.GetBool(parameter));
        }

    }
}
