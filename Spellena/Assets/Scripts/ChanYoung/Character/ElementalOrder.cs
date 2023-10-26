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

        float networkRightCurrentWeight;
        float networkLeftCurrentWeight;

        float rightCurrentWeight = 0.1f;
        float leftCurrentWeight = 0.1f;

        float rightNotMineCurrentWeight = 0.1f;
        float leftNotMineCurrentWeight = 0.1f;

        float targetWeight = 0.3f;

        //스킬 순서 11, 12, 13, 22, 23, 33 총 6개
        List<int> commands = new List<int>();

        bool isReadyToUseSkill = false;

        bool isSpell1 = false;
        bool isSpell2 = false;
        bool isSpell3 = false;
        bool isSpell4 = false;
        bool isSpell5 = false;
        bool isSpell6 = false;

        BurstFlare burstFlare;



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

        public override void IsLocalPlayer()
        {
            base.IsLocalPlayer();
            overlayCamera.SetActive(true);
        }
        //스킬














        //입력
        void OnSkill1()
        {
            if (photonView.IsMine)
            {
                if (commands.Count < 2)
                {
                    commands.Add(1);
                }

                if(commands.Count >= 2)
                {
                    isReadyToUseSkill = true;
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

                if (commands.Count >= 2)
                {
                    isReadyToUseSkill = true;
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

                if (commands.Count >= 2)
                {
                    isReadyToUseSkill = true;
                }
            }
        }


        void OnButtonCancel()
        {
            if(photonView.IsMine)
            {
                isReadyToUseSkill = false;
                commands.Clear();
            }
        }

        void OnSkill4()
        {
            Debug.Log("4번 비어있음");
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

        //동기화
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
                networkRightCurrentWeight = (float)stream.ReceiveNext();
                networkLeftCurrentWeight = (float)stream.ReceiveNext();
            }
        }




        //애니메이션

        void SetSpellReadyEffect()
        {
            
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
                    if (rightCurrentWeight > 0.1f)
                    {
                        rightCurrentWeight -= Time.deltaTime;
                        leftCurrentWeight -= Time.deltaTime;
                        if (rightCurrentWeight < 0.1f)
                        {
                            rightCurrentWeight = 0.1f;
                            leftCurrentWeight = 0.1f;
                        }
                    }
                }
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftCurrentWeight);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightCurrentWeight);
            }
            else
            {
                Vector3 _tempHandPoint = Vector3.Lerp(currentHandPoint, networkHandPoint, Time.deltaTime * 5);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, _tempHandPoint);
                animator.SetIKPosition(AvatarIKGoal.RightHand, _tempHandPoint);
                currentHandPoint = _tempHandPoint;

                float _tempRightHandWeight = Mathf.Lerp(rightNotMineCurrentWeight, networkRightCurrentWeight, Time.deltaTime * 5);
                float _tempLeftHandWeight = Mathf.Lerp(leftNotMineCurrentWeight, networkLeftCurrentWeight, Time.deltaTime * 5);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _tempRightHandWeight);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _tempLeftHandWeight);
                rightNotMineCurrentWeight = _tempRightHandWeight;
                leftNotMineCurrentWeight = _tempLeftHandWeight;
            }
        }

        private void reverseAnimatorBool(string parameter)
        {
            overlayAnimator.SetBool(parameter, !overlayAnimator.GetBool(parameter));
            animator.SetBool(parameter, !animator.GetBool(parameter));
        }

        void CheckPoint()
        {
            Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(Aim.transform.position);
            handPoint = _tempRay.origin + _tempRay.direction;
        }

    }
}
