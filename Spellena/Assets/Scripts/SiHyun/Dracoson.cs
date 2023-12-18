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
using Unity.VisualScripting;

namespace Player
{
    public enum SkillStateDracoson
    {
        None, DragonSightReady,
        DragonSightHolding, DragonSightAttack,
        Skill1Ready, Skill1Casting, Skill1Channeling,
        Skill2Ready, Skill2Casting, Skill2Channeling,
        Skill3Ready, Skill3Casting, Skill3Holding,
        Skill4Ready, Skill4Casting, 
        Breathe,
        DragonicBreath
    }

    public class Dracoson : Character
    {
        public DracosonData dracosonData;
        public GameObject thirdCamera;
        public GameObject overlaycamera;
        public GameObject minimapCamera;
        public GameObject aim;
        public Animator overlayAnimator;
        public Transform overlaySight;
        public GameObject overlayRightHand;
        public Transform staffTopForMe;
        public Transform staffTopForOther;
        public GameObject dracosonMetamorphose;
        public GameObject breathePoint;

        float rightHandWeight = 0.04f;

        [Range(1, 3)]
        public int projectile = 1;

        [Range(0, 1)]
        public float weight = 0.01f;

        public DragonicBreathe breatheObject;
        public GameObject dragonShield;
        private GameObject currentObjectForMe;
        private GameObject currentObjectForOther;
        public GameObject rangeMark;
        private int previouseChargeCount = 0;

        int shieldId;

        int ownerNum;
        Vector3 defaultCameraLocalVec;

        float dragonicSightHoldingTime0;
        float dragonicSightHoldingTime1;
        float dragonicSightHoldingTime2;
        float dragonicSightHoldingTime3;
        float dragonSightAttackTime;
        float skill1CastingTime;
        float skill1ChannelingTime;
        float skill2CastingTime;
        float skill2ChannelingTime;
        float skill3CastingTime;
        float skill3HoldingTime;
        float skill4CastingTime;
        float skill4DurationTime;
        float dragonicBreatheHoldingTime;

        float dragonSightCollDownTime;
        float skill1CoolDownTime;
        float skill2CoolDownTime;
        float skill3CoolDownTime;
        float skill4CoolDownTime;


        public enum LocalStateDracoson
        {
            None, Metamorphose, ChargePhase0, ChargePhase1, ChargePhase2, ChargePhase3,
        }

        public SkillStateDracoson skillState = SkillStateDracoson.None;
        LocalStateDracoson localState = LocalStateDracoson.None;

        //0 : 스킬1, 1 : 스킬2, 2 : 스킬3, 3 : 스킬4
        public float[] skillCoolDownTime = new float[4];
        float[] skillCastingTime = new float[4];
        bool[] skillCastingCheck = new bool[4];
        float[] skillChannelingTime = new float[4];
        bool[] skillChannelingCheck = new bool[4];

        //0 : 용의 시선 홀딩 0, 1 : 용의 시선 홀딩 1, 2 : 용의 시선 홀딩 2, 3 : 용의 시선 홀딩 3,
        //4 : 용의 시선 공격, 5 : 용의 숨결
        float[] normalCastingTime = new float[6];
        bool[] normalCastingCheck = new bool[6];

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

            dragonicSightHoldingTime0 = dracosonData.dragonicSightHoldingTime0;
            dragonicSightHoldingTime1 = dracosonData.dragonicSightHoldingTime1;
            dragonicSightHoldingTime2 = dracosonData.dragonicSightHoldingTime2;
            dragonicSightHoldingTime3 = dracosonData.dragonicSightHoldingTime3;
            dragonSightAttackTime = dracosonData.dragonSightAttackTime;
            skill1CastingTime = dracosonData.skill1CastingTime;
            skill1ChannelingTime = dracosonData.skill1ChannelingTime;
            skill2CastingTime = dracosonData.skill2CastingTime;
            skill2ChannelingTime = dracosonData.skill2ChannelingTime;
            skill3CastingTime = dracosonData.skill3CastingTime;
            skill3HoldingTime = dracosonData.skill3HoldingTime;
            skill4CastingTime = dracosonData.skill4CastingTime;
            skill4DurationTime = dracosonData.skill4DurationTime;
            dragonicBreatheHoldingTime = dracosonData.dragonicBreatheHoldingTime;

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
            }

