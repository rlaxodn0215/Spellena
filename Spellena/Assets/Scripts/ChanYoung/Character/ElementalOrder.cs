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

        float rightCurrentWeight = 0f;
        float leftCurrentWeight = 0f;

        float rightNotMineCurrentWeight = 0f;
        float leftNotMineCurrentWeight = 0f;

        float targetWeight = 0.4f;

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


        public GameObject leftHandSpellFire;
        public GameObject leftHandSpellStorm;
        public GameObject leftHandSpellLand;
        public GameObject rightHandSpellFire;
        public GameObject rightHandSpellStorm;
        public GameObject rightHandSpellLand;

        public GameObject testCube;

        //로컬 클라이언트에서 접근
        bool isEterialStorm = false;
        bool isMeteorStrike = false;
        bool isRagnaEdge = false;
        bool isTerraBreak = false;
        bool isGaiaTied = false;

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
            //walkSpeed = elementalOrderData.moveSpeed;
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

        void UseSkill(Vector3 origin, Vector3 direction)
        {
            if(skillState == SkillState.BurstFlare)
            {
                if(burstFlare == null)
                {
                    burstFlare = new BurstFlare();
                    burstFlare.Initialize();
                }

                if(burstFlare.CheckCoolDown() == false)
                {
                    return;
                }

                bool _result = burstFlare.ShootBullet();

                object[] _data = new object[5];
                _data[0] = name;
                _data[1] = tag;
                _data[2] = "BurstFlare";
                _data[3] = origin + direction;
                _data[4] = direction;

                overlayAnimator.SetBool("Spell2", true);

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

                    overlayAnimator.SetBool("Spell6", true);

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

                        pointStrike += new Vector3(0, 0.05f, 0);

                        overlayAnimator.SetBool("Spell4", true);

                        PhotonNetwork.Instantiate("ChanYoung/Prefabs/MeteorStrike", pointStrike, Quaternion.identity, data: _data);
                        meteorStrikeCoolDownTime = meteorStrike.GetSkillCoolDownTime();
                        meteorStrike = null;
                        isPointStrike = false;
                        skillState = SkillState.None;
                    }
                }
            }
            else if(skillState == SkillState.RagnaEdge)
            {
                if(ragnaEdge == null)
                {
                    ragnaEdge = new RagnaEdge();
                    isRagnaEdge = true;
                }

                if(isPointStrike == true)
                {
                    object[] _data = new object[5];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "RagnaEdge";

                    ragnaEdgeCoolDownTime = ragnaEdge.GetSkillCoolDownTime();

                    overlayAnimator.SetBool("Spell1", true);

                    PhotonNetwork.Instantiate("ChanYoung/Prefabs/RagnaEdge", pointStrike, Quaternion.identity, data: _data);
                    ragnaEdge = null;
                    isPointStrike = false;
                    skillState = SkillState.None;
                }
            }
            else if(skillState == SkillState.TerraBreak)
            {
                if (terraBreak == null)
                {
                    terraBreak = new TerraBreak();
                    isTerraBreak = true;
                }

                if (isPointStrike == true)
                {
                    object[] _data = new object[5];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "TerraBreak";

                    overlayAnimator.SetBool("Spell5", true);

                    PhotonNetwork.Instantiate("ChanYoung/Prefabs/TerraBreak", pointStrike, Quaternion.identity, data: _data);
                    terraBreakCoolDownTime = terraBreak.GetSkillCoolDownTime();
                    terraBreak = null;
                    isPointStrike = false;
                    skillState = SkillState.None;
                }
            }
            else if(skillState == SkillState.GaiaTied)
            {
                if(gaiaTied == null)
                {
                    gaiaTied = new GaiaTied();
                    isGaiaTied = true;
                }

                if(isPointStrike == true)
                {
                    object[] _data = new object[5];
                    _data[0] = name;
                    _data[1] = tag;
                    _data[2] = "gaiaTied";
                    _data[3] = direction;

                    overlayAnimator.SetBool("Spell3", true);

                    PhotonNetwork.Instantiate("ChanYoung/Prefabs/GaiaTied", pointStrike, Quaternion.identity, data: _data);
                    gaiaTiedCoolDownTime = gaiaTied.GetSkillCoolDownTime();
                    gaiaTied = null;
                    isPointStrike = false;
                    skillState = SkillState.None;
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
            else if(skillState == SkillState.MeteorStrike || skillState == SkillState.TerraBreak
                || skillState == SkillState.GaiaTied)
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
                        _maxDistace =  _localMeteorStrike.maxDistance;
                    }
                    else if(skillState == SkillState.TerraBreak)
                    {
                        TerraBreak _localTerraBreak = new TerraBreak();
                        _maxDistace = _localTerraBreak.maxDistance;
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
                        if(Physics.Raycast(_bottomRay, out _bottomRayHit, Mathf.Infinity, _tempLayerMask))
                        {
                            _arrivedGroundVec = _bottomRayHit.point;
                            if(_arrivedGroundVec.y < _bottomRayHit.collider.gameObject.transform.position.y)
                                _arrivedGroundVec = new Vector3(_arrivedGroundVec.x, _bottomRayHit.collider.gameObject.transform.position.y, _arrivedGroundVec.z);

                            testCube.SetActive(true);

                            testCube.transform.position = _arrivedGroundVec;

                            if (testCube.GetComponent<ParticleSystem>().time > 0.7f)
                            {
                                testCube.GetComponent<ParticleSystem>().Simulate(0.3f);
                            }


                        }
                        else
                        {
                            testCube.SetActive(false);
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
                            testCube.SetActive(true);

                            if (_arrivedGroundVec.y < _bottomRayHit.collider.gameObject.transform.position.y)
                                _arrivedGroundVec = new Vector3(_arrivedGroundVec.x, _bottomRayHit.collider.gameObject.transform.position.y, _arrivedGroundVec.z);

                            testCube.transform.position = _arrivedGroundVec;

                            if (testCube.GetComponent<ParticleSystem>().time > 0.7f)
                            {
                                testCube.GetComponent<ParticleSystem>().Simulate(0.3f);
                            }

                        }
                        else
                        {
                            testCube.SetActive(false);
                        }
                    }
                }
            }
            else if(skillState == SkillState.RagnaEdge)
            {
                if(isRagnaEdge == true)
                {
                    minimapCamera.GetComponent<Camera>().targetTexture = null;
                    minimapCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;
                    camera.GetComponent<MouseControl>().enabled = false;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
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
                else if(isMeteorStrike)
                {
                    isMeteorStrike = false;
                    testCube.SetActive(false);
                }
                else if(isRagnaEdge)
                {
                    minimapCamera.GetComponent<Camera>().targetTexture = minimapRenderTexture;
                    minimapCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;

                    camera.GetComponent<MouseControl>().enabled = true;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    isRagnaEdge= false;
                }
                else if(isTerraBreak)
                {
                    isTerraBreak = false;
                    testCube.SetActive(false);
                }
                else if(isGaiaTied)
                {
                    isGaiaTied = false;
                    testCube.SetActive(false);
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
            }
            else if(skillState == SkillState.TerraBreak)
            {
                terraBreak = null;
            }
            else if(skillState == SkillState.EterialStorm)
            {
                eterialStorm = null;
            }
            else if(skillState == SkillState.RagnaEdge)
            {
                ragnaEdge = null;
            }
            else if(skillState == SkillState.GaiaTied)
            {
                gaiaTied = null;
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
                    else if (((commands[0] == 1 && commands[1] == 2)
                        || (commands[0] == 2 && commands[1] == 1)) && ragnaEdgeCoolDownTime <= 0f)
                    {
                        skillState = SkillState.RagnaEdge;
                        CheckCommands(origin, direction);
                    }
                    else if (commands[0] == 2 && commands[1] == 2 && terraBreakCoolDownTime <= 0f)
                    {
                        skillState = SkillState.TerraBreak;
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
                    if(isEterialStorm || isRagnaEdge)
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
                    else if(isMeteorStrike || isTerraBreak || isGaiaTied)
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
                stream.SendNext(isRagnaEdge);
                stream.SendNext(isTerraBreak);
                stream.SendNext(isGaiaTied);
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
                isRagnaEdge = (bool)stream.ReceiveNext();
                isTerraBreak = (bool)stream.ReceiveNext();
                isGaiaTied = (bool)stream.ReceiveNext();
                arrivedVec = (Vector3)stream.ReceiveNext();
            }
        }

        //애니메이션
        void CheckOverlayAnimator()
        {
            if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell1"))
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos + new Vector3(0, 0.2f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell1", false);
                animator.SetBool("Spell1", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell2"))
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos + new Vector3(0, 0.2f, 0), Time.deltaTime * 32f);
                if (burstFlare == null)
                {
                    overlayAnimator.SetBool("Spell2", false);
                    animator.SetBool("Spell2", false);
                }
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell3"))
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos + new Vector3(0, 0.1f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell3", false);
                animator.SetBool("Spell3", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell4"))
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                   overlayCameraDefaultPos + new Vector3(0, 0.1f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell4", false);
                animator.SetBool("Spell4", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell5"))
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                   overlayCameraDefaultPos + new Vector3(0, 0.15f, 0), Time.deltaTime * 8f);
                overlayAnimator.SetBool("Spell5", false);
                animator.SetBool("Spell5", false);
            }
            else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell6"))
            {
                overlayAnimator.SetBool("Spell6", false);
                animator.SetBool("Spell6", false);
            }
            else
            {
                overlayCamera.transform.localPosition = Vector3.Lerp(overlayCamera.transform.localPosition,
                    overlayCameraDefaultPos, Time.deltaTime);
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
            animator.logWarnings = false;
            overlayAnimator.logWarnings = false;    
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
                else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Spell5"))
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

                Debug.Log(rightCurrentWeight);



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
