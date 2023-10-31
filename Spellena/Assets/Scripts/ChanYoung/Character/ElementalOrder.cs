using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Player
{
    public class ElementalOrder : Character, IPunObservable
    {
        public CharacterData elementalOrderData;

        public GameObject overlayCamera;
        public GameObject minimapCamera;

        Vector3 overlayCameraDefaultPos;
        public GameObject Aim;
        public GameObject OverlaySight;

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
        bool isClicked = false;

        Ray screenRay;

        public enum SkillState
        {
            None, BurstFlare, GaiaTied, EterialStorm
        }

        SkillState skillState = SkillState.None;

        BurstFlare burstFlare;
        GaiaTied gaiaTied;
        EterialStorm eterialStorm;

        float burstFlareCoolDownTime = 0f;
        float GaiaTiedCoolDownTime = 0f;

        public GameObject leftHandSpell;
        public GameObject rightHandSpell;

        public GameObject testCube;

        public Material fireMaterial;
        public Material landMaterial;
        public Material stormMaterial;

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
                CheckCoolDown();
                CheckOverlayAnimator();
                CheckPoint();
                CheckSkillOnMine();
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
        void CheckCoolDown()
        {
            if(burstFlare != null)
            {
                burstFlare.ShootCoolDown();
            }

            if(burstFlareCoolDownTime > 0f)
            {
                burstFlareCoolDownTime -= Time.deltaTime;
            }

        }

        void UseSkill(Vector3 origin, Vector3 direction)
        {
            if(skillState == SkillState.BurstFlare)
            {
                if(burstFlare == null)
                {
                    burstFlare = new BurstFlare();
                    burstFlare.Initialize();
                    overlayAnimator.SetBool("Spell3", true);
                }

                if(burstFlare.CheckCoolDown() == false)
                {
                    Debug.Log("쿨타임");
                    return;
                }

                bool _result = burstFlare.ShootBullet();

                object[] _data = new object[5];
                _data[0] = name;
                _data[1] = tag;
                _data[2] = "BurstFlare";
                _data[3] = origin + direction;
                _data[4] = direction;
                PhotonNetwork.Instantiate("ChanYoung/Prefabs/BurstFlare", origin, Quaternion.identity, data: _data);

                if (_result == true)
                {
                    skillState = SkillState.None;
                    burstFlareCoolDownTime = burstFlare.GetSkillCoolDownTime();
                    burstFlare = null;
                }
            }
            else if(skillState == SkillState.EterialStorm)
            {
                if(eterialStorm == null)
                {
                    Rect _tempRect = minimapCamera.GetComponent<Camera>().rect;
                    _tempRect.x = 0;
                    _tempRect.y = 0;
                    _tempRect.width = 1;
                    _tempRect.height = 1;
                    minimapCamera.GetComponent<Camera>().rect = _tempRect;
                    camera.GetComponent<MouseControl>().enabled = false;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;

                    eterialStorm = new EterialStorm();
                }
                else
                {
                    Ray _tempRay = minimapCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                    RaycastHit _tempHit;
                    int _tempLayerMask = ~(1 << LayerMask.NameToLayer("Minimap"));
                    if (Physics.Raycast(_tempRay, out _tempHit, Mathf.Infinity, _tempLayerMask))
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            Vector3 _tempVec = new Vector3(_tempHit.point.x, _tempHit.point.y + 1f, _tempHit.point.z);
                            Instantiate(testCube, _tempVec, Quaternion.identity);

                            Rect _tempRect = minimapCamera.GetComponent<Camera>().rect;
                            _tempRect.x = 0.7f;
                            _tempRect.y = 0.7f;
                            _tempRect.width = 0.3f;
                            _tempRect.height = 0.3f;
                            minimapCamera.GetComponent<Camera>().rect = _tempRect;

                            camera.GetComponent<MouseControl>().enabled = true;
                            Cursor.visible = false;
                            Cursor.lockState = CursorLockMode.Locked;

                            skillState = SkillState.None;
                            eterialStorm = null;
                        }
                    }
                }
            }
        }


        bool isEterialStorm = false;
        void CheckSkillOnMine()
        {
            if(skillState == SkillState.EterialStorm)
            {

            }
        }

        //RPC
        [PunRPC]
        public void AddCommand(int command)
        {
            if(commands.Count < 2)
            {
                commands.Add(command);
            }

            if(commands.Count >= 2)
            {
                isReadyToUseSkill = true;
            }
        }
        [PunRPC]
        public void CancelSkill()
        {
            isReadyToUseSkill = false;

            if(skillState == SkillState.BurstFlare)
            {
                burstFlareCoolDownTime = burstFlare.GetSkillCoolDownTime();
                burstFlare = null;
            }

            skillState = SkillState.None;
            commands.Clear();
        }
        [PunRPC]
        public void ClickMouse(Vector3 origin, Vector3 direction)
        {
            if(skillState == SkillState.None)
            {
                if(isReadyToUseSkill == true)
                {
                    if (((commands[0] == 1 && commands[1] == 3)
                        || (commands[0] == 3 && commands[1] == 1)) && burstFlareCoolDownTime <= 0f)
                    {
                        skillState = SkillState.BurstFlare;
                        isReadyToUseSkill = false;
                        UseSkill(origin, direction);
                        commands.Clear();
                    }
                    else if (((commands[0] == 2 && commands[1] == 3)
                        || (commands[0] == 3 && commands[1] == 2)) && GaiaTiedCoolDownTime <= 0f)
                    {
                        skillState = SkillState.GaiaTied;
                        isReadyToUseSkill = false;
                        UseSkill(origin, direction);
                        commands.Clear();
                    }
                    else if (((commands[0] == 2 && commands[1] == 3)
                        || (commands[0] == 3 && commands[1] == 2)) && GaiaTiedCoolDownTime <= 0f)
                    {
                        skillState = SkillState.EterialStorm;
                        isReadyToUseSkill = false;
                        UseSkill(origin, direction);
                        commands.Clear();
                    }
                }
            }
            else
            {
                UseSkill(origin, direction);
            }
        }

        //입력
        void OnSkill1()
        {
            if (photonView.IsMine)
            {
                photonView.RPC("AddCommand", RpcTarget.MasterClient, 1);
            }
        }

        void OnSkill2()
        {
            if (photonView.IsMine)
            {
                photonView.RPC("AddCommand", RpcTarget.MasterClient, 2);
            }
        }
        void OnSkill3()
        {
            if (photonView.IsMine)
            {
                photonView.RPC("AddCommand", RpcTarget.MasterClient, 3);
            }
        }

        void OnButtonCancel()
        {
            if(photonView.IsMine)
            {
                photonView.RPC("CancelSkill", RpcTarget.MasterClient);
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
                isClicked = !isClicked;
                if (isClicked == true)
                {
                    photonView.RPC("ClickMouse", RpcTarget.MasterClient, screenRay.origin, screenRay.direction);
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
                stream.SendNext(skillState);
            }
            else
            {
                networkHandPoint = (Vector3)stream.ReceiveNext();
                networkRightCurrentWeight = (float)stream.ReceiveNext();
                networkLeftCurrentWeight = (float)stream.ReceiveNext();
                skillState = (SkillState)stream.ReceiveNext();
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

        void SetHandEffectPositionIK(int typeRight, int typeLeft)
        {
            if (typeRight == 0)
                rightHandSpell.GetComponent<MeshRenderer>().enabled = false;
            else
            {
                rightHandSpell.GetComponent<MeshRenderer>().enabled = true;
                if (typeRight == 1)
                    rightHandSpell.GetComponent<MeshRenderer>().material = fireMaterial;
                else if(typeRight == 2)
                    rightHandSpell.GetComponent<MeshRenderer>().material = landMaterial;
                else if (typeRight == 3)
                    rightHandSpell.GetComponent<MeshRenderer>().material = stormMaterial;
            }
            if (typeLeft == 0)
                leftHandSpell.GetComponent<MeshRenderer>().enabled = false;
            else
            {
                leftHandSpell.GetComponent<MeshRenderer>().enabled = true;
                if (typeLeft == 1)
                    leftHandSpell.GetComponent<MeshRenderer>().material = fireMaterial;
                else if (typeLeft == 2)
                    leftHandSpell.GetComponent<MeshRenderer>().material = landMaterial;
                else if (typeLeft == 3)
                    leftHandSpell.GetComponent<MeshRenderer>().material = stormMaterial;
            }


            rightHandSpell.transform.position = overlayAnimator.GetBoneTransform(HumanBodyBones.RightHand).position
                                                + overlayAnimator.GetBoneTransform(HumanBodyBones.RightHand).right * 0.1f
                                                - overlayAnimator.GetBoneTransform(HumanBodyBones.RightHand).up * 0.1f;

            Vector3 _tempLeftHand = overlayAnimator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            Vector3 _tempLeftIKHand = overlayAnimator.GetIKPosition(AvatarIKGoal.LeftHand);
            float _tempWeight = overlayAnimator.GetIKPositionWeight(AvatarIKGoal.LeftHand);

            Quaternion _tempLeftHandRot = overlayAnimator.GetBoneTransform(HumanBodyBones.LeftHand).rotation;
            Quaternion _tempLeftIKHandRot = overlayAnimator.GetIKRotation(AvatarIKGoal.LeftHand);
            Quaternion _tempFinalLeftHandRot = Quaternion.Slerp(_tempLeftIKHandRot, _tempLeftHandRot,
                overlayAnimator.GetIKRotationWeight(AvatarIKGoal.LeftHand));
            Vector3 _tempLeftHandDirectionForward = _tempFinalLeftHandRot * Vector3.forward * 0.1f;
            Vector3 _tempLeftHandDirectionRight = _tempFinalLeftHandRot * Vector3.right * 0.2f;
            Vector3 _tempLeftHandDirectionUp = _tempFinalLeftHandRot * Vector3.up * 0.25f;
            leftHandSpell.transform.position = _tempLeftHand * (1 - _tempWeight) + _tempLeftIKHand * _tempWeight
                - _tempLeftHandDirectionForward + _tempLeftHandDirectionRight - _tempLeftHandDirectionUp;
        }

        protected override void OnAnimatorIK()
        {
            base.OnAnimatorIK();
            if (photonView.IsMine)
            {
                if (commands.Count <= 0)
                {
                    SetHandEffectPositionIK(0, 0);
                }
                else if(commands.Count == 1)
                {
                    SetHandEffectPositionIK(commands[0], 0);
                }
                else
                {
                    SetHandEffectPositionIK(commands[0], commands[1]);
                }


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

                
                if(overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
                {
                    overlayAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.2f);
                    overlayAnimator.SetIKPosition(AvatarIKGoal.LeftHand, OverlaySight.transform.position);
                }
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
            screenRay = camera.GetComponent<Camera>().ScreenPointToRay(Aim.transform.position);
            handPoint = screenRay.origin + screenRay.direction;
        }

    }
}