            if (photonView.IsMine)
            {
                CheckOnLocalClient();
                CheckAnimationSpeed();
                CheckAnimatorExtra();
                Debug.Log(skillState);
                Debug.Log(localState);
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

            if(skillState == SkillStateDracoson.Breathe)
                CallRPCEvent("BreatheDirection", "Response", camera.transform.rotation);
                
            if (isFly)
            {
                float _currentHeight = transform.position.y;
                float _maxHeight = 15f;
                if(_currentHeight < _maxHeight)
                    rigidbody.AddForce(Vector3.up * 17f, ForceMode.Acceleration);
            }
        }

        void CheckChanneling()
        {

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
            
        }
        void CheckOnLocalClientFixed()
        {
           
        }

        //마스터 클라이언트에서만 작동
        void CheckOnMasterClient()
        {
            if (localState == LocalStateDracoson.Metamorphose)
            {
                if (skillChannelingTime[3] <= 0f && skillChannelingCheck[3])
                {
                    localState = LocalStateDracoson.None;
                    dracosonMetamorphose.SetActive(false);
                    CallRPCEvent("SetAvatar", "Response", "Metamorphose", false);
                    CallRPCEvent("SetAvatar", "Response", "AvatarForMe", true);
                    CallRPCEvent("SetAvatar", "Response", "AvatarForOhter", true);
                    CallRPCEvent("SetAnimator", "Response", "Dracoson");
                    CallRPCEvent("SetCameraPosition", "Response", "Default");
                    isFly = false;
                }
            }
            if (skillState == SkillStateDracoson.DragonSightHolding)
            {
                if (localState == LocalStateDracoson.ChargePhase0)
                {
                    if (normalCastingTime[0] <= 0f && normalCastingCheck[0])
                    {
                        normalCastingCheck[0] = false;
                        localState = LocalStateDracoson.ChargePhase1;
                        CallRPCEvent("InstantiateEffect", "Response", 1);
                        CallRPCEvent("UpdateData", "Response", skillState, "normalCastingTime", 1, dragonicSightHoldingTime1, true);
                    }
                }
                else if (localState == LocalStateDracoson.ChargePhase1)
                {
                    if (normalCastingTime[1] <= 0f && normalCastingCheck[1])
                    {
                        normalCastingCheck[1] = false;
                        localState = LocalStateDracoson.ChargePhase2;
                        CallRPCEvent("InstantiateEffect", "Response", 2);
                        CallRPCEvent("UpdateData", "Response", skillState, "normalCastingTime", 2, dragonicSightHoldingTime2, true);
                    }
                }
                else if (localState == LocalStateDracoson.ChargePhase2)
                {
                    if (normalCastingTime[2] <= 0f && normalCastingCheck[2])
                    {
                        normalCastingCheck[2] = false;
                        localState = LocalStateDracoson.ChargePhase3;
                        CallRPCEvent("InstantiateEffect", "Response", 3);
                        CallRPCEvent("UpdateData", "Response", skillState, "normalCastingTime", 3, dragonicSightHoldingTime3, true);
                    }
                }
                else if (localState == LocalStateDracoson.ChargePhase3)
                {
                    if (normalCastingTime[3] <= 0f && normalCastingCheck[3])
                    {
                        normalCastingCheck[3] = false;
                        skillState = SkillStateDracoson.DragonSightAttack;
                        CallRPCEvent("SetAnimation", "Response", "isDragonSightAttack", true);
                        CallRPCEvent("SetAnimation", "Response", "isDragonSightHolding", false);
                        CallRPCEvent("UpdateData", "Response", skillState, "normalCastingTime", 4, dragonSightAttackTime, true);
                    }
                }
            }
            else if (skillState == SkillStateDracoson.DragonSightAttack)
            {
                if (normalCastingTime[4] <= 0f && normalCastingCheck[4])
                {
                    if (localState == LocalStateDracoson.ChargePhase1)
                        CallRPCEvent("InstantiateObject", "Response", "DragonicFlame", 1);
                    else if (localState == LocalStateDracoson.ChargePhase2)
                        CallRPCEvent("InstantiateObject", "Response", "DragonicFlame", 2);
                    else if (localState == LocalStateDracoson.ChargePhase3)
                        CallRPCEvent("InstantiateObject", "Response", "DragonicFlame", 3);
                    normalCastingCheck[4] = false;
                    skillState = SkillStateDracoson.None;
                    localState = LocalStateDracoson.None;
                    CallRPCEvent("ResetAnimation", "Response");
                    CallRPCEvent("UpdateData", "Response", skillState, "OnlySkillState", 0, 0f, false);
                    CallRPCEvent("DestroyEffect", "Response", "Flame");
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
            else if(skillState == SkillStateDracoson.Skill2Casting)
            {
                if (skillCastingTime[1] <= 0f && skillCastingCheck[1])
                {
                    skillCastingCheck[1] = false;
                    skillState = SkillStateDracoson.Skill2Channeling;
                    CallRPCEvent("SetAnimation", "Response", "isSkill2", true);
                    CallRPCEvent("SetAnimation", "Response", "Skill2Ready", false);
                    CallRPCEvent("UpdateData", "Response", skillState, "skillChannelingTime", 1, skill2ChannelingTime, true);
                    CallRPCEvent("MarkingSkillRange", "Response", "Skill2Range", false);
                }
            }
            else if(skillState == SkillStateDracoson.Skill2Channeling)
            {
                if (skillChannelingTime[1] <= 0f && skillChannelingCheck[1])
                {
                    skillState = SkillStateDracoson.None;
                    CallRPCEvent("InstantiateObject", "Response", "DragonPunch");
                    CallRPCEvent("ResetAnimation", "Response");
                    CallRPCEvent("UpdateData", "Response", skillState, "OnlySkillState", 0, 0f, false);
                    CallRPCEvent("SetCoolDownTime", "Response", 0);
                    CallRPCEvent("PauseControl", "Response", "All", false);
                }
            }
            else if (skillState == SkillStateDracoson.Skill3Casting)
            {
                if (skillCastingTime[2] <= 0f && skillCastingCheck[2])
                {
                    skillCastingCheck[2] = false;
                    skillState = SkillStateDracoson.Skill3Holding;
                    Debug.Log("쉴드 스킬 소환");
                    CallRPCEvent("InstantiateObject", "Response", "DragonShield");
                    CallRPCEvent("UpdateData", "Response", skillState, "skillChannelingTime", 2, skill3HoldingTime, true);
                }
            }
            else if (skillState == SkillStateDracoson.Skill3Holding)
            {
                if (skillChannelingTime[2] <= 0f && skillChannelingCheck[2])
                {
                    skillChannelingCheck[2] = false;
                    skillState = SkillStateDracoson.None;
                    CallRPCEvent("ResetAnimation", "Response");
                    CallRPCEvent("PauseControl", "Response", "OnlyMove", false);
                    CallRPCEvent("SetCamera", "Response", "Default");
                    CallRPCEvent("UpdateData", "Response", skillState, "OnlySkillState", 0, 0f, false);
                    CallRPCEvent("StopEffect", "Response", "DragonShield", shieldId);
                }
            }
            //용가리 변신
            else if (skillState == SkillStateDracoson.Skill4Casting)
            {
                if (skillCastingTime[3] <= 0.3f && skillCastingTime[3] >= 0.28f && skillCastingCheck[3])
                    CallRPCEvent("InstantiateObject", "Response", "MetamorphoseEffect");
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
                    CallRPCEvent("SetAnimator", "Response", "Metamorphose");
                    CallRPCEvent("PauseControl", "Response", "All", false);
                    CallRPCEvent("SetCameraPosition", "Response", "Change");
                }
            }
            else if (skillState == SkillStateDracoson.Breathe)
            {
                if (normalCastingTime[5] <= 0f && normalCastingCheck[5])
                {
                    Debug.Log("브레스 끝남 휴");
                    normalCastingCheck[5] = false;
                    skillState = SkillStateDracoson.None;
                    CallRPCEvent("ResetAnimation", "Response");
                    CallRPCEvent("UpdateData", "Response", skillState, "OnlySkillState", 0, 0f, false);
                    CallRPCEvent("StopEffect", "Response", "Breathe");
                }
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
            {
                CallRPCEvent("CancelSkill", "Request", 0);
                CallRPCEvent("MarkingSkillRange", "Response", "Skill2Range", false);
            }
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
            else if ((string)data[0] == "InstantiateEffect")
                InstantiateEffect(data);
            else if ((string)data[0] == "SetCoolDownTime")
                SetCoolDownTime(data);
            else if ((string)data[0] == "UseSkill")
                UseSkill(data);
            else if ((string)data[0] == "EndSkill")
                EndSkill();
            else if ((string)data[0] == "SetAnimator")
                SetAnimator(data);
            else if ((string)data[0] == "PauseControl")
                PauseControl(data);
            else if ((string)data[0] == "SetCameraPosition")
                SetCameraPosition(data);
            else if ((string)data[0] == "StopEffect")
                StopEffect(data);
            else if ((string)data[0] == "BreatheDirection")
                BreatheDirection(data);
            else if ((string)data[0] == "SetBreatheID")
                SetBreatheID(data);
            else if ((string)data[0] == "SetEffectID")
                SetEffectID(data);
            else if ((string)data[0] == "DestroyEffect")
                DestroyEffect(data);
            else if ((string)data[0] == "SetShieldID")
                SetShieldID(data);
            else if ((string)data[0] == "SetCamera")
                SetCamera(data);
            else if ((string)data[0] == "MarkingSkillRange")
                MarkingSkillRange(data);
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

        void MarkingSkillRange(object[] data)
        {
            if(photonView.IsMine)
            {
                if ((string)data[1] == "Skill2Range")
                {
                    rangeMark.SetActive((bool)data[2]);
                }
            }
        }

        void DestroyEffect(object[] data)
        {
            if ((string)data[1] == "Flame")
            {
                if (currentObjectForMe != null)
                {
                    Destroy(currentObjectForMe);
                }
                if (currentObjectForOther != null)
                {
                    PhotonNetwork.Destroy(currentObjectForOther);
                }
            }
            else if((string)data[1] == "Shield")
            {
                if(dragonShield != null)
                {
                    PhotonNetwork.Destroy(dragonShield);
                }
            }
        }
            void SetEffectID(object[] data)
        {
            currentObjectForOther = PhotonView.Find((int)data[1])?.gameObject;
            currentObjectForOther.layer = 7;
        }

        void SetBreatheID(object[] data)
        {
            breatheObject = PhotonView.Find((int)data[1])?.gameObject.GetComponent<DragonicBreathe>();
            breathePoint = PhotonView.Find((int)data[2])?.gameObject;
        }

        void SetShieldID(object[] data)
        {
            shieldId = (int)data[1];
            dragonShield = PhotonView.Find(shieldId)?.gameObject;
        }

        void BreatheDirection(object[] data)
        {
            if (photonView.IsMine)
            {
                breatheObject.GetComponent<PhotonTransformView>().transform.rotation =
                camera.transform.rotation;
            }
        }

        void StopEffect(object[] data)
        {
            if ((string)data[1] == "Breathe")
            {
                GameObject _gameObject = PhotonView.Find((int)data[2])?.gameObject;
                ParticleSystem _daw = _gameObject.GetComponent<ParticleSystem>();
                _daw.Stop();
            }
            else if ((string)data[1] == "DragonShield")
            {
                GameObject _gameObject = PhotonView.Find((int)data[2])?.gameObject;
                GameObject _daw1 = _gameObject.transform.GetChild(0).gameObject;
                GameObject _daw2 = _gameObject.transform.GetChild(1).gameObject;
                _daw1.SetActive(false);
                _daw2.SetActive(false);
                /*foreach (Transform child in _daw1.transform)
                {
                    ParticleSystem _particleSystem = 
                    child.GetComponent<ParticleSystem>();

                    if(_particleSystem != null)
                    {
                        _particleSystem.Stop();
                    }
                }
                foreach (Transform child in _daw2.transform)
                {
                    ParticleSystem _particleSystem =
                    child.GetComponent<ParticleSystem>();

                    if (_particleSystem != null)
                    {
                        _particleSystem.Stop();
                    }
                }*/
            }
        }

        void SetCamera(object[] data)
        {
            if (photonView.IsMine)
            {
                if ((string)data[1] == "Skill3")
                {
                    thirdCamera.SetActive(true);
                    camera.SetActive(false);
                    overlaycamera.SetActive(false);
                }
                else if ((string)data[1] == "Default")
                {
                    thirdCamera.SetActive(false);
                    camera.SetActive(true);
                    overlaycamera.SetActive(true);
                }
            }
        }

        void SetCameraPosition(object[] data)
        {
            if (photonView.IsMine)
            {
                Debug.Log("카메라 위치 조정");
                if ((string)data[1] == "Change")
                    camera.transform.position = new Vector3(camera.transform.position.x, 3.5f, camera.transform.position.z);
                else if ((string)data[1] == "Default")
                    camera.transform.position = new Vector3(transform.position.x, 1.74f, transform.position.z);
            }
        }

        void PauseControl(object[] data)
        {
            if ((string)data[1] == "OnlyMove")
            {
                if ((bool)data[2])
                {
                    Rigidbody _rigidbody = gameObject.GetComponent<Rigidbody>();
                    _rigidbody.constraints = RigidbodyConstraints.FreezePosition;
                }
                else if (!(bool)data[2])
                {
                    Rigidbody _rigidbody = gameObject.GetComponent<Rigidbody>();
                    _rigidbody.constraints = RigidbodyConstraints.None;
                    _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                }
            }
            else if ((string)data[1] == "All")
            {
                camera.GetComponent<MouseControl>().enabled = !(bool)data[2];
                GetComponent<PlayerInput>().enabled = !(bool)data[2];
            }
        }

        void SetAnimator(object[] data)
        {
            //if (photonView.IsMine)
            {
                if ((string)data[1] == "Metamorphose")
                    animator = dracosonMetamorphose.GetComponent<Animator>();
                else if ((string)data[1] == "Dracoson")
                    animator = GetComponent<Animator>();
            }
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
                        CallRPCEvent("MarkingSkillRange", "Response", "Skill2Range", true);
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
            Debug.Log("마우스 클릭");
            int mouseCode = (int)data[1];
            if(mouseCode == 0)
            {
                if (skillState == SkillStateDracoson.None)
                {
                    if(localState == LocalStateDracoson.None)
                    {
                        Debug.Log("용의 시선 차징");
                        skillState = SkillStateDracoson.DragonSightHolding;
                        localState = LocalStateDracoson.ChargePhase0;
                        CallRPCEvent("SetAnimation", "Response", "isDragonSightHolding", true);
                        CallRPCEvent("UpdateData", "Response", skillState, "normalCastingTime", 0, dragonicSightHoldingTime0, true);
                    }
                    else if (localState == LocalStateDracoson.Metamorphose)
                    {
                        Debug.Log("브레스 피해욧!!");
                        skillState = SkillStateDracoson.Breathe;
                        CallRPCEvent("SetAnimation", "Response", "Breathe", true);
                        CallRPCEvent("UpdateData", "Response", skillState, "normalCastingTime", 5, dragonicBreatheHoldingTime, true);
                        CallRPCEvent("InstantiateObject", "Response", "DragonicBreathe");
                    }
                }
                else if (skillState == SkillStateDracoson.Skill1Ready)
                {
                    Debug.Log("스킬 1 시전");
                    skillState = SkillStateDracoson.Skill1Casting;
                    CallRPCEvent("SetAnimation", "Response", "Skill1Ready", false);
                    CallRPCEvent("SetAnimation", "Response", "isSkill1", true);
                    CallRPCEvent("UpdateData", "Response", skillState, "skillCastingTime", 0, skill1CastingTime, true);
                }
                else if(skillState == SkillStateDracoson.Skill2Ready)
                {
                    Debug.Log("스킬 2 시전");
                    skillState = SkillStateDracoson.Skill2Casting;
                    CallRPCEvent("UpdateData", "Response", skillState, "skillCastingTime", 1, skill1CastingTime, true);
                    CallRPCEvent("PauseControl", "Response", "All", true);
                }
                else if(skillState == SkillStateDracoson.Skill3Ready)
                {
                    Debug.Log("스킬 3 시전");
                    skillState = SkillStateDracoson.Skill3Casting;
                    CallRPCEvent("SetAnimation", "Response", "Skill3Ready", false);
                    CallRPCEvent("SetAnimation", "Response", "isSkill3", true);
                    CallRPCEvent("UpdateData", "Response", skillState, "skillCastingTime", 2, skill3CastingTime, true);
                    CallRPCEvent("SetCamera", "Response", "Skill3");
                    CallRPCEvent("PauseControl", "Response", "OnlyMove", true);
                }
                else if(skillState == SkillStateDracoson.Skill4Ready)
                {
                    Debug.Log("스킬 4 시전");
                    skillState = SkillStateDracoson.Skill4Casting;
                    CallRPCEvent("SetAnimation", "Response", "Skill4Ready", false);
                    CallRPCEvent("SetAnimation", "Response", "isSkill4", true);
                    CallRPCEvent("UpdateData", "Response", skillState, "skillCastingTime", 3, skill4CastingTime, true);
                    CallRPCEvent("InstantiateObject", "Response", "MagicCircle");
                    CallRPCEvent("PauseControl", "Response", "All", true);
                }
            }
        }

        void CancelHolding()
        {
            Debug.Log("홀딩 캔슬");
            if (skillState == SkillStateDracoson.DragonSightHolding)
            {
                if (localState == LocalStateDracoson.ChargePhase0)
                {
                    skillState = SkillStateDracoson.None;
                    localState = LocalStateDracoson.None;
                    normalCastingCheck[0] = false;
                    CallRPCEvent("SetAnimation", "Response", "isDragonSightHolding", false);
                    CallRPCEvent("SetAnimation", "Response", "HoldingCancel", true);
                    CallRPCEvent("ResetAnimation", "Response");
                }
                else if (localState == LocalStateDracoson.ChargePhase1)
                {
                    normalCastingCheck[1] = false;
                    skillState = SkillStateDracoson.DragonSightAttack;
                    CallRPCEvent("SetAnimation", "Response", "isDragonSightAttack", true);
                    CallRPCEvent("SetAnimation", "Response", "isDragonSightHolding", false);
                    CallRPCEvent("UpdateData", "Response", skillState, "normalCastingTime", 4, dragonSightAttackTime, true);
                }
                else if (localState == LocalStateDracoson.ChargePhase2)
                {
                    normalCastingCheck[2] = false;
                    skillState = SkillStateDracoson.DragonSightAttack;
                    CallRPCEvent("SetAnimation", "Response", "isDragonSightAttack", true);
                    CallRPCEvent("SetAnimation", "Response", "isDragonSightHolding", false);
                    CallRPCEvent("UpdateData", "Response", skillState, "normalCastingTime", 4, dragonSightAttackTime, true);

                }
                else if (localState == LocalStateDracoson.ChargePhase3)
                {
                    normalCastingCheck[3] = false;
                    skillState = SkillStateDracoson.DragonSightAttack;
                    CallRPCEvent("SetAnimation", "Response", "isDragonSightAttack", true);
                    CallRPCEvent("SetAnimation", "Response", "isDragonSightHolding", false);
                    CallRPCEvent("UpdateData", "Response", skillState, "normalCastingTime", 4, dragonSightAttackTime, true);
                }
            }
            else if(skillState == SkillStateDracoson.Skill3Holding)
            {
                skillState = SkillStateDracoson.None;
                skillChannelingCheck[2] = false;
                int _shieldViewID = dragonShield.GetPhotonView().ViewID;
                CallRPCEvent("PauseControl", "Response", "OnlyMove", false);
                CallRPCEvent("SetCamera", "Response", "Default");
                CallRPCEvent("StopEffect", "Response", "DragonShield", _shieldViewID);
                CallRPCEvent("ResetAnimation", "Response");
                CallRPCEvent("UpdateData", "Response", skillState, "OnlySkillState", 0, 0f, false);
            }
            else if (skillState == SkillStateDracoson.Breathe)
            {
                skillState = SkillStateDracoson.None;
                Debug.Log("브레스 끝남 휴");
                normalCastingCheck[2] = false;
                int _breatheViewID = breatheObject.gameObject.GetPhotonView().ViewID;
                CallRPCEvent("StopEffect", "Response", "Breathe", _breatheViewID);
                CallRPCEvent("ResetAnimation", "Response");
                CallRPCEvent("UpdateData", "Response", skillState, "OnlySkillState", 0, 0f, false);
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
            Debug.Log(data[1]);
            Debug.Log(data[2]);
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
            //if (photonView.IsMine)
            {
                GameObject _childObject = PhotonView.Find((int)data[1]).gameObject;
                GameObject _parentObject = PhotonView.Find((int)data[2]).gameObject;

                _childObject.transform.SetParent(_parentObject.transform);
            }
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
                else if ((string)data[1] == "DragonSpin")
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
                else if ((string)data[1] == "DragonPunch")
                {
                    Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(aim.transform.position);
                    Quaternion _tempQ = Quaternion.LookRotation(_tempRay.direction);

                    object[] _data = new object[3];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "DragonPunch";

                    PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dragon Punch",
                        _tempRay.origin + _tempRay.direction * 3.5f, _tempQ, data: _data);
                }
                else if ((string)data[1] == "DragonShield")
                {
                    Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(aim.transform.position);
                    Quaternion _tempQ = Quaternion.LookRotation(_tempRay.direction);

                    object[] _data = new object[3];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "DragonShield";
                    GameObject _dragonShield = PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dragonic Shield",
                        transform.position + new Vector3(0, 0.15f,0) + _tempRay.direction * 0.5f, Quaternion.identity, data: _data);

                    int _parentViewID = gameObject.GetPhotonView().ViewID;
                    int _dragonShieldViewID = _dragonShield.GetPhotonView().ViewID;
                    CallRPCEvent("SetParent", "Response", _dragonShieldViewID, _parentViewID);
                    CallRPCEvent("SetShieldID", "Response", _dragonShieldViewID);
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
                else if ((string)data[1] == "MetamorphoseEffect")
                {
                    object[] _data = new object[3];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "MetamorphoseEffect";

                    PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Metamorphose Effect",
                        transform.position, Quaternion.identity, data: _data);
                }
                else if ((string)data[1] == "DragonicBreathe")
                {
                    Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(aim.transform.position);
                    Quaternion _tempQ = Quaternion.LookRotation(_tempRay.direction);

                    object[] _data = new object[3];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "DragonicBreathe";

                    GameObject _breatheobject = PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dragonic Breathe",
                        breathePoint.transform.position + _tempRay.direction * 0.5f,
                        _tempQ, data: _data);
                    breatheObject = _breatheobject.GetComponent<DragonicBreathe>();

                    breatheObject.SetInfo(camera.transform);

                    int _breathePoint = breathePoint.GetPhotonView().ViewID;
                    int _breathe = breatheObject.gameObject.GetPhotonView().ViewID;
                    CallRPCEvent("SetBreatheID", "Response", _breathe, _breathePoint);
                    CallRPCEvent("SetParent", "Response", _breathe, _breathePoint);
                }
            }
        }
        void InstantiateEffect(object[] data)
        {
            if (photonView.IsMine)
            {
                if (currentObjectForMe != null)
                {
                    Destroy(currentObjectForMe);
                }
                if (currentObjectForOther != null)
                {
                    PhotonNetwork.Destroy(currentObjectForOther);
                }

                Quaternion _staffRotationForMe = Quaternion.LookRotation(staffTopForMe.forward);

                GameObject _effectPrefabMe =
                    Resources.Load<GameObject>("SiHyun/Prefabs/Dracoson/Dracoson Charge Effect " + (int)data[1]);

                currentObjectForMe = Instantiate(_effectPrefabMe, staffTopForMe.position, _staffRotationForMe);
                currentObjectForMe.transform.parent = staffTopForMe.transform;
                currentObjectForMe.layer = 8;

                Quaternion _staffRotationForOther = Quaternion.LookRotation(staffTopForOther.forward);
                GameObject _effectPrefabOther = PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dracoson Charge Effect " + (int)data[1],
                    staffTopForOther.position, _staffRotationForOther);
                
                int _effectPrefabOtherID = _effectPrefabOther.GetPhotonView().ViewID;
                int _staffTopForOtherID = staffTopForOther.gameObject.GetPhotonView().ViewID;
                CallRPCEvent("SetEffectID", "Response", _effectPrefabOtherID);
                CallRPCEvent("SetParent", "Response", _effectPrefabOtherID, _staffTopForOtherID);

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
                animator.SetBool("HoldingCancel", false);
                animator.SetBool("isDragonSightHolding", false);
                animator.SetBool("isDragonSightAttack", false);
                animator.SetBool("Breathe", false);

                overlayAnimator.SetBool("isSkill1", false);
                overlayAnimator.SetBool("Skill1Ready", false);
                overlayAnimator.SetBool("isSkill2", false);
                overlayAnimator.SetBool("Skill2Ready", false);
                overlayAnimator.SetBool("isSkill3", false);
                overlayAnimator.SetBool("Skill3Ready", false);
                overlayAnimator.SetBool("isSkill4", false);
                overlayAnimator.SetBool("Skill4Ready", false);
                overlayAnimator.SetBool("HoldingCancel", false);
                overlayAnimator.SetBool("isDragonSightHolding", false);
                overlayAnimator.SetBool("isDragonSightAttack", false);
            }
        }

        void CheckAnimationSpeed()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("DragonSightHolding"))
                SetAnimationSpeed("DragonSightHoldingSpeed", dragonicSightHoldingTime1);
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
                SetAnimationSpeed("Skill3Speed", skill3HoldingTime);

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
            else if(overlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("isKill2"))
                LerpWeight(0);

            overlayAnimator.SetIKPosition(AvatarIKGoal.RightHand, overlaySight.position);
            overlayAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
        }

        void LerpWeight(float weight)
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, weight, Time.deltaTime * 8f);
        }
    }
}
