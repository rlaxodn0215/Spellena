using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Player
{
    public class ElementalOrder : Character, IPunObservable
    {
        public ElementalOrderData elementalOrderData;
        public GameObject avatarForOther;

        public GameObject overlayCamera;
        public GameObject minimapCamera;
        public GameObject Aim;
        public GameObject OverlaySight;
        public GameObject overlayAnimatorObject;

        public GameObject leftHandSpellFire;
        public GameObject leftHandSpellStorm;
        public GameObject leftHandSpellLand;
        public GameObject rightHandSpellFire;
        public GameObject rightHandSpellStorm;
        public GameObject rightHandSpellLand;

        public GameObject rangePointStrikeArea;
        public GameObject rangeBoxArea;

        public Animator overlayAnimator;

        RenderTexture minimapRenderTexture;


        //스킬 순서 11, 12, 13, 22, 23, 33 총 6개
        List<int> commands = new List<int>();

        Vector3 overlayCameraDefaultPos;
        Vector3 networkHandPoint;
        Vector3 currentHandPoint;

        Vector3 pointStrike;

        float networkRightCurrentWeight;
        float networkLeftCurrentWeight;

        float rightCurrentWeight = 0f;
        float leftCurrentWeight = 0f;

        float rightNotMineCurrentWeight = 0f;
        float leftNotMineCurrentWeight = 0f;

        float targetWeight = 0.4f;

        bool isReadyToUseSkill = false;
        bool isClicked = false;
        bool isPointStrike = false;

        Ray screenRay;

        public enum SkillState
        {
            None, BurstFlare, GaiaTied, EterialStorm, RagnaEdge, TerraBreak, MeteorStrike
        }

        public SkillState skillState = SkillState.None;

        RagnaEdge ragnaEdge = new RagnaEdge();
        BurstFlare burstFlare = new BurstFlare();
        GaiaTied gaiaTied = new GaiaTied();
        MeteorStrike meteorStrike = new MeteorStrike();
        TerraBreak terraBreak = new TerraBreak();
        EterialStorm eterialStorm = new EterialStorm();

        float spell1DefaultAnimationLength;

        [HideInInspector]
        public float ragnaEdgeCoolDownTime = 0f;
        [HideInInspector]
        public float burstFlareCoolDownTime = 0f;
        [HideInInspector]
        public float gaiaTiedCoolDownTime = 0f;
        [HideInInspector]
        public float meteorStrikeCoolDownTime = 0f;
        [HideInInspector]
        public float terraBreakCoolDownTime = 0f;
        [HideInInspector]
        public float eterialStormCoolDownTime = 0f;

        //로컬 클라이언트에서 접근
        bool isRagnaEdge = false;
        bool isGaiaTied = false;
        bool isMeteorStrike = false;
        bool isTerraBreak = false;
        bool isEterialStorm = false;

        private Vector3 handPoint;

        protected override void Awake()
        {
            base.Awake();
            CheckPoint();
            currentHandPoint = handPoint;

            //if (photonView.IsMine)
            //{
            //    //테스트 정보
            //    HashTable _tempTable = new HashTable();
            //    _tempTable.Add("CharacterViewID", photonView.ViewID);
            //    _tempTable.Add("IsAlive", true);
            //    PhotonNetwork.LocalPlayer.SetCustomProperties(_tempTable);
            //}

        }

        protected override void Start()
        {
            base.Start();
            if (photonView.IsMine)
            {
                Initialize();
                overlayCameraDefaultPos = overlayCamera.transform.localPosition;
            }
            dataHp = elementalOrderData.hp;
            animator = avatarForOther.GetComponent<Animator>();
        }
        protected override void Update()
        {
            base.Update();
            if (photonView.IsMine)
            {
                CheckOverlayAnimator();
                CheckAnimator();
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
            hp = elementalOrderData.hp;
            moveSpeed = elementalOrderData.moveSpeed;
            sideSpeed = elementalOrderData.sideSpeed;
            backSpeed = elementalOrderData.backSpeed;
            jumpHeight = elementalOrderData.jumpHeight;
            runSpeedRatio = elementalOrderData.runSpeedRatio;
            minimapRenderTexture = minimapCamera.GetComponent<Camera>().targetTexture;
        }

        [PunRPC]
        public override void IsLocalPlayer()
        {
            base.IsLocalPlayer();
            overlayCamera.SetActive(true);
            minimapCamera.SetActive(true);
        }

        //쿨타임
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

            if(terraBreakCoolDownTime > 0f)
            {
                terraBreakCoolDownTime -= Time.deltaTime;
            }

            if(ragnaEdgeCoolDownTime > 0f)
            {
                ragnaEdgeCoolDownTime -= Time.deltaTime;
            }

            if(meteorStrikeCoolDownTime > 0f)
            {
                meteorStrikeCoolDownTime -= Time.deltaTime;
            }
        }

        //1. 로컬 플레이어 입력
        void OnSkill1()
        {
            if (photonView.IsMine)
                RequestAddCommand(1);
        }

        void OnSkill2()
        {
            if (photonView.IsMine)
                RequestAddCommand(2);
        }

        void OnSkill3()
        {
            if (photonView.IsMine)
                RequestAddCommand(3);
        }

        void RequestAddCommand(int num)
        {
            object[] _tempData = new object[2];
            _tempData[0] = "AddCommand";
            _tempData[1] = num;
            RequestRPCCall(_tempData);
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
            if (photonView.IsMine)
            {
                isClicked = !isClicked;
                if (isClicked == true)
                {
                    //로컬 클라이언트 접근
                    if (isEterialStorm || isRagnaEdge)
                    {
                        Ray _tempRay = minimapCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                        RaycastHit _tempHit;
                        int _tempLayerMask = ~(1 << LayerMask.NameToLayer("Minimap"));
                        if (Physics.Raycast(_tempRay, out _tempHit, Mathf.Infinity, _tempLayerMask))
                        {
                            pointStrike = new Vector3(_tempHit.point.x, _tempHit.point.y, _tempHit.point.z);
                            isPointStrike = true;
                        }
                    }
                    else if (isMeteorStrike || isTerraBreak)
                    {
                        pointStrike = rangePointStrikeArea.transform.position;
                        isPointStrike = true;
                    }
                    else if (isGaiaTied)
                    {
                        rangeBoxArea.transform.localPosition -= new Vector3(0, rangeBoxArea.transform.localScale.y / 2, rangeBoxArea.transform.localScale.z / 2);
                        pointStrike = rangeBoxArea.transform.position;
                        isPointStrike = true;
                    }
                    object[] _tempPointStrikeData = new object[3];
                    _tempPointStrikeData[0] = "SetPointStrike";
                    _tempPointStrikeData[1] = isPointStrike;
                    _tempPointStrikeData[2] = pointStrike;
                    RequestRPCCall(_tempPointStrikeData);

                    object[] _tempData = new object[4];
                    _tempData[0] = "ClickMouse";
                    _tempData[1] = screenRay.origin;
                    _tempData[2] = screenRay.direction;
                    _tempData[3] = photonView.OwnerActorNr;
                    RequestRPCCall(_tempData);
                }
            }
        }

        //2. 로컬 플레이어 입력 후 마스터 클라이언트에서 이벤트 발생

        //마스터 클라이언트로 요청
        void RequestRPCCall(object[] data)
        {
            photonView.RPC("CallRPCElementalOrderMasterClient", RpcTarget.MasterClient, data);
        }

        [PunRPC]
        public void CallRPCElementalOrderMasterClient(object[] data)
        {
            if ((string)data[0] == "AddCommand")
                AddCommand((int)data[1]);
            else if ((string)data[0] == "CancelSkill")
                CancelSkill();
            else if ((string)data[0] == "ClickMouse")
                ClickMouse((Vector3)data[1], (Vector3)data[2], (int)data[3]);
            else if ((string)data[0] == "ResetSkill")
                ResetSkillResponse();
            else if ((string)data[0] == "SetPointStrike")
                SetPointStrike(data);
        }

        void AddCommand(int command)
        {
            if (commands.Count < 2)
                commands.Add(command);

            if (commands.Count >= 2)
                isReadyToUseSkill = true;
            UpdateData();
        }

        void CancelSkill()
        {
            isReadyToUseSkill = false;
            if (burstFlare.CheckReady() == true)
            {
                burstFlareCoolDownTime = elementalOrderData.burstFlareCoolDownTime;
                burstFlare.SetReady(false);
            }
            skillState = SkillState.None;
            commands.Clear();
            UpdateData();
        }
        
        void ClickMouse(Vector3 origin, Vector3 direction, int ownerActorNum)
        {
            if (skillState == SkillState.None)
            {
                if (isReadyToUseSkill == true)
                {
                    if (((commands[0] == 1 && commands[1] == 3)
                        || (commands[0] == 3 && commands[1] == 1)) && burstFlareCoolDownTime <= 0f)
                    {
                        skillState = SkillState.BurstFlare;
                        CheckCommands(origin, direction, ownerActorNum);
                    }
                    else if (((commands[0] == 2 && commands[1] == 3)
                        || (commands[0] == 3 && commands[1] == 2)) && gaiaTiedCoolDownTime <= 0f)
                    {
                        skillState = SkillState.GaiaTied;
                        CheckCommands(origin, direction, ownerActorNum);
                    }
                    else if (commands[0] == 3 && commands[1] == 3 && eterialStormCoolDownTime <= 0f)
                    {
                        skillState = SkillState.EterialStorm;
                        CheckCommands(origin, direction, ownerActorNum);
                    }
                    else if (commands[0] == 1 && commands[1] == 1 && meteorStrikeCoolDownTime <= 0f)
                    {
                        skillState = SkillState.MeteorStrike;
                        CheckCommands(origin, direction, ownerActorNum);
                    }
                    else if (((commands[0] == 1 && commands[1] == 2)
                        || (commands[0] == 2 && commands[1] == 1)) && ragnaEdgeCoolDownTime <= 0f)
                    {
                        skillState = SkillState.RagnaEdge;
                        CheckCommands(origin, direction, ownerActorNum);
                    }
                    else if (commands[0] == 2 && commands[1] == 2 && terraBreakCoolDownTime <= 0f)
                    {
                        skillState = SkillState.TerraBreak;
                        CheckCommands(origin, direction, ownerActorNum);
                    }
                }
            }
            else
                UseSkill(origin, direction, ownerActorNum);

            UpdateData();
            SetSkill();
        }

        void CheckCommands(Vector3 origin, Vector3 direction, int ownerActorNum)
        {
            isReadyToUseSkill = false;
            UseSkill(origin, direction, ownerActorNum);
            commands.Clear();
        }

        void ResetSkill()
        {
            object[] _tempData = new object[2];
            _tempData[0] = "ResetSkill";
            RequestRPCCall(_tempData);
        }

        void ResetSkillResponse()
        {
            object[] _tempData = new object[2];
            _tempData[0] = "ResetSkillResponse";
            ResponseRPCCall((_tempData));
        }

        void SetPointStrike(object[] data)
        {
            isPointStrike = (bool)data[1];
            pointStrike = (Vector3)data[2];
        }


        //마스터 클라이언트의 응답

        void ResponseRPCCall(object[] data)
        {
            photonView.RPC("CallRPCElementalOrderToAll", RpcTarget.AllBuffered, data);
        }

        [PunRPC]
        public void CallRPCElementalOrderToAll(object[] data)
        {
            if (((string)data[0] == "UpdateData"))
                UpdateDataServer(data);
            else if (((string)data[0] == "SetSkill"))
                SetSkillServer(data);
            else if (((string)data[0] == "SetAnimation"))
                SetAnimationServer(data);
            else if (((string)data[0] == "ResetSkillResponse"))
                ResetSkillServer();
        }

        void SetAnimation(string spell, bool isSpell)
        {
            object[] _tempData = new object[3];
            _tempData[0] = "SetAnimation";
            _tempData[1] = spell;
            _tempData[2] = isSpell;
            ResponseRPCCall(_tempData);
        }

        void SetAnimationServer(object[] data)
        {
            if (photonView.IsMine)
            {
                overlayAnimator.SetBool((string)data[1], (bool)data[2]);
                animator.SetBool((string)data[1], (bool)data[2]);
            }
        }

        void UpdateData()
        {
            object[] _tempData = new object[6];
            _tempData[0] = "UpdateData";
            _tempData[1] = commands.ToArray();
            _tempData[2] = skillState;
            _tempData[3] = burstFlare.CheckReady();
            _tempData[4] = burstFlare.CheckCurrentBullet();
            _tempData[5] = isPointStrike;
            ResponseRPCCall(_tempData);
        }

        void UpdateDataServer(object[] data)
        {
            commands = ((int[])data[1]).ToList();
            skillState = (SkillState)data[2];
            burstFlare.SetReady((bool)data[3]);
            burstFlare.SetBullet((int)data[4]);
            isPointStrike = (bool)data[5];
        }


        public void SetSkill()
        {
            object[] _tempData = new object[6];
            _tempData[0] = "SetSkill";
            _tempData[1] = isRagnaEdge;
            _tempData[2] = isGaiaTied;
            _tempData[3] = isMeteorStrike;
            _tempData[4] = isTerraBreak;
            _tempData[5] = isEterialStorm;
            ResponseRPCCall(_tempData);
        }

        void SetSkillServer(object[] data)
        {
            isRagnaEdge = (bool)data[1];
            isGaiaTied = (bool)data[2];
            isMeteorStrike = (bool)data[3];
            isTerraBreak = (bool)data[4];
            isEterialStorm = (bool)data[5];
        }

        void ResetSkillServer()
        {
            isRagnaEdge = false;
            isGaiaTied = false;
            isMeteorStrike = false;
            isTerraBreak = false;
            isEterialStorm = false;
        }

        //마스터 클라이언트에서 데이터 처리
        void UseSkill(Vector3 origin, Vector3 direction, int ownerActorNum)
        {
            if (skillState == SkillState.RagnaEdge)
            {
                if (ragnaEdge.CheckReady() == false)
                {
                    isRagnaEdge = true;
                    ragnaEdge.Initialize();
                }

                if (isPointStrike == true)
                {
                    object[] _data = new object[3];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "RagnaEdge";

                    SetAnimation("Spell1", true);

                    GameObject _tempObject = PhotonNetwork.Instantiate("ChanYoung/Prefabs/RagnaEdge", pointStrike, Quaternion.identity, data: _data);
                    _tempObject.GetComponent<PhotonView>().TransferOwnership(ownerActorNum);


                    isPointStrike = false;
                    ragnaEdgeCoolDownTime = elementalOrderData.rangaEdgeCoolDownTime;
                    ragnaEdge.EndSkill();

                    skillState = SkillState.None;
                }
            }
            else if (skillState == SkillState.BurstFlare)
            {
                if (burstFlare.CheckCoolDown() == false)
                    return;
                if (burstFlare.CheckReady() == false)
                {
                    burstFlare.Initialize();
                    SetAnimation("Spell2", true);
                }
                else
                {
                    bool _result = burstFlare.ShootBullet();

                    object[] _data = new object[5];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "BurstFlare";
                    _data[3] = origin + direction;
                    _data[4] = direction;

                    GameObject _tempObject = PhotonNetwork.Instantiate("ChanYoung/Prefabs/BurstFlare", origin, Quaternion.identity, data: _data);
                    _tempObject.GetComponent<PhotonView>().TransferOwnership(ownerActorNum);

                    if (_result == true)
                    {
                        SetAnimation("Spell2", false);
                        burstFlareCoolDownTime = elementalOrderData.burstFlareCoolDownTime;
                        burstFlare.EndSkill();
                        skillState = SkillState.None;
                    }
                }
            }
            else if (skillState == SkillState.GaiaTied)
            {
                if (gaiaTied.CheckReady() == false)
                {
                    gaiaTied.Initialize();
                    isGaiaTied = true;
                }

                if (isPointStrike == true)
                {
                    object[] _data = new object[4];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "gaiaTied";
                    _data[3] = direction;

                    SetAnimation("Spell3", true);

                    GameObject _tempObject = PhotonNetwork.Instantiate("ChanYoung/Prefabs/GaiaTied", pointStrike, Quaternion.identity, data: _data);
                    _tempObject.GetComponent<PhotonView>().TransferOwnership(ownerActorNum);

                    isPointStrike = false;
                    gaiaTiedCoolDownTime = elementalOrderData.gaiaTiedCoolDownTime;
                    gaiaTied.EndSkill();
                    skillState = SkillState.None;
                }
            }
            else if(skillState == SkillState.MeteorStrike)
            {
                if (meteorStrike.CheckReady() == false)
                {
                    isMeteorStrike = true;
                    meteorStrike.Initialize();
                }
                else
                {
                    if (isPointStrike == true)
                    {
                        object[] _data = new object[3];
                        _data[0] = name;
                        _data[1] = tag;
                        _data[2] = "MeteorStrike";

                        pointStrike += new Vector3(0, 0.05f, 0);

                        SetAnimation("Spell4", true);

                        GameObject _tempObject = PhotonNetwork.Instantiate("ChanYoung/Prefabs/MeteorStrike", pointStrike, Quaternion.identity, data: _data);
                        _tempObject.GetComponent<PhotonView>().TransferOwnership(ownerActorNum);

                        isPointStrike = false;
                        meteorStrikeCoolDownTime = elementalOrderData.meteorStrikeCoolDownTime;
                        meteorStrike.EndSkill();
                        skillState = SkillState.None;
                    }
                }
            }
            else if(skillState == SkillState.TerraBreak)
            {
                if (terraBreak.CheckReady() == false)
                {
                    isTerraBreak = true;
                    terraBreak.Initialize();
                }

                if (isPointStrike == true)
                {
                    object[] _data = new object[5];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "TerraBreak";

                    SetAnimation("Spell5", true);

                    GameObject _tempObject = PhotonNetwork.Instantiate("ChanYoung/Prefabs/TerraBreak", pointStrike, Quaternion.identity, data: _data);
                    _tempObject.GetComponent<PhotonView>().TransferOwnership(ownerActorNum);

                    isPointStrike = false;
                    terraBreakCoolDownTime = terraBreak.GetSkillCoolDownTime();
                    terraBreak.EndSkill();
                    skillState = SkillState.None;
                }
            }
            else if (skillState == SkillState.EterialStorm)
            {
                isEterialStorm = true;
                if (isPointStrike == true)
                {
                    object[] _data = new object[3];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "EterialStorm";

                    SetAnimation("Spell6", true);

                    GameObject _tempObject = PhotonNetwork.Instantiate("ChanYoung/Prefabs/Cylinder", pointStrike, Quaternion.identity, data: _data);
                    _tempObject.GetComponent<PhotonView>().TransferOwnership(ownerActorNum);

                    isPointStrike = false;
                    eterialStormCoolDownTime = elementalOrderData.eterialStormCoolDownTime;
                    skillState = SkillState.None;
                }
            }
        }

        //로컬 클라이언트에서 확인하는 요소
        void CheckSkillOnMine()
        {
            if (skillState == SkillState.None)
            {
                if (isRagnaEdge)
                {
                    minimapCamera.GetComponent<Camera>().targetTexture = minimapRenderTexture;
                    minimapCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;

                    camera.GetComponent<MouseControl>().enabled = true;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    ResetSkill();
                }
                else if (isGaiaTied)
                {
                    rangeBoxArea.SetActive(false);
                    ResetSkill();
                }
                else if (isMeteorStrike)
                {
                    rangePointStrikeArea.SetActive(false);
                    ResetSkill();
                }
                else if (isTerraBreak)
                {
                    rangePointStrikeArea.SetActive(false);
                    ResetSkill();
                }
                else if (isEterialStorm)
                {
                    minimapCamera.GetComponent<Camera>().targetTexture = minimapRenderTexture;
                    minimapCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;

                    camera.GetComponent<MouseControl>().enabled = true;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    ResetSkill();
                }
            }
            else if (skillState == SkillState.RagnaEdge)
            {
                if (isRagnaEdge == true)
                {
                    rangePointStrikeArea.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);
                    minimapCamera.GetComponent<Camera>().targetTexture = null;
                    minimapCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;
                    camera.GetComponent<MouseControl>().enabled = false;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            else if (skillState == SkillState.GaiaTied || skillState == SkillState.MeteorStrike
                || skillState == SkillState.TerraBreak)
            {
                if (isMeteorStrike == true || isTerraBreak == true || isGaiaTied == true)
                {
                    Vector3 _arrivedGroundVec;
                    Ray _tempRay = camera.GetComponent<Camera>().ScreenPointToRay(Aim.transform.position);
                    RaycastHit _tempRayHit;
                    LayerMask _tempLayerMask = LayerMask.GetMask("Map");
                    float _maxDistace;
                    if (skillState == SkillState.MeteorStrike)
                    {
                        MeteorStrike _localMeteorStrike = new MeteorStrike();
                        _maxDistace = _localMeteorStrike.maxDistance;
                        rangePointStrikeArea.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                    }
                    else if (skillState == SkillState.TerraBreak)
                    {
                        TerraBreak _localTerraBreak = new TerraBreak();
                        _maxDistace = _localTerraBreak.maxDistance;
                        rangePointStrikeArea.transform.localScale = new Vector3(2f, 2f, 2f);
                    }
                    else
                    {
                        GaiaTied _localGaiaTied = new GaiaTied();
                        _maxDistace = _localGaiaTied.maxDistance;
                    }

                    if (Physics.Raycast(_tempRay, out _tempRayHit, _maxDistace, _tempLayerMask))
                    {
                        Vector3 _hitPoint = _tempRayHit.point - _tempRay.direction * 0.05f;
                        Ray _bottomRay = new Ray(_hitPoint, Vector3.down);
                        RaycastHit _bottomRayHit;
                        if (Physics.Raycast(_bottomRay, out _bottomRayHit, Mathf.Infinity, _tempLayerMask))
                        {
                            _arrivedGroundVec = _bottomRayHit.point;
                            if (_arrivedGroundVec.y < _bottomRayHit.collider.gameObject.transform.position.y)
                                _arrivedGroundVec = new Vector3(_arrivedGroundVec.x, _bottomRayHit.collider.gameObject.transform.position.y, _arrivedGroundVec.z);
                            if (isGaiaTied)
                            {
                                rangeBoxArea.SetActive(true);
                                rangeBoxArea.transform.position = _arrivedGroundVec;
                                rangeBoxArea.transform.localPosition +=
                                    new Vector3(0, rangeBoxArea.transform.localScale.y / 2, rangeBoxArea.transform.localScale.z / 2);
                            }
                            else
                            {
                                rangePointStrikeArea.SetActive(true);
                                rangePointStrikeArea.transform.position = _arrivedGroundVec;

                                if (rangePointStrikeArea.GetComponent<ParticleSystem>().time > 0.7f)
                                {
                                    rangePointStrikeArea.GetComponent<ParticleSystem>().Simulate(0.3f);
                                }
                            }

                        }
                        else
                        {
                            rangeBoxArea.SetActive(false);
                            rangePointStrikeArea.SetActive(false);
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

                            if (_arrivedGroundVec.y < _bottomRayHit.collider.gameObject.transform.position.y)
                                _arrivedGroundVec = new Vector3(_arrivedGroundVec.x, _bottomRayHit.collider.gameObject.transform.position.y, _arrivedGroundVec.z);

                            if (isGaiaTied)
                            {
                                rangeBoxArea.SetActive(true);
                                rangeBoxArea.transform.position = _arrivedGroundVec;
                                rangeBoxArea.transform.localPosition += new Vector3(0, rangeBoxArea.transform.localScale.y / 2, rangeBoxArea.transform.localScale.z / 2);
                            }
                            else
                            {
                                rangePointStrikeArea.SetActive(true);
                                rangePointStrikeArea.transform.position = _arrivedGroundVec;

                                if (rangePointStrikeArea.GetComponent<ParticleSystem>().time > 0.7f)
                                {
                                    rangePointStrikeArea.GetComponent<ParticleSystem>().Simulate(0.3f);
                                }
                            }
                        }
                        else
                        {
                            rangeBoxArea.SetActive(false);
                            rangePointStrikeArea.SetActive(false);
                        }
                    }
                }
            }
            else if (skillState == SkillState.EterialStorm)
            {
                if (isEterialStorm)
                {
                    minimapCamera.GetComponent<Camera>().targetTexture = null;
                    minimapCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;
                    camera.GetComponent<MouseControl>().enabled = false;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }

        }


        //요청

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

        //로컬에서 처리
        void CheckOverlayAnimator()
        {
            if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell1"))
            {
                float _animationLength = overlayAnimator.GetCurrentAnimatorStateInfo(1).length;
                //3.31초의 클립을 2초에 보여주기 위한 애니메이션 속도 처리
                float _normalizedSpeed = _animationLength / elementalOrderData.ragnaEdgeCastingTime;
                overlayAnimator.SetFloat("Spell1Speed", _normalizedSpeed);

                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos + new Vector3(0, 0.2f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell1", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell2"))
            {
                float _animationLength = overlayAnimator.GetCurrentAnimatorStateInfo(1).length;
                float _normalizedSpeed = _animationLength / elementalOrderData.burstFlareCastingTime;
                overlayAnimator.SetFloat("Spell2Speed", _normalizedSpeed);

                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos + new Vector3(0, 0.2f, 0), Time.deltaTime * 32f);
                if (burstFlare.CheckReady() == false)
                    overlayAnimator.SetBool("Spell2", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell3"))
            {
                float _animationLength = overlayAnimator.GetCurrentAnimatorStateInfo(1).length;
                float _normalizedSpeed = _animationLength / elementalOrderData.gaiaTiedCastingTime;
                overlayAnimator.SetFloat("Spell3Speed", _normalizedSpeed);

                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos + new Vector3(0, 0.1f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell3", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell4"))
            {
                float _animationLength = overlayAnimator.GetCurrentAnimatorStateInfo(1).length;
                float _normalizedSpeed = _animationLength / elementalOrderData.meteorStrikeCastingTime;
                overlayAnimator.SetFloat("Spell4Speed", _normalizedSpeed);

                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                   overlayCameraDefaultPos + new Vector3(0, 0.1f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell4", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell5"))
            {
                float _animationLength = overlayAnimator.GetCurrentAnimatorStateInfo(1).length;
                float _normalizedSpeed = _animationLength / elementalOrderData.terraBreakCastingTime;
                overlayAnimator.SetFloat("Spell5Speed", _normalizedSpeed);
                overlayAnimator.SetBool("Spell5", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell6"))
            {
                float _animationLength = overlayAnimator.GetCurrentAnimatorStateInfo(1).length;
                float _normalizedSpeed = _animationLength / elementalOrderData.eterialStormCastingTime;
                overlayAnimator.SetFloat("Spell6Speed", _normalizedSpeed);

                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                  overlayCameraDefaultPos + new Vector3(0, 0.2f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell6", false);
            }
            else
            {
                //대기 상태시에는 애니메이터의 속도는 0이다
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos, Time.deltaTime);
            }

        }
        void CheckAnimator()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell1"))
            {
                float _animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
                float _normalizedSpeed = _animationLength / elementalOrderData.ragnaEdgeCastingTime;
                animator.SetFloat("Spell1Speed", _normalizedSpeed);
                animator.SetBool("Spell1", false);
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell2"))
            {
                float _animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
                float _normalizedSpeed = _animationLength / elementalOrderData.burstFlareCastingTime;
                animator.SetFloat("Spell2Speed", _normalizedSpeed);
                if (burstFlare.CheckReady() == false)
                {
                    animator.SetBool("Spell2", false);
                }
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell3"))
            {
                float _animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
                float _normalizedSpeed = _animationLength / elementalOrderData.gaiaTiedCastingTime;
                animator.SetFloat("Spell3Speed", _normalizedSpeed);
                animator.SetBool("Spell3", false);
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell4"))
            {
                float _animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
                float _normalizedSpeed = _animationLength / elementalOrderData.meteorStrikeCastingTime;
                animator.SetFloat("Spell4Speed", _normalizedSpeed);
                animator.SetBool("Spell4", false);
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell5"))
            {
                float _animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
                float _normalizedSpeed = _animationLength / elementalOrderData.terraBreakCastingTime;
                animator.SetFloat("Spell5Speed", _normalizedSpeed);
                animator.SetBool("Spell5", false);
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell6"))
            {
                float _animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
                float _normalizedSpeed = _animationLength / elementalOrderData.eterialStormCastingTime;
                animator.SetFloat("Spell6Speed", _normalizedSpeed);
                animator.SetBool("Spell6", false);
            }
        }

        void SetHandEffectPositionIK(int typeRight, int typeLeft)
        {
            if (typeRight == 0)
            {
                rightHandSpellFire.SetActive(false);
                rightHandSpellLand.SetActive(false);
                rightHandSpellStorm.SetActive(false);
            }
            else
            {
                if (typeRight == 1)
                {
                    rightHandSpellFire.SetActive(true);
                    rightHandSpellLand.SetActive(false);
                    rightHandSpellStorm.SetActive(false);
                }
                else if (typeRight == 2)
                {
                    rightHandSpellFire.SetActive(false);
                    rightHandSpellLand.SetActive(true);
                    rightHandSpellStorm.SetActive(false);
                }
                else if (typeRight == 3)
                {
                    rightHandSpellFire.SetActive(false);
                    rightHandSpellLand.SetActive(false);
                    rightHandSpellStorm.SetActive(true);
                }
            }
            if (typeLeft == 0)
            {
                leftHandSpellFire.SetActive(false);
                leftHandSpellLand.SetActive(false);
                leftHandSpellStorm.SetActive(false);
            }
            else
            {
                if (typeLeft == 1)
                {
                    leftHandSpellFire.SetActive(true);
                    leftHandSpellLand.SetActive(false);
                    leftHandSpellStorm.SetActive(false);
                }
                else if (typeLeft == 2)
                {
                    leftHandSpellFire.SetActive(false);
                    leftHandSpellLand.SetActive(true);
                    leftHandSpellStorm.SetActive(false);
                }
                else if (typeLeft == 3)
                {
                    leftHandSpellFire.SetActive(false);
                    leftHandSpellLand.SetActive(false);
                    leftHandSpellStorm.SetActive(true);
                }
            }
        }

        protected override void OnAnimatorIK()
        {
            base.OnAnimatorIK();
            if (photonView.IsMine)
            {
                if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
                {
                    leftCurrentWeight = Mathf.Lerp(leftCurrentWeight, 0.2f, Time.deltaTime * 8f);
                    rightCurrentWeight = Mathf.Lerp(rightCurrentWeight, 0.22f, Time.deltaTime * 8f);
                }
                else if(overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell1"))
                {
                    rightCurrentWeight = Mathf.Lerp(rightCurrentWeight, targetWeight, Time.deltaTime * 8f);
                    leftCurrentWeight = Mathf.Lerp(leftCurrentWeight, targetWeight, Time.deltaTime * 8f);
                }
                else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell2"))
                {
                    rightCurrentWeight = Mathf.Lerp(rightCurrentWeight, 0.2f, Time.deltaTime * 8f);
                    leftCurrentWeight = Mathf.Lerp(leftCurrentWeight, 0, Time.deltaTime * 8f);
                }
                else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell6"))
                {

                    rightCurrentWeight = Mathf.Lerp(rightCurrentWeight, targetWeight, Time.deltaTime * 8f);
                    leftCurrentWeight = Mathf.Lerp(leftCurrentWeight, 0, Time.deltaTime * 8f);
                }
                else
                {
                    rightCurrentWeight = Mathf.Lerp(rightCurrentWeight, 0, Time.deltaTime * 8f);
                    leftCurrentWeight = Mathf.Lerp(leftCurrentWeight, 0, Time.deltaTime * 8f);
                }

                overlayAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftCurrentWeight);
                overlayAnimator.SetIKPosition(AvatarIKGoal.LeftHand, OverlaySight.transform.position);

                overlayAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightCurrentWeight);
                overlayAnimator.SetIKPosition(AvatarIKGoal.RightHand, OverlaySight.transform.position);

                if (commands.Count <= 0)
                    SetHandEffectPositionIK(0, 0);
                else if(commands.Count == 1)
                    SetHandEffectPositionIK(commands[0], 0);
                else
                    SetHandEffectPositionIK(commands[0], commands[1]);

                animator.SetIKPosition(AvatarIKGoal.LeftHand, handPoint);
                animator.SetIKPosition(AvatarIKGoal.RightHand, handPoint);
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

        void CheckPoint()
        {
            screenRay = camera.GetComponent<Camera>().ScreenPointToRay(Aim.transform.position);
            handPoint = screenRay.origin + screenRay.direction;
        }
    }
}
