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
        RenderTexture minimapRenderTexture;

        Vector3 overlayCameraDefaultPos;
        public GameObject Aim;
        public GameObject OverlaySight;

        Animator overlayAnimator;
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

        Vector3 pointStrike;
        bool isPointStrike = false;
        Vector3 arrivedVec;

        public enum SkillState
        {
            None, BurstFlare, GaiaTied, EterialStorm, RagnaEdge, TerraBreak, MeteorStrike
        }

        SkillState skillState = SkillState.None;

        BurstFlare burstFlare;
        GaiaTied gaiaTied;
        EterialStorm eterialStorm;
        MeteorStrike meteorStrike;
        RagnaEdge ragnaEdge;
        TerraBreak terraBreak;

        float burstFlareCoolDownTime = 0f;
        float gaiaTiedCoolDownTime = 0f;
        float eterialStormCoolDownTime = 0f;
        float meteorStrikeCoolDownTime = 0f;
        float ragnaEdgeCoolDownTime = 0f;
        float terraBreakCoolDownTime = 0f;

        public GameObject leftHandSpell;
        public GameObject rightHandSpell;

        public GameObject testCube;

        public Material fireMaterial;
        public Material landMaterial;
        public Material stormMaterial;

        //로컬 클라이언트에서 접근
        bool isEterialStorm = false;
        bool isMeteorStrike = false;

        private Vector3 handPoint;

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
                CheckOverlayAnimator();
                CheckPoint();
                CheckSkillOnMine();
            }

            if(PhotonNetwork.IsMasterClient)
            {
                CheckCoolDown();
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
            minimapRenderTexture = minimapCamera.GetComponent<Camera>().targetTexture;
        }

        public override void IsLocalPlayer()
        {
            base.IsLocalPlayer();
            overlayCamera.SetActive(true);
            minimapCamera.SetActive(true);
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

            if(gaiaTiedCoolDownTime > 0f)
            {
                gaiaTiedCoolDownTime -= Time.deltaTime;
            }

            if(eterialStormCoolDownTime > 0f)
            {
                eterialStormCoolDownTime -= Time.deltaTime;
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
                    eterialStorm = new EterialStorm();
                }
                if (isPointStrike == true)
                {
                    object[] _data = new object[5];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "EterialStorm";

                    PhotonNetwork.Instantiate("ChanYoung/Prefabs/Cylinder", pointStrike, Quaternion.identity, data: _data);
                    eterialStormCoolDownTime = eterialStorm.GetSkillCoolDownTime();
                    skillState = SkillState.None;
                    eterialStorm = null;
                    isPointStrike = false;
                }
            }
            else if(skillState == SkillState.MeteorStrike)
            {
                if(meteorStrike == null)
                {
                    meteorStrike = new MeteorStrike();
                    isMeteorStrike = true;
                }
                else
                {
                    if(isPointStrike == true)
                    {
                        object[] _data = new object[5];
                        _data[0] = name;
                        _data[1] = tag;
                        _data[2] = "MeteorStrike";

                        PhotonNetwork.Instantiate("ChanYoung/Prefabs/MeteorStrike", pointStrike, Quaternion.identity, data: _data);
                        meteorStrikeCoolDownTime = meteorStrike.GetSkillCoolDownTime();
                        meteorStrike = null;
                        isPointStrike = false;
                        skillState = SkillState.None;
                    }
                }
            }
        }
        //로컬 클라이언트에서 확인하는 요소
        void CheckSkillOnMine()
        {
            if(skillState == SkillState.EterialStorm)
            {
                if (isEterialStorm == false)
                {
                    minimapCamera.GetComponent<Camera>().targetTexture = null;
                    minimapCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;
                    camera.GetComponent<MouseControl>().enabled = false;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    Debug.Log(isEterialStorm);
                    isEterialStorm = true;
                }  
            }
            else if(skillState == SkillState.MeteorStrike)
            {
                if (isMeteorStrike == true)
                {
                    Vector3 _arrivedGroundVec;
                    Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(Aim.transform.position);
                    RaycastHit _tempRayHit;
                    LayerMask _tempLayerMask = LayerMask.GetMask("Map");
                    MeteorStrike _localMeteorShower = new MeteorStrike();
                    float _maxDistace = _localMeteorShower.maxDistance;
    
                    if (Physics.Raycast(_tempRay, out _tempRayHit, _maxDistace, _tempLayerMask))
                    {
                        Vector3 _hitPoint = _tempRayHit.point - _tempRay.direction * 0.05f;
                        Ray _bottomRay = new Ray(_hitPoint, Vector3.down);
                        RaycastHit _bottomRayHit;
                        if(Physics.Raycast(_bottomRay, out _bottomRayHit, Mathf.Infinity, _tempLayerMask))
                        {
                            _arrivedGroundVec = _bottomRayHit.point;
                            testCube.GetComponent<MeshRenderer>().enabled = true;
                            testCube.transform.position = _arrivedGroundVec;

                        }
                        else
                        {
                            testCube.GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                    else
                    {
                        Vector3 _hitPoint = _tempRay.origin + _tempRay.direction * _maxDistace;
                        Ray _bottomRay = new Ray(_hitPoint, Vector3.down);
                        RaycastHit _bottomRayHit;


                        if (Physics.Raycast(_bottomRay, out _bottomRayHit, Mathf.Infinity, _tempLayerMask))
                        {
                            _arrivedGroundVec = _bottomRayHit.point;
                            testCube.GetComponent<MeshRenderer>().enabled = true;
                            testCube.transform.position = _arrivedGroundVec;

                        }
                        else
                        {
                            testCube.GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                }
            }
            else if(skillState == SkillState.None)
            {
                if(isEterialStorm == true)
                {
                    minimapCamera.GetComponent<Camera>().targetTexture = minimapRenderTexture;
                    minimapCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;

                    camera.GetComponent<MouseControl>().enabled = true;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    isEterialStorm = false;
                }
                else if(isMeteorStrike == true)
                {
                    isMeteorStrike = false;
                    testCube.GetComponent <MeshRenderer>().enabled = false;
                }
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
            else if(skillState == SkillState.MeteorStrike)
            {
                meteorStrike = null;
                isMeteorStrike = false;
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
                        CheckCommands(origin, direction);
                    }
                    else if (((commands[0] == 2 && commands[1] == 3)
                        || (commands[0] == 3 && commands[1] == 2)) && gaiaTiedCoolDownTime <= 0f)
                    {
                        skillState = SkillState.GaiaTied;
                        CheckCommands(origin, direction);
                    }
                    else if (commands[0] == 3 && commands[1] == 3 && eterialStormCoolDownTime <= 0f)
                    {
                        skillState = SkillState.EterialStorm;
                        CheckCommands(origin, direction);
                    }
                    else if (commands[0] == 1 && commands[1] == 1 && meteorStrikeCoolDownTime <= 0f)
                    {
                        skillState = SkillState.MeteorStrike;
                        CheckCommands(origin, direction);
                    }
                }

            }
            else
            {
                UseSkill(origin, direction);
            }
        }

        void CheckCommands(Vector3 origin, Vector3 direction)
        {
            isReadyToUseSkill = false;
            UseSkill(origin, direction);
            commands.Clear();
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
                    //로컬 클라이언트 접근
                    if(isEterialStorm == true)
                    {
                        Ray _tempRay = minimapCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                        RaycastHit _tempHit;
                        int _tempLayerMask = ~(1 << LayerMask.NameToLayer("Minimap"));
                        if (Physics.Raycast(_tempRay, out _tempHit, Mathf.Infinity, _tempLayerMask))
                        {
                            pointStrike = new Vector3(_tempHit.point.x, _tempHit.point.y + 1f, _tempHit.point.z);
                            isPointStrike = true;
                        }
                    }
                    else if(isMeteorStrike == true)
                    {
                        pointStrike = testCube.transform.position;
                        isPointStrike = true;
                    }

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
                stream.SendNext(pointStrike);
                stream.SendNext(isPointStrike);
                stream.SendNext(isMeteorStrike);
                stream.SendNext(arrivedVec);
            }
            else
            {
                networkHandPoint = (Vector3)stream.ReceiveNext();
                networkRightCurrentWeight = (float)stream.ReceiveNext();
                networkLeftCurrentWeight = (float)stream.ReceiveNext();
                skillState = (SkillState)stream.ReceiveNext();
                pointStrike = (Vector3)stream.ReceiveNext();
                isPointStrike = (bool)stream.ReceiveNext();
                isMeteorStrike = (bool)stream.ReceiveNext();
                arrivedVec = (Vector3)stream.ReceiveNext();
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
        }

        protected override void OnAnimatorIK()
        {
            animator.logWarnings = false;
            overlayAnimator.logWarnings = false;    
            base.OnAnimatorIK();


            if (photonView.IsMine)
            {
                if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
                {
                    overlayAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.2f);
                    overlayAnimator.SetIKPosition(AvatarIKGoal.LeftHand, OverlaySight.transform.position);
                    overlayAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.22f);
                    overlayAnimator.SetIKPosition(AvatarIKGoal.RightHand, OverlaySight.transform.position);
                }


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
