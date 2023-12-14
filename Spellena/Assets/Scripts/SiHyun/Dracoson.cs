using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Linq;
using HashTable = ExitGames.Client.Photon.Hashtable;
using UnityEditor;
using System.Security.Cryptography;
using UnityEngine.InputSystem;
using System.ComponentModel;

namespace Player
{
    public enum SkillStateDracoson
    {
        None, DragonSightReady,
        DragonSightHolding, DragonSightAttack,
        Skill1Ready, Skill1Casting, Skill1Channeling,
        Skill2Ready, Skill2Casting, Skill2Channeling,
        Skill3Ready, Skill3Casting, Skill3Channeling,
        Skill4Ready, Skill4Casting, 
        Breathe,
        DragonicBreath
    }

    public class Dracoson : Character
    {
        public DracosonData dracosonData;
        public GameObject overlaycamera;
        public GameObject minimapCamera;
        public GameObject aim;
        public Animator overlayAnimator;
        public Transform overlaySight;
        public GameObject overlayRightHand;
        public Transform staffTopForMe;
        public Transform staffTopForOther;
        public GameObject dracosonMetamorphose;

        float rightHandWeight = 0.04f;

        [Range(1, 3)]
        public int projectile = 1;

        [Range(0, 1)]
        public float weight = 0.01f;

        private GameObject currentObjectForMe;
        private GameObject currentObjectForOther;
        private int previouseChargeCount = 0;

        int ownerNum;
        Vector3 defaultCameraLocalVec;

        float dragonSightHoldingTime;
        float dragonSightAttackTime;
        float skill1CastingTime;
        float skill1ChannelingTime;
        float skill2CastingTime;
        float skill2ChannelingTime;
        float skill3CastingTime;
        float skill3ChannelingTime;
        float skill4CastingTime;
        float skill4DurationTime;

        float dragonSightCollDownTime;
        float skill1CoolDownTime;
        float skill2CoolDownTime;
        float skill3CoolDownTime;
        float skill4CoolDownTime;


        public enum LocalStateDracoson
        {
            None, Metamorphose
        }

        public SkillStateDracoson skillState = SkillStateDracoson.None;
        LocalStateDracoson localState = LocalStateDracoson.None;

        //0 : 스킬1, 1 : 스킬2, 2 : 스킬3, 3 : 스킬4
        public float[] skillCoolDownTime = new float[4];
        float[] skillCastingTime = new float[4];
        bool[] skillCastingCheck = new bool[4];
        float[] skillChannelingTime = new float[4];
        bool[] skillChannelingCheck = new bool[4];

        //0 : 용의 시선 홀딩, 1 : 용의 시선 공격, 2 : 용의 숨결
        float[] normalCastingTime = new float[3];
        bool[] normalCastingCheck = new bool[3];

        //0 : 왼쪽 마우스, 1 : 오른쪽 마우스
        bool[] isClicked = new bool[2];

        bool isFly = false;

        Vector3 aimPos;
        Vector3 aimDirection;

        protected override void Awake()
        {
            base.Awake();
            if (photonView.IsMine)
            {
                //테스트 정보
                HashTable _tempTable = new HashTable();
                _tempTable.Add("CharacterViewID", photonView.ViewID);
                _tempTable.Add("IsAlive", true);
                PhotonNetwork.LocalPlayer.SetCustomProperties(_tempTable);

                object[] _tempData = new object[2];
                _tempData[0] = "SetOwnerNum";
                _tempData[1] = photonView.OwnerActorNr;
                RequestRPCCall(_tempData);
            }
        }

        protected override void Start()
        {
            base.Start();
            Init();
            dracosonMetamorphose.SetActive(false);
        }

