using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Player
{
    public class ElementalOrder : Character, IPunObservable
    {
        public CharacterData elementalOrderData;
        public GameObject overlayCamera;
        Vector3 overlayCameraDefaultPos;
        public GameObject Aim;

        Animator overlayAnimator;

        Vector3 handPoint;
        Vector3 networkHandPoint;
        Vector3 currentHandPoint;


        //스킬 순서 11, 12, 13, 22, 23, 33 총 6개
        List<int> commands = new List<int>();

        bool isReadyToUseSkill = false;

        bool isSpell1 = false;
        bool isSpell2 = false;
        bool isSpell3 = false;
        bool isSpell4 = false;
        bool isSpell5 = false;
        bool isSpell6 = false;


        protected override void Awake()
        {
            base.Awake();
            CheckPoint();
            currentHandPoint = handPoint;
        }

        protected override void Start()
        {
            base.Start();
            Initialize();
            overlayCameraDefaultPos = overlayCamera.transform.localPosition;
        }

        protected override void Update()
        {
            base.Update();
            if (photonView.IsMine)
            {
                PlayerSkillInput();
                CheckOverlayAnimator();
                CheckPoint();
            }
        }

        void CheckPoint()
        {
            Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(Aim.transform.position);
            handPoint = _tempRay.origin + _tempRay.direction * 0.5f;
        }

        float rightCurrentWeight = 0f;
        float leftCurrentWeight = 0f;
        float targetWeight = 0.3f;
        protected override void OnAnimatorIK()
        {
            base.OnAnimatorIK();
            if (photonView.IsMine)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftHand, handPoint);
                animator.SetIKPosition(AvatarIKGoal.RightHand, handPoint);
                if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell1"))
                {
                    if (rightCurrentWeight < targetWeight)
                    {
                        rightCurrentWeight += Time.deltaTime;
                        leftCurrentWeight += Time.deltaTime;
                        if (rightCurrentWeight > targetWeight)
                        {
                            rightCurrentWeight = targetWeight;
                            leftCurrentWeight = targetWeight;
                        }
                    }
                }

                else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell5"))
                {
                    if (rightCurrentWeight < targetWeight)
                    {
                        rightCurrentWeight += Time.deltaTime / 4;
                        leftCurrentWeight -= Time.deltaTime / 4;
                        if (rightCurrentWeight > targetWeight)
                        {
                            rightCurrentWeight = targetWeight;
                        }
                        if (leftCurrentWeight < 0f)
                        {
                            leftCurrentWeight = 0;
                        }

                    }
                }
                else
                {
                    if (rightCurrentWeight > 0f)
                    {
                        rightCurrentWeight -= Time.deltaTime;
                        leftCurrentWeight -= Time.deltaTime;
                        if (rightCurrentWeight < 0f)
                        {
                            rightCurrentWeight = 0;
                            leftCurrentWeight = 0;
                        }
                    }
                }
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftCurrentWeight);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightCurrentWeight);
            }

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
            if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell1"))
            {
                overlayCamera.transform.localPosition = overlayCameraDefaultPos + new Vector3(0, 0.383f, 0);
                overlayAnimator.SetBool("Spell1", false);
                animator.SetBool("Spell1", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell2"))
            {
                overlayCamera.transform.localPosition = overlayCameraDefaultPos + new Vector3(0, 0.1f, 0);
                overlayAnimator.SetBool("Spell2", false);
                animator.SetBool("Spell2", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell3"))
            {
                overlayCamera.transform.localPosition = overlayCameraDefaultPos + new Vector3(0, 0.1f, 0);
                overlayAnimator.SetBool("Spell3", false);
                animator.SetBool("Spell3", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell4"))
            {
                overlayCamera.transform.localPosition = overlayCameraDefaultPos + new Vector3(0, 0.1f, 0);
                overlayAnimator.SetBool("Spell4", false);
                animator.SetBool("Spell4", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell5"))
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition, overlayCameraDefaultPos + new Vector3(0, 0.383f, 0), Time.deltaTime / 2);
                overlayAnimator.SetBool("Spell5", false);
                animator.SetBool("Spell5", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell6"))
            {
                //overlayCamera.transform.localPosition = overlayCameraDefaultPos + new Vector3(0, 0.1f, 0);
                overlayAnimator.SetBool("Spell6", false);
                animator.SetBool("Spell6", false);
            }
            else
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition, overlayCameraDefaultPos, Time.deltaTime * 3);
            }


        }

        public override void IsLocalPlayer()
        {
            base.IsLocalPlayer();
            overlayCamera.SetActive(true);
        }

        void OnSkill1()
        {
            if (photonView.IsMine)
            {
                if (commands.Count < 2)
                {
                    commands.Add(1);
                }
            }
        }

        void OnSkill2()
        {
            if (photonView.IsMine)
            {
                if (commands.Count < 2)
                {
                    commands.Add(2);
                }
            }
        }
        void OnSkill3()
        {
            if (photonView.IsMine)
            {
                if (commands.Count < 2)
                {
                    commands.Add(3);
                }
            }
        }

        void OnSkill4()
        {
            if (photonView.IsMine)
            {
                if (commands.Count >= 2)
                {
                    isReadyToUseSkill = true;
                }
            }
        }
        void OnMouseButton()
        {
            if (photonView.IsMine)
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
                        else if (commands[0] == 3 && commands[1] == 3)
                        {
                            reverseAnimatorBool("Spell6");
                            isSpell6 = true;
                        }
                    }
                }
            }
        }
        protected void PlayerSkillInput()
        {
            if (isReadyToUseSkill == true)
            {
                if (isSpell1 == true)
                {
                    commands.Clear();
                    //스킬1이 발사된다.
                    isSpell1 = false;
                    isReadyToUseSkill = false;
                }
                else if (isSpell2 == true)
                {
                    commands.Clear();
                    isSpell2 = false;
                    isReadyToUseSkill = false;
                }
                else if (isSpell3 == true)
                {
                    commands.Clear();
                    isSpell3 = false;
                    isReadyToUseSkill = false;
                }
                else if (isSpell4 == true)
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
                else if (isSpell6 == true)
                {
                    commands.Clear();
                    isSpell6 = false;
                    isReadyToUseSkill = false;
                }
            }
        }

        private void reverseAnimatorBool(string parameter)
        {
            overlayAnimator.SetBool(parameter, !overlayAnimator.GetBool(parameter));
            animator.SetBool(parameter, !animator.GetBool(parameter));
        }

        public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            base.OnPhotonSerializeView(stream, info);
            if (stream.IsWriting)
            {
                stream.SendNext(handPoint);
                stream.SendNext(rightCurrentWeight);
                stream.SendNext(leftCurrentWeight);
            }
            else
            {
                networkHandPoint = (Vector3)stream.ReceiveNext();
                rightCurrentWeight = (float)stream.ReceiveNext();
                leftCurrentWeight = (float)stream.ReceiveNext();
            }
        }
    }
}
