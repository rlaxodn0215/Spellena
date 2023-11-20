using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

using HashTable = ExitGames.Client.Photon.Hashtable;

namespace Player
{
    public class ElementalOrder : Character, IPunObservable
    {
        public ElementalOrderData elementalOrderData;

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

        SkillState skillState = SkillState.None;

        RagnaEdge ragnaEdge = new RagnaEdge();
        BurstFlare burstFlare = new BurstFlare();
        GaiaTied gaiaTied = new GaiaTied();
        MeteorStrike meteorStrike = new MeteorStrike();
        TerraBreak terraBreak = new TerraBreak();
        EterialStorm eterialStorm = new EterialStorm();


        float ragnaEdgeCoolDownTime = 0f;
        float burstFlareCoolDownTime = 0f;
        float gaiaTiedCoolDownTime = 0f;
        float meteorStrikeCoolDownTime = 0f;
        float terraBreakCoolDownTime = 0f;
        float eterialStormCoolDownTime = 0f;

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

            if (photonView.IsMine)
            {
                //테스트 정보
                HashTable _tempTable = new HashTable();
                _tempTable.Add("CharacterViewID", photonView.ViewID);
                PhotonNetwork.LocalPlayer.SetCustomProperties(_tempTable);
            }

        }

        protected override void Start()
        {
            base.Start();
            if (photonView.IsMine)
            {
                Initialize();
                overlayCameraDefaultPos = overlayCamera.transform.localPosition;
            }

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
            minimapRenderTexture = minimapCamera.GetComponent<Camera>().targetTexture;
        }

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
            if (photonView.IsMine)
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
                    SetPointStrike();
                    photonView.RPC("ClickMouse", RpcTarget.MasterClient, screenRay.origin, screenRay.direction, photonView.OwnerActorNr);
                }
            }
        }

        //2. 로컬 플레이어 입력 후 마스터 클라이언트에서 이벤트 발생
        [PunRPC]
        public void AddCommand(int command)
        {
            if (commands.Count < 2)
            {
                commands.Add(command);
            }

            if (commands.Count >= 2)
            {
                isReadyToUseSkill = true;
            }
            UpdateData();
        }

        [PunRPC]
        public void CancelSkill()
        {
            isReadyToUseSkill = false;

            if (skillState == SkillState.BurstFlare)
            {
                burstFlareCoolDownTime = burstFlare.GetSkillCoolDownTime();
                burstFlare = null;
            }
            else if (skillState == SkillState.MeteorStrike)
            {
                meteorStrike = null;
            }
            else if (skillState == SkillState.TerraBreak)
            {
                terraBreak = null;
            }
            else if (skillState == SkillState.EterialStorm)
            {
                eterialStorm = null;
            }
            else if (skillState == SkillState.RagnaEdge)
            {
                ragnaEdge = null;
            }
            else if (skillState == SkillState.GaiaTied)
            {
                gaiaTied = null;
            }

            skillState = SkillState.None;
            commands.Clear();
            UpdateData();
        }

        [PunRPC]
        public void ClickMouse(Vector3 origin, Vector3 direction, int ownerActorNum)
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
            {
                UseSkill(origin, direction, ownerActorNum);
            }

            UpdateData();
            SetSkill();
        }

        //3. 마스터 클라이언트에서 데이터 처리
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
                    object[] _data = new object[5];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "RagnaEdge";

                    ragnaEdgeCoolDownTime = ragnaEdge.GetSkillCoolDownTime();

                    overlayAnimator.SetBool("Spell1", true);
                    animator.SetBool("Spell1", true);

                    PhotonNetwork.Instantiate("ChanYoung/Prefabs/RagnaEdge", pointStrike, Quaternion.identity, data: _data);
                    isPointStrike = false;
                    skillState = SkillState.None;
                    ragnaEdge.EndSkill();
                }
            }
            else if (skillState == SkillState.BurstFlare)
            {
                if (burstFlare.CheckCoolDown() == false)
                    return;
                if (burstFlare.CheckReady() == false)
                {
                    burstFlare.Initialize();
                    photonView.RPC("SetAnimation", RpcTarget.AllBuffered, "Spell2", true);
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
                        skillState = SkillState.None;
                        burstFlareCoolDownTime = burstFlare.GetSkillCoolDownTime();

                        photonView.RPC("SetAnimation", RpcTarget.AllBuffered, "Spell6", false);

                        burstFlare.EndSkill();
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

                    photonView.RPC("SetAnimation", RpcTarget.AllBuffered, "Spell3", true);

                    GameObject _tempObject = PhotonNetwork.Instantiate("ChanYoung/Prefabs/GaiaTied", pointStrike, Quaternion.identity, data: _data);
                    _tempObject.GetComponent<PhotonView>().TransferOwnership(ownerActorNum);
                    gaiaTiedCoolDownTime = gaiaTied.GetSkillCoolDownTime();
                    isPointStrike = false;
                    skillState = SkillState.None;
                    gaiaTied.EndSkill();
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

                        photonView.RPC("SetAnimation", RpcTarget.AllBuffered, "Spell4", true);

                        GameObject _tempObject = PhotonNetwork.Instantiate("ChanYoung/Prefabs/MeteorStrike", pointStrike, Quaternion.identity, data: _data);
                        _tempObject.GetComponent<PhotonView>().TransferOwnership(ownerActorNum);
                        meteorStrikeCoolDownTime = meteorStrike.GetSkillCoolDownTime();
                        isPointStrike = false;
                        skillState = SkillState.None;
                        meteorStrike.EndSkill();
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

                    photonView.RPC("SetAnimation", RpcTarget.AllBuffered, "Spell5", true);

                    GameObject _tempObject = PhotonNetwork.Instantiate("ChanYoung/Prefabs/TerraBreak", pointStrike, Quaternion.identity, data: _data);
                    _tempObject.GetComponent<PhotonView>().TransferOwnership(ownerActorNum);
                    terraBreakCoolDownTime = terraBreak.GetSkillCoolDownTime();
                    isPointStrike = false;
                    skillState = SkillState.None;
                    terraBreak.EndSkill();
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

                    photonView.RPC("SetAnimation", RpcTarget.AllBuffered, "Spell6", true);

                    GameObject _tempObject = PhotonNetwork.Instantiate("ChanYoung/Prefabs/Cylinder", pointStrike, Quaternion.identity, data: _data);
                    _tempObject.GetComponent<PhotonView>().TransferOwnership(ownerActorNum);
                    eterialStormCoolDownTime = eterialStorm.GetSkillCoolDownTime();
                    skillState = SkillState.None;
                    isPointStrike = false;
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

        //데이터 연동 관련 요소

        [PunRPC]
        public void SetAnimation(string stateName, bool isSpell)
        {
            if(photonView.IsMine)
            {
                overlayAnimator.SetBool(stateName, isSpell);
                animator.SetBool(stateName, isSpell);
            }
        }

        public void SetPointStrike()
        {
            photonView.RPC("SetPointStrikeServer", RpcTarget.AllBuffered, isPointStrike, pointStrike);
        }

        [PunRPC]
        public void SetPointStrikeServer(bool isNewPointStrike, Vector3 newPointStrike)
        {
            isPointStrike = isNewPointStrike;
            pointStrike = newPointStrike;
        }

        public void ResetSkill()
        {
            photonView.RPC("ResetSkillServer", RpcTarget.MasterClient);
        }

        [PunRPC]
        public void ResetSkillServer()
        {
            photonView.RPC("SetSkillServer", RpcTarget.AllBuffered, false, false, false, false, false);
        }

        public void SetSkill()
        {
            photonView.RPC("SetSkillServer", RpcTarget.OthersBuffered, isRagnaEdge, isGaiaTied, isMeteorStrike, isTerraBreak, isEterialStorm);
        }

        [PunRPC]
        public void SetSkillServer(bool spell1, bool spell3, bool spell4, bool spell5, bool spell6)
        {
            isRagnaEdge = spell1;
            isGaiaTied = spell3;
            isMeteorStrike = spell4;
            isTerraBreak = spell5;
            isEterialStorm = spell6;
        }

        [PunRPC]
        public void UpdateDataServer(int[] masterCommands, SkillState newSkillState, bool isReadyBurstFlare,
            int burstFlareBullets)
        {
            commands = masterCommands.ToList();
            skillState = newSkillState;
            burstFlare.SetReady(isReadyBurstFlare);
            burstFlare.SetBullet(burstFlareBullets);
        }

        void CheckCommands(Vector3 origin, Vector3 direction, int ownerActorNum)
        {
            isReadyToUseSkill = false;
            UseSkill(origin, direction, ownerActorNum);
            commands.Clear();
        }

        void UpdateData()
        {
            photonView.RPC("UpdateDataServer", RpcTarget.OthersBuffered, commands.ToArray(), skillState, burstFlare.CheckReady(),
                burstFlare.CheckCurrentBullet());
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
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos + new Vector3(0, 0.2f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell1", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell2"))
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos + new Vector3(0, 0.2f, 0), Time.deltaTime * 32f);
                if (burstFlare.CheckReady() == false)
                    overlayAnimator.SetBool("Spell2", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell3"))
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos + new Vector3(0, 0.1f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell3", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell4"))
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                   overlayCameraDefaultPos + new Vector3(0, 0.1f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell4", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell5"))
            {
                overlayAnimator.SetBool("Spell5", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell6"))
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                  overlayCameraDefaultPos + new Vector3(0, 0.2f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell6", false);
            }
            else
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos, Time.deltaTime);
            }

        }
        void CheckAnimator()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell1"))
                animator.SetBool("Spell1", false);
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell2"))
            {
                Debug.Log(animator.GetBool("Spell2"));
                if (burstFlare.CheckReady() == false)
                {
                    animator.SetBool("Spell2", false);
                }
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell3"))
                animator.SetBool("Spell3", false);
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell4"))
                animator.SetBool("Spell4", false);
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell5"))
                animator.SetBool("Spell5", false);
            else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Spell6"))
                animator.SetBool("Spell6", false);
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