        void Init()
        {
            defaultCameraLocalVec = camera.transform.localPosition;
            dataHp = dracosonData.hp;
            hp = dracosonData.hp;
            moveSpeed = dracosonData.moveSpeed;
            backSpeed = dracosonData.backSpeed;
            sideSpeed = dracosonData.sideSpeed;
            runSpeedRatio = dracosonData.runSpeedRatio;
            sitSpeed = dracosonData.sitSpeed;
            sitSideSpeed = dracosonData.sitSideSpeed;
            sitBackSpeed = dracosonData.sitBackSpeed;
            jumpHeight = dracosonData.jumpHeight;
            headShotRatio = dracosonData.headShotRatio;

            dragonSightHoldingTime = dracosonData.dragonSightHoldingTime;
            dragonSightAttackTime = dracosonData.dragonSightAttackTime;
            skill1CastingTime = dracosonData.skill1CastingTime;
            skill1ChannelingTime = dracosonData.skill1ChannelingTime;
            skill2CastingTime = dracosonData.skill2CastingTime;
            skill2ChannelingTime = dracosonData.skill2ChannelingTime;
            skill3CastingTime = dracosonData.skill3CastingTime;
            skill3ChannelingTime = dracosonData.skill3ChannelingTime;
            skill4CastingTime = dracosonData.skill4CastingTime;
            skill4DurationTime = dracosonData.skill4DurationTime;

            dragonSightCollDownTime = dracosonData.dragonSightCoolDownTime;
            skill1CoolDownTime = dracosonData.skill1CoolDownTime;
            skill2CoolDownTime = dracosonData.skill2CoolDownTime;
            skill3CoolDownTime = dracosonData.skill3CoolDownTime;
            skill4CoolDownTime = dracosonData.skill4CoolDownTime;
        }

        [PunRPC]
        public override void IsLocalPlayer()
        {
            base.IsLocalPlayer();
            overlaycamera.SetActive(true);
            minimapCamera.SetActive(true);
            ChangeLayerRecursively(overlayRightHand.transform, 8);
            Transform avatarForOtherRoot = transform.GetChild(0).GetChild(0).GetChild(1);
            avatarForOtherRoot.GetComponentInChildren<MeshRenderer>().transform.gameObject.layer = 6;
            //avatarForOtherRoot.GetComponentInChildren<MeshRenderer>().enabled = false;
            Transform dracosonMetamorphose = transform.GetChild(0).GetChild(2);
            ChangeLayerRecursively(dracosonMetamorphose, 6);
        }

        void ChangeLayerRecursively(Transform targetTransform, int layerNum)
        {
            targetTransform.gameObject.layer = layerNum;

            foreach (Transform child in targetTransform)
            {
                ChangeLayerRecursively(child, layerNum);
            }
        }


