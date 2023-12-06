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
        None,
        DragonSightHolding, DragonSightAttack,
        Skill1Ready, Skill1Casting, Skill1Channeling,
        Skill2Ready, Skill2Casting, Skill2Channeling,
        Skill3Ready, Skill3Casting, Skill3Channeling,
        Skill4Ready, Skill4Casting,
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
        public float weight = 0.5f;

        private GameObject currentObjectForMe;
        private GameObject currentObjectForOther;
        private int previouseChargeCount = 0;

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

        float dragonSightCollDownTime;
        float skill1CoolDownTime;
        float skill2CoolDownTime;
        float skill3CoolDownTime;
        float skill4CoolDownTime;


        public enum LocalStateDracoson
        {
            None, Skill2
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
            if (dracosonMetamorphose.activeSelf && dracosonMetamorphose != null)
            {
                animator = dracosonMetamorphose.GetComponent<Animator>();
            }
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
            ChangeLayerRecursively(overlayRightHand.transform);
            Transform avatarForOtherRoot = transform.GetChild(0).GetChild(0).GetChild(1);//다른 사람들이 보는 자신의 아바타
            avatarForOtherRoot.GetComponentInChildren<MeshRenderer>().transform.gameObject.layer = 6;
            //avatarForOtherRoot.GetComponentInChildren<MeshRenderer>().enabled = false;
        }

        void ChangeLayerRecursively(Transform targetTransform)
        {
            targetTransform.gameObject.layer = 8;

            foreach (Transform child in targetTransform)
            {
                ChangeLayerRecursively(child);
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
                /*CheckAnimationSpeed();
                CheckAnimatorExtra();*/
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

        void CheckOnMasterClient()
        {
            if(skillState == SkillStateDracoson.DragonSightHolding)
            {
                if(normalCastingTime[1] <= 0f && normalCastingCheck[1])
                {

                }
            }
        }
        


        void OnSkill1()
        {
            if (photonView.IsMine)
                CallSkill(1);
        }

        void OnSkill2()
        {
            if (photonView.IsMine)
                CallSkill(2);
        }

        void OnSkill3()
        {
            if (photonView.IsMine)
                CallSkill(3);
        }

        void OnSkill4()
        {
            if (photonView.IsMine)
                CallSkill(4);
        }

        void CallSkill(int num)
        {
            object[] _tempObject = new object[2];
            _tempObject[0] = "SetSkill";
            _tempObject[1] = num;
            RequestRPCCall(_tempObject);
        }

        void OnButtonCancel()
        {
            if (photonView.IsMine)
            {
                object[] _tempData = new object[2];
                _tempData[0] = "CancelSkill";
                RequestRPCCall(_tempData);
            }
        }

        void OnMouseButton()
        {
           
        }

        //마스터 클라이언트로 요청
        void RequestRPCCall(object[] data)
        {
            photonView.RPC("CallRPCCultistMasterClient", RpcTarget.MasterClient, data);
            Debug.Log("RPC 쏘는중");
        }

        [PunRPC]
        public void CallRPCCultistMasterClient(object[] data)
        {
            if ((string)data[0] == "SetSkill")
                SetSkill(data);
            else if ((string)data[0] == "CancelSkill")
                CancelSkill();
            else if ((string)data[0] == "ClickMouse")
                ClickMouse();
            else if ((string)data[0] == "CancelHolding")
                CancelHolding();
            else if ((string)data[0] == "SetChargeCount")
                SetChargeCount(data);
        }

        void SetSkill(object[] data)
        {
           
        }

        void CancelSkill()
        {
            skillState = SkillStateDracoson.None;
        }

        void InstantiateObject(int chargePhase)
        {

            Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(aim.transform.position);
            Quaternion _tempQ = Quaternion.LookRotation(_tempRay.direction);

            if (chargePhase != 0)
            {
                Debug.Log("투사체 발사");
                PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dragonic Flame Projectile " + chargePhase,
                _tempRay.origin + _tempRay.direction * 0.5f, _tempQ, data: null);
            }
            else
            {
                PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dragonic Flame Projectile " + projectile,
                _tempRay.origin + _tempRay.direction * 0.5f, _tempQ, data: null);
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
                currentObjectForMe = PhotonNetwork.Instantiate("SiHyun/Prefabs/Dracoson/Dracoson Charge Effect " + chargePhase,
                    staffTopForMe.position, _staffRotationForMe);

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
        }

        void ClickMouse()
        {
            
        }

        void SetChargeCount(object[] data)
        {
            photonView.RPC("SetChargeCount", RpcTarget.AllBuffered, (int)data[1]);
            photonView.RPC("SetChargePhase", RpcTarget.AllBuffered, data);
        }

        void CallSetAnimation(string parameter, bool isParameter)
        {
            object[] _tempData = new object[3];
            _tempData[0] = "SetAnimation";
            _tempData[1] = parameter;
            _tempData[2] = isParameter;
            ResponseRPCCall(_tempData);
        }

        //마스터 클라이언트가 모든 클라이언트에게
        void ResponseRPCCall(object[] data)
        {
            photonView.RPC("CallRPCCulTistToAll", RpcTarget.AllBuffered, data);
            Debug.Log("RPC를 모두에게 쏘는중");
        }

        [PunRPC]
        public void SetChargeCount(int chargePhase)
        {
            chargeCount = chargePhase;
        }

        [PunRPC]
        public void SetChargePhase(object[] data)
        {
            overlayAnimator.SetBool("ChargePhase1", (bool)data[2]);
            overlayAnimator.SetBool("ChargePhase2", (bool)data[3]);
            overlayAnimator.SetBool("ChargePhase3", (bool)data[4]);
            overlayAnimator.SetBool("ChargePhaseOver", (bool)data[5]);
        }


        [PunRPC]
        public void CallRPCCulTistToAll(object[] data)
        {
            if ((string)data[0] == "UpdateData")
                UpdateDataByMasterClient(data);
            else if ((string)data[0] == "SetAnimation")
                SetAnimation(data);
        }

        void SetAnimation(object[] data)
        {
            if (photonView.IsMine)
            {
                animator.SetBool((string)data[1], (bool)data[2]);
                overlayAnimator.SetBool((string)data[1], (bool)data[2]);
            }
        }

        void UpdateDataByMasterClient(object[] data)
        {
            skillState = (SkillStateDracoson)data[2];
        }

        void CheckAnimator()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Invocation"))
            {
                animator.SetBool("isInvocation", false);
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Throw"))
            {
                animator.SetBool("isThrow", false);
            }
        }

        protected override void OnAnimatorIK()
        {
            base.OnAnimatorIK();

            if (overlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                LerpWeight(0.0045f);
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