        protected override void Update()
        {
            base.Update();
            if (PhotonNetwork.IsMasterClient)
            {
                CheckOnMasterClient();
                Debug.Log(skillState);
            }

            if (photonView.IsMine)
            {
                CheckOnLocalClient();
                CheckAnimationSpeed();
                CheckAnimatorExtra();
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (photonView.IsMine)
            {
                CheckOnLocalClientFixed();
                CheckChanneling();
            }
            CheckCoolDownTimeForAll();
                
            if(isFly)
            {
                float _currentHeight = transform.position.y;
                float _maxHeight = 15f;
                if(_currentHeight < _maxHeight)
                    rigidbody.AddForce(Vector3.up * 17f, ForceMode.Acceleration);
            }
        }

        void CheckChanneling()
        {
            if (skillState == SkillStateDracoson.Skill2Channeling
                || skillState == SkillStateDracoson.Skill4Casting || skillState == SkillStateDracoson.Skill3Casting)
            {
                moveVec = Vector3.zero;
                if (skillState == SkillStateDracoson.Skill2Channeling)
                {
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }
            }

            if (skillState == SkillStateDracoson.DragonSightHolding)
            {
                moveVec = Vector3.zero;
                rigidbody.MovePosition(rigidbody.transform.position + transform.forward * moveSpeed * runSpeedRatio * Time.deltaTime * 1.5f);
                animator.SetInteger("VerticalSpeed", 1);
                animator.SetInteger("HorizontalSpeed", 0);
            }

        }

        //모든 클라이언트에서 작동
        void CheckCoolDownTimeForAll()
        {
            CheckCoolDownTimeLoop(ref skillCoolDownTime);
            CheckCoolDownTimeLoop(ref skillCastingTime);
            CheckCoolDownTimeLoop(ref skillChannelingTime);
            CheckCoolDownTimeLoop(ref normalCastingTime);
        }

        void CheckCoolDownTimeLoop(ref float[] times)
        {
            for (int i = 0; i < times.Length; i++)
            {
                if (times[i] > 0f)
                    times[i] -= Time.deltaTime;
            }
        }

        void CheckOnLocalClient()
        {
            /*if (localState == LocalStateDracoson.Skill2 && phlegmHorror != null)
            {
            }
            //이건 나중에 캐릭터 클래스로 이동 시킨다.
            //여기 까지*/
        }
        void CheckOnLocalClientFixed()
        {
            /*if (localState == LocalStateDracoson.Skill2 && phlegmHorror != null)
            {
                phlegmHorror.GetComponent<Rigidbody>().MovePosition(phlegmHorror.GetComponent<Rigidbody>().transform.position +
                 Time.deltaTime * camera.transform.forward * moveSpeed * 1.4f);
            }*/
        }

        //마스터 클라이언트에서만 작동
        void CheckOnMasterClient()
        {
            Debug.Log(skillChannelingTime[0]);
            if (skillState == SkillStateDracoson.DragonSightHolding)
            {
                if (normalCastingTime[0] <= 0f && normalCastingCheck[0])
                {
                    normalCastingCheck[0] = false;
                }
            }
            else if (skillState == SkillStateDracoson.DragonSightAttack)
            {
                if (normalCastingTime[1] <= 0f && normalCastingCheck[1])
                {
                    normalCastingCheck[1] = false;
                    skillState = SkillStateDracoson.None;
                    if (chargeCount == 1)
                        CallRPCEvent("InstantiateObject", "Response", "DragonicFlame", 1);
                    else if (chargeCount == 2)
                        CallRPCEvent("InstantiateObject", "Response", "DragonicFlame", 2);
                    else if (chargeCount == 3)
                        CallRPCEvent("InstantiateObject", "Response", "DragonicFlame", 3);
                    CallRPCEvent("ResetAnimation", "Response");
                    CallRPCEvent("UpdateData", "Response", skillState, "OnlySkillState", 0, 0f, false);
                }
            }
            else if (skillState == SkillStateDracoson.Skill1Casting)
            {
                if (skillCastingTime[0] <= 0f && skillCastingCheck[0])
                {
                    skillCastingCheck[0] = false;
                    skillState = SkillStateDracoson.Skill1Channeling;
                    CallRPCEvent("InstantiateObject", "Response", "DragonSpin");
                    CallRPCEvent("UpdateData", "Response", skillState, "skillChannelingTime", 0, skill1ChannelingTime, true);
                }
            }
            else if (skillState == SkillStateDracoson.Skill1Channeling)
            {
                if (skillChannelingTime[0] <= 0f && skillChannelingCheck[0])
                {
                    Debug.Log("채널링 끝남");
                    skillState = SkillStateDracoson.None;
                    CallRPCEvent("ResetAnimation", "Response");
                    CallRPCEvent("UpdateData", "Response", skillState, "OnlySkillState", 0, 0f, false);
                    CallRPCEvent("SetCoolDownTime", "Response", 0);
                }
            }
            else if (skillState == SkillStateDracoson.Skill4Casting)
            {
                if (skillCastingTime[3] <= 0f && skillCastingCheck[3])
                {
                    skillState = SkillStateDracoson.None;
                    localState = LocalStateDracoson.Metamorphose;
                    CallRPCEvent("UpdateData", "Response", skillState, "skillChannelingTime", 3, skill4DurationTime, true);
                    dracosonMetamorphose.SetActive(true);
                    CallRPCEvent("ResetAnimation", "Response");
                    CallRPCEvent("SetAvatar", "Response", "Metamorphose", true);
                    CallRPCEvent("SetAvatar", "Response", "AvatarForMe", false);
                    CallRPCEvent("SetAvatar", "Response", "AvatarForOhter", false);
                    CallRPCEvent("InstantiateObject", "Response", "MetamorphoseEffect");
                    camera.GetComponent<MouseControl>().enabled = true;
                    GetComponent<PlayerInput>().enabled = true;
                    animator = dracosonMetamorphose.GetComponent<Animator>();
                }
            }
            else if (localState == LocalStateDracoson.Metamorphose)
            {
                if (skillChannelingTime[3] <= 0f && skillChannelingCheck[3])
                {
                    localState = LocalStateDracoson.None;
                    dracosonMetamorphose.SetActive(false);
                    CallRPCEvent("SetAvatar", "Response", "Metamorphose", false);
                    CallRPCEvent("SetAvatar", "Response", "AvatarForMe", true);
                    CallRPCEvent("SetAvatar", "Response", "AvatarForOhter", true);
                    animator = GetComponent<Animator>();
                    isFly = false;
                }
            }
            else if (skillState == SkillStateDracoson.Breathe)
            {

            }
        }

        protected override void OnJump()
        {
            if (localState != LocalStateDracoson.Metamorphose)
            {
                base.OnJump();
            }
        }

        void OnFly()
        {
            if (localState == LocalStateDracoson.Metamorphose)
            {
                if (!isFly)
                    Debug.Log("점프키 누름");
                else
                    Debug.Log("점프키 뗌");
                isFly = !isFly;
            }
        }

        void OnSkill1()
        {
            if (photonView.IsMine)
            {
                CallRPCEvent("SetSkill", "Request", 1);
                CallRPCEvent("SetAnimation", "Response", "Skill1Ready", true);
            }
        }

        void OnSkill2()
        {
            if (photonView.IsMine)
            {
                CallRPCEvent("SetSkill", "Request", 2);
                CallRPCEvent("SetAnimation", "Response", "Skill2Ready", true);
            }
        }

        void OnSkill3()
        {
            if (photonView.IsMine)
            {
                CallRPCEvent("SetSkill", "Request", 3);
                CallRPCEvent("SetAnimation", "Response", "Skill3Ready", true);
            }
        }

        void OnSkill4()
        {
            if (photonView.IsMine)
            {
                CallRPCEvent("SetSkill", "Request", 4);
                CallRPCEvent("SetAnimation", "Response", "Skill4Ready", true);
            }

        }

        void OnMouseButton()
        {
            if (photonView.IsMine)
            {
                if (!isClicked[0])
                    CallRPCEvent("ClickMouse", "Request", 0);
                else
                    CallRPCEvent("CancelHolding", "Request");
                isClicked[0] = !isClicked[0];
            }
        }

        void OnMouseButton2()
        {
            if (photonView.IsMine)
            {
                if (!isClicked[1])
                    CallRPCEvent("ClickMouse", "Request", 1);
                isClicked[1] = !isClicked[1];
            }
        }

        void OnButtonCancel()
        {
            if (photonView.IsMine)
                CallRPCEvent("CancelSkill", "Request", 0);
        }

        void RequestRPCCall(object[] data)
        {
            photonView.RPC("CallRPCDracosonMasterClient", RpcTarget.MasterClient, data);
        }

        [PunRPC]
        public void CallRPCDracosonMasterClient(object[] data)
        {
            if ((string)data[0] == "SetSkill")
                SetSkill(data);
            else if ((string)data[0] == "ClickMouse")
                ClickMouse(data);
            else if ((string)data[0] == "CancelSkill")
                CancelSkill();
            else if ((string)data[0] == "CancelHolding")
                CancelHolding();
            else if ((string)data[0] == "SetChargeCount")
                SetChargePhase(data);
            else if ((string)data[0] == "SetOwnerNum")
                ResponseRPCCall(data);
            /*else if ((string)data[0] == "ProgressSkillLogic")
                ProgressSkillLogic(data);*/
        }

        void ResponseRPCCall(object[] data)
        {
            photonView.RPC("CallRPCDracosonToAll", RpcTarget.AllBuffered, data);
        }

        [PunRPC]
        public void CallRPCDracosonToAll(object[] data)
        {
            if ((string)data[0] == "UpdateData")
                UpdateData(data);
            if ((string)data[0] == "SetAnimation")
                SetAnimation(data);
            else if ((string)data[0] == "SetParent")
                SetParent(data);
            else if ((string)data[0] == "SetOwnerNum")
                SetOwnerNum(data);
            else if ((string)data[0] == "SetChargePhase")
                SetChargePhase(data);
            else if ((string)data[0] == "SetAvatar")
                SetAvatar(data);
            else if ((string)data[0] == "ResetAnimation")
                ResetAnimation();
            else if ((string)data[0] == "InstantiateObject")
                InstantiateObject(data);
            else if ((string)data[0] == "SetCoolDownTime")
                SetCoolDownTime(data);
            else if ((string)data[0] == "UseSkill")
                UseSkill(data);
            else if ((string)data[0] == "EndSkill")
                EndSkill();
        }

        //RPC 요청
        void CallRPCEvent(string command, string type, params object[] parameters)
        {
            object[] _sendData;
            if (parameters.Length >= 1)
            {
                _sendData = new object[parameters.Length + 1];
                for (int i = 0; i < parameters.Length; i++)
                {
                    _sendData[i + 1] = parameters[i];
                }
            }
            else
                _sendData = new object[2];
            _sendData[0] = command;

            if (type == "Request")
                RequestRPCCall(_sendData);
            else if (type == "Response")
                ResponseRPCCall(_sendData);
        }

        void SetSkill(object[] data)
        {
            int _skillNum = (int)data[1];

            if (skillCoolDownTime[_skillNum - 1] <= 0f)
            {
                if(skillState == SkillStateDracoson.Skill1Ready ||
                   skillState == SkillStateDracoson.Skill2Ready ||
                   skillState == SkillStateDracoson.Skill3Ready ||
                   skillState == SkillStateDracoson.Skill4Ready ||
                   skillState == SkillStateDracoson.None ||
                   (skillState == SkillStateDracoson.DragonSightReady && normalCastingTime[0] <= 0f))
                {
                    if (_skillNum == 1)
                    {
                        skillState = SkillStateDracoson.Skill1Ready;
                    }
                    else if (_skillNum == 2)
                    {
                        skillState = SkillStateDracoson.Skill2Ready;
                    }
                    else if (_skillNum == 3)
                    {
                        skillState = SkillStateDracoson.Skill3Ready;

                    }
                    else if (_skillNum == 4)
                        if (ultimateCount >= -1000)
                        {
                            skillState = SkillStateDracoson.Skill4Ready;
                        }
                }
            }
        }

        void ClickMouse(object[] data)
        {
            // 0은 왼쪽 1은 오른쪽
            int mouseCode = (int)data[1];
            if(mouseCode == 0)
            {
                if (skillState == SkillStateDracoson.None)
                {
                    /*skillState = SkillStateDracoson.DragonSightHolding;
                    CallRPCEvent("SetAnimation", "Response", "isDragonSightHolding", true);
                    CallRPCEvent("UpdateData", "Response", skillState, "normalCastingTime", 1, dragonSightHoldingTime, true);
                    CallRPCEvent("SetChargePhase", "Response", true);*/
                }
                else if (skillState == SkillStateDracoson.Skill1Ready)
                {
                    Debug.Log("스킬 1 시전");
                    skillState = SkillStateDracoson.Skill1Casting;
                    CallRPCEvent("SetAnimation", "Response", "Skill1Ready", false);
                    CallRPCEvent("SetAnimation", "Response", "isSkill1", true);
                    CallRPCEvent("UpdateData", "Response", skillState, "skillCastingTime", 0, skill1CastingTime, true);
                }
                else if(skillState == SkillStateDracoson.Skill4Ready)
                {
                    Debug.Log("스킬 4 시전");
                    skillState = SkillStateDracoson.Skill4Casting;
                    CallRPCEvent("SetAnimation", "Response", "Skill4Ready", false);
                    CallRPCEvent("SetAnimation", "Response", "isSkill4", true);
                    CallRPCEvent("UpdateData", "Response", skillState, "skillCastingTime", 3, skill4CastingTime, true);
                    CallRPCEvent("InstantiateObject", "Response", "MagicCircle");
                    camera.GetComponent<MouseControl>().enabled = false;
                    GetComponent<PlayerInput>().enabled = false;
                }
                else if(localState == LocalStateDracoson.Metamorphose)
                {
                    Debug.Log("브레스 피해욧!!");
                }
            }
        }

        void CancelSkill()
        {
            if (skillState == SkillStateDracoson.Skill1Ready || skillState == SkillStateDracoson.Skill2Ready ||
                skillState == SkillStateDracoson.Skill3Ready || skillState == SkillStateDracoson.Skill4Ready ||
               (skillState == SkillStateDracoson.DragonSightReady && normalCastingTime[0] <= 0f))
            {
                skillState = SkillStateDracoson.None;
                CallRPCEvent("UpdateData", "Response", skillState, "OnlySkillState", 0, 0f, true);
                CallRPCEvent("ResetAnimation", "Response");
            }
        }

        void UpdateData(object[] data)
        {
            skillState = (SkillStateDracoson)data[1];

            string _timeType = (string)data[2];
            int _index = (int)data[3];
            float _newTime = (float)data[4];
            bool _onceChecker = (bool)data[5];

            if (_timeType == "normalCastingTime")
            {
                normalCastingTime[_index] = _newTime;
                normalCastingCheck[_index] = _onceChecker;
            }
            else if (_timeType == "skillCastingTime")
            {
                skillCastingTime[_index] = _newTime;
                skillCastingCheck[_index] = _onceChecker;
            }
            else if (_timeType == "skillChannelingTime")
            {
                skillChannelingTime[_index] = _newTime;
                skillCastingCheck[_index] = false;
                skillChannelingCheck[_index] = _onceChecker;
            }
            else if (_timeType == "OnlySkillState")
            {
                for (int i = 0; i < normalCastingCheck.Length; i++)
                    normalCastingCheck[i] = false;
                for (int i = 0; i < skillCastingCheck.Length; i++)
                    skillCastingCheck[i] = false;
                for (int i = 0; i < skillChannelingCheck.Length; i++)
                    skillChannelingCheck[i] = false;
            }
        }

        void SetCoolDownTime(object[] data)
        {
            int _index = (int)data[1];

            if (_index == 0)
                skillCoolDownTime[_index] = skill1CoolDownTime;
            else if (_index == 1)
                skillCoolDownTime[_index] = skill2CoolDownTime;
            else if (_index == 2)
                skillCoolDownTime[_index] = skill3CoolDownTime;
            else if (_index == 3)
                skillCoolDownTime[_index] = skill4CoolDownTime;
        }

        void SetAvatar(object[] data)
        {
            bool _isActive = (bool)data[2];
            if((string)data[1] == "Metamorphose")
                dracosonMetamorphose.SetActive(_isActive);
            else if ((string)data[1] == "AvatarForMe")
                transform.GetChild(0).GetChild(0).gameObject.SetActive(_isActive);
            else if ((string)data[1] == "AvatarForOhter")
                transform.GetChild(0).GetChild(1).gameObject.SetActive(_isActive);
        }

        void EndSkill()
        {
            if(photonView.IsMine)
            {
                /*if(localState == LocalStateDracoson.Breathe)
                {
                    localState = LocalStateDracoson.None;
                    dracosonMetamorphose.SetActive(false);
                }*/
            }
        }
        void UseSkill(object[] data)
        {
            if (photonView.IsMine)
            {
                /*int _index = (int)data[1];
                if (_index == 0)//스킬 1 사용
                    UseSkill1();
                else if (_index == 1)
                    UseSkill2();*/
            }
        }

        void SetAnimation(object[] data)
        {
            if (photonView.IsMine)
            {
                animator.SetBool((string)data[1], (bool)data[2]);
                overlayAnimator.SetBool((string)data[1], (bool)data[2]);
            }
        }

        void SetOwnerNum(object[] data)
        {
            ownerNum = (int)data[1];
        }

        void SetParent(object[] data)
        {
            GameObject _dragonSpin = PhotonView.Find((int)data[1]).gameObject;
            GameObject _parentObject = PhotonView.Find((int)data[2]).gameObject;

            _dragonSpin.transform.SetParent(_parentObject.transform);
        }

        void InstantiateObject(object[] data)
        {
            if (photonView.IsMine)
            {
                if ((string)data[1] == "DragonicFlame")
                {
                    Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(aim.transform.position);
                    Quaternion _tempQ = Quaternion.LookRotation(_tempRay.direction);

                    object[] _data = new object[3];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "DragonicFlame";

                    PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dragonic Flame Projectile " + data[2],
                        _tempRay.origin + _tempRay.direction * 0.5f, _tempQ, data: _data);

                    chargeCount = 0;
                }
                else if((string)data[1] == "DragonSpin")
                {
                    Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(aim.transform.position);
                    Quaternion _tempQ = Quaternion.LookRotation(_tempRay.direction);

                    object[] _data = new object[3];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "DragonSpin";
                    GameObject _dragonSpin = PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/DragonSpin",
                        transform.position + _tempRay.direction * 0.5f, Quaternion.identity, data: _data);

                    int _parentViewID = gameObject.GetPhotonView().ViewID;
                    int _dragonSpinViewID = _dragonSpin.GetPhotonView().ViewID;
                    CallRPCEvent("SetParent", "Response", _dragonSpinViewID, _parentViewID);
                }
                else if ((string)data[1] == "MagicCircle")
                {
                    Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(aim.transform.position);

                    object[] _data = new object[3];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "MagicCircle";

                    PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dracoson Magic Circle",
                        transform.position, Quaternion.Euler(-90, 0, 0), data: _data);
                }
                else if((string)data[1] == "MetamorphoseEffect")
                {
                    object[] _data = new object[3];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "MetamorphoseEffect";

                    PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Metamorphose Effect",
                        transform.position, Quaternion.identity, data: _data);
                }
            }
        }

        void InstantiateChargeEffect(int chargePhase)
        {
            if (photonView.IsMine)
            {
                if (currentObjectForMe != null)
                {
                    PhotonNetwork.Destroy(currentObjectForMe);
                }
                if (currentObjectForOther != null)
                {
                    PhotonNetwork.Destroy(currentObjectForOther);
                }

                Quaternion _staffRotationForMe = Quaternion.LookRotation(staffTopForMe.forward);

                GameObject _effectPrefab =
                    Resources.Load<GameObject>("SiHyun/Prefabs/Dracoson/Dracoson Charge Effect " + chargePhase);

                currentObjectForMe = Instantiate(_effectPrefab, staffTopForMe.position, _staffRotationForMe);
                currentObjectForMe.transform.parent = staffTopForMe.transform;
                currentObjectForMe.layer = 8;

                Quaternion _staffRotationForOther = Quaternion.LookRotation(staffTopForOther.forward);
                currentObjectForOther = PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dracoson Charge Effect " + chargePhase,
                    staffTopForOther.position, _staffRotationForOther);
                currentObjectForOther.transform.parent = staffTopForOther.transform;
                currentObjectForOther.layer = 6;
            }
        }

        void CancelHolding()
        {
            //홀딩 캔슬
            Debug.Log("홀딩 캔슬");
            if(skillState == SkillStateDracoson.DragonSightHolding)
            {
                /*skillState = SkillStateDracoson.DragonSightAttack;
                CallRPCEvent("SetAnimation", "Response", "isDragonSightHolding", false);
                CallRPCEvent("SetAnimation", "Response", "isDragonSightAttack", true);
                CallRPCEvent("UpdateData", "Response", skillState, "OnlySkillState", 0, 0f, true);*/
            }
        }

        void SetChargePhase(object[] data)
        {

        }

        void CallSetAnimation(string parameter, bool isParameter)
        {
            object[] _tempData = new object[3];
            _tempData[0] = "SetAnimation";
            _tempData[1] = parameter;
            _tempData[2] = isParameter;
            ResponseRPCCall(_tempData);
        }

        void ResetAnimation()
        {
            if (photonView.IsMine)
            {
                animator.SetBool("isSkill1", false);
                animator.SetBool("Skill1Ready", false);
                animator.SetBool("isSkill2", false);
                animator.SetBool("Skill2Ready", false);
                animator.SetBool("isSkill3", false);
                animator.SetBool("Skill3Ready", false);
                animator.SetBool("isSkill4", false);
                animator.SetBool("Skill4Ready", false);
                animator.SetBool("isDragonSightHolding", false);
                animator.SetBool("isDragonSightAttack", false);
                animator.SetBool("isInvocation", false);


                overlayAnimator.SetBool("isSkill1", false);
                overlayAnimator.SetBool("Skill1Ready", false);
                overlayAnimator.SetBool("isSkill2", false);
                overlayAnimator.SetBool("Skill2Ready", false);
                overlayAnimator.SetBool("isSkill3", false);
                overlayAnimator.SetBool("Skill3Ready", false);
                overlayAnimator.SetBool("isSkill4", false);
                overlayAnimator.SetBool("Skill4Ready", false);
                overlayAnimator.SetBool("isDragonSightHolding", false);
                overlayAnimator.SetBool("isDragonSightAttack", false);
                overlayAnimator.SetBool("isInvocation", false);
                

            }
        }

        void CheckAnimationSpeed()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("DragonSightHolding"))
                SetAnimationSpeed("DragonSightHoldingSpeed", dragonSightHoldingTime);
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("DragonSightAttack"))
                SetAnimationSpeed("DragonSightAttackSpeed", dragonSightAttackTime);
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill1Casting"))
                SetAnimationSpeed("Skill1CastingSpeed", skill1CastingTime);
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill1"))
                SetAnimationSpeed("Skill1Speed", skill1ChannelingTime);
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill2"))
                SetAnimationSpeed("Skill2CastingSpeed", skill2CastingTime);
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill3Casting"))
                SetAnimationSpeed("Skill3CastingSpeed", skill3CastingTime);
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Skill3"))
                SetAnimationSpeed("Skill3Speed", skill3ChannelingTime);

            if (animator.GetCurrentAnimatorStateInfo(4).IsName("Skill4"))
                SetAnimationSpeedExtra("Skill4CastingSpeed", skill4CastingTime);
        }


        void SetAnimationSpeed(string state, float animationTime)
        {
            float _beforeSpeed = animator.GetCurrentAnimatorStateInfo(0).speed;
            float _animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            float _normalizedSpeed = _animationLength / animationTime;
            if (_normalizedSpeed != 1 && _normalizedSpeed != _beforeSpeed)
            {
                animator.SetFloat(state, _normalizedSpeed);
                overlayAnimator.SetFloat(state, _normalizedSpeed);
            }
        }
        void SetAnimationSpeedExtra(string state, float animationTime)
        {
            float _beforeSpeed = animator.GetCurrentAnimatorStateInfo(4).speed;
            float _animationLength = animator.GetCurrentAnimatorStateInfo(4).length;
            float _normalizedSpeed = _animationLength / animationTime;
            if (_normalizedSpeed != 1 && _normalizedSpeed != _beforeSpeed)
            {
                animator.SetFloat(state, _normalizedSpeed);
                overlayAnimator.SetFloat(state, _normalizedSpeed);
            }
        }

        void CheckAnimatorExtra()
        {
        }
        protected override void OnAnimatorIK()
        {
            base.OnAnimatorIK();

            if (overlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                LerpWeight(weight);
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("isHolding"))
                LerpWeight(weight);

            overlayAnimator.SetIKPosition(AvatarIKGoal.RightHand, overlaySight.position);
            overlayAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
        }

        void LerpWeight(float weight)
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, weight, Time.deltaTime * 8f);
        }
    }
}
