using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;
using System;
using GlobalEnum;

public class PlayerCommon : MonoBehaviourPunCallbacks, IPunObservable
{
    Photon.Realtime.Player player;

    public PlayerData playerData;

    protected int hp;
    private float speed = 5f;
    private float jumpForce = 7f;

    //speedRate로 접근해 플레이어의 이동속도 조정
    protected float speedRate = 0f;
    protected float jumpForceRate = 1.0f;

    protected bool isGround = false;
    protected bool isAlive = true;

    protected bool isUniqueState = false;

    public Camera cameraMain;
    public Camera cameraOverlay;
    public Camera cameraMinimap;
    public GameObject UI;
    protected GameObject aim;

    protected GameObject minimapMask;

    protected GameObject unique;

    protected Rigidbody rigidbodyMain;
    protected GameObject avatarForMe;
    protected GameObject avatarForOther;

    public Vector2 moveDirection;
    public bool isRunning = false;
    protected bool isClicked = false;
    protected bool isClicked2 = false;

    //가해지는 외부 힘
    protected Vector3 externalForce = Vector3.zero;
    protected LayerMask layerMaskMap;
    protected LayerMask layerMaskWall;
    protected LayerMask layerMaskPlayer;

    public event Action<Vector2, bool> UpdateLowerAnimation;
    public event Action<AnimationChangeType, CallType, int> PlayAnimation;

    protected List<SkillData> skillDatas = new List<SkillData>();
    private List<int> skillListener = new List<int>(); 
    protected List<SkillData> plainDatas = new List<SkillData>();
    private List<int> plainListener = new List<int>();

    protected int plainIndex = -1;

    protected bool isCameraLocked = false;
    protected PlayerInput playerInput;
    protected Vector3 pointStrike;

    protected class SkillData
    {
        public enum State
        {
            None, Unique, Casting, Holding, Channeling
        }

        public int routeIndex = 0;
        public bool isMine = false; //isMin

        //자신의 클라이언트에서의 흐름
        public List<State> route = new List<State>();
        public bool isLocalReady = false;

        //다른 클라이언트에서의 흐름
        public List<State> networkRoute = new List<State>();
        public bool isReady = false;

        //진행 시간 -> 
        public float progressTime = 0f;

        //쿨타임적용은 모든 클라이언트에 적용됨 -> isReady는 마스터에서만 신호를 보냄
        public float coolDownTime = 0f;

    }

    virtual protected void Awake()
    {
        InitCommonComponents();
        InitUniqueComponents();
    }

    //캐릭터 공통으로 적용되는 정보
    protected void InitCommonComponents()
    {
        rigidbodyMain = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        player = photonView.Owner;


        layerMaskMap = LayerMask.GetMask("Map");
        layerMaskWall = LayerMask.GetMask("Wall");
        layerMaskPlayer = LayerMask.GetMask("Player");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        avatarForOther = transform.GetChild(1).gameObject;
        avatarForMe = transform.GetChild(2).gameObject;

        unique = transform.GetChild(0).GetChild(1).gameObject;

        minimapMask = UI.transform.GetChild(0).gameObject;

        aim = UI.transform.GetChild(3).gameObject;

        if (photonView.IsMine)
            SetLocalPlayer();


        //스킬
        AddSkill(playerData.skillCastingTime.Count);
        AddPlain(playerData.plainCastingTime.Count);
    }


    //캐릭터 마다 다르게 적용되는 정보
    virtual protected void InitUniqueComponents()
    {

    }

    virtual protected void FixedUpdate()
    {
        ProgressTime();
        ListenStateChange();

        if (photonView.IsMine)
        {
            MovePlayer();
            CheckPlayerFunction();
        }
    }

    //스킬 진행, 쿨타임 진행
    virtual protected void ProgressTime()
    {
        for (int i = 0; i < skillDatas.Count; i++)
        {
            if (skillDatas[i].coolDownTime > 0f)
                skillDatas[i].coolDownTime -= Time.fixedDeltaTime;

            if (PhotonNetwork.IsMasterClient && skillDatas[i].isReady == false
                && skillDatas[i].coolDownTime <= 0f && skillDatas[i].progressTime <= 0f)
            {
                skillDatas[i].isReady = true;
                photonView.RPC("NotifyReady", player, (int)CallType.Skill, i);
            }

            if (skillDatas[i].progressTime > 0f)
            {
                skillDatas[i].progressTime -= Time.fixedDeltaTime;
                //다음 상태로 이전되면서 상태 전환 이벤트 발생
                if (skillDatas[i].progressTime <= 0f)
                    ChangeNextRoot(CallType.Skill, i);
            }
        }

        for (int i = 0; i < plainDatas.Count; i++)
        {
            if (PhotonNetwork.IsMasterClient && plainDatas[i].isReady == false)
            {
                plainDatas[i].isReady = true;
                photonView.RPC("NotifyReady", player, (int)CallType.Plain, i);
            }

            if (plainDatas[i].progressTime > 0f)
            {
                plainDatas[i].progressTime -= Time.fixedDeltaTime;
                if (plainDatas[i].progressTime <= 0f)
                    ChangeNextRoot(CallType.Plain, i);
            }
        }
    }

    //캐릭터의 평타, 스킬의 준비 완료 상태를 클라이언트에게 보냄
    [PunRPC]
    public void NotifyReady(int callType, int index)
    {
        if ((CallType)callType == CallType.Skill)
            skillDatas[index].isReady = true;
        else
            plainDatas[index].isReady = true;
    }

    /*
    기능 : 캐릭터 스킬의 상태를 다음 상태로 전환
    인자 ->
    callType : 스킬인지 평타인지를 받아옴
    index : 몇 번 스킬인지 받아옴
    */
    virtual protected void ChangeNextRoot(CallType callType, int index)
    {
        if (callType == CallType.Skill)
        {
            skillDatas[index].routeIndex++;
            if (skillDatas[index].isMine)
                if (skillDatas[index].routeIndex >= skillDatas[index].route.Count)
                    skillDatas[index].routeIndex = 0;
            else
                if (skillDatas[index].routeIndex >= skillDatas[index].networkRoute.Count)
                    skillDatas[index].routeIndex = 0;
        }
        else
        {
            plainDatas[index].routeIndex++;
            if (plainDatas[index].isMine)
                if (plainDatas[index].routeIndex >= plainDatas[index].route.Count)
                    plainDatas[index].routeIndex = 0;
            else
                if (plainDatas[index].routeIndex >= plainDatas[index].networkRoute.Count)
                    plainDatas[index].routeIndex = 0;
        }
    }


    virtual protected void Update()
    {
    }
    

    /*
    기능 : 현재 스킬의 상태가 변경되는 지 매 프레임마다 확인
    */
    private void ListenStateChange()
    {
        for(int i = 0; i < skillDatas.Count; i++)
        {
            if (skillDatas[i].routeIndex != skillListener[i])
            {
                CallStateChangeEvent(CallType.Skill, i);
                skillListener[i] = skillDatas[i].routeIndex;
            }
        }

        for(int i = 0; i < plainDatas.Count; i++)
        {
            if (plainDatas[i].routeIndex != plainListener[i])
            {
                CallStateChangeEvent(CallType.Plain, i);
                plainListener[i] = plainDatas[i].routeIndex;
            }
        }
    }

    //스킬, 평타 상태 변경 시 호출됨
    virtual protected void CallStateChangeEvent(CallType callType, int index)
    {
        //정상적인 상태 이동
        if (callType == CallType.Skill)
            PlaySkillLogic(index);
        else
            PlayPlainLogic(index);
    }

    //스킬 ,평타 로직 진행 -> 캐릭터마다 오버라이드 시켜 진행
    virtual protected void PlaySkillLogic(int index)
    {

    }

    virtual protected void PlayPlainLogic(int index)
    {

    }


    protected void AddSkill(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SkillData _skillData = new SkillData();
            skillDatas.Add(_skillData);
            skillListener.Add(0);
        }
    }

    protected void AddPlain(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SkillData _plainData = new SkillData();
            plainDatas.Add(_plainData);
            plainListener.Add(0);
        }
    }

    virtual protected void CheckPlayerFunction()
    {
        Ray _jumpRay = new Ray();
        _jumpRay.direction = Vector3.down;
        _jumpRay.origin = transform.position + new Vector3(0, 0.01f, 0);
        RaycastHit _groundHit;

        if (Physics.Raycast(_jumpRay, out _groundHit, 0.02f, layerMaskMap))
            isGround = true;
        else
            isGround = false;
    }


    /*
    기능 : moveDirection의 값과 externalForce에 따라 Rigidbody의 velocity를 변경
    */ 
    virtual protected void MovePlayer()
    {
        Vector3 _direction = Vector3.zero;

        if (moveDirection.x > 0)
            _direction += transform.right;
        else if (moveDirection.x < 0)
            _direction -= transform.right;

        if (moveDirection.y > 0)
            _direction += transform.forward;
        else if (moveDirection.y < 0)
            _direction -= transform.forward;

        _direction.Normalize();

        externalForce = Vector3.Lerp(externalForce, Vector3.zero, Time.fixedDeltaTime * 5);

        if (Mathf.Abs(externalForce.x) <= 0.01f)
            externalForce.x = 0;
        if (Mathf.Abs(externalForce.z) <= 0.01f)
            externalForce.z = 0;
        externalForce.y = 0;

        Vector3 _velocity = _direction * speed * speedRate;

        rigidbodyMain.velocity = new Vector3(_velocity.x, rigidbodyMain.velocity.y, _velocity.z) + externalForce;
    }

    /*
    기능 : 캐릭터 생성 시 플레이어 클라이언트에서 자신의 캐릭터용 설정을 적용
    */
    virtual public void SetLocalPlayer()
    {
        cameraOverlay.gameObject.SetActive(true);
        cameraMain.gameObject.SetActive(true);
        cameraMinimap.gameObject.SetActive(true);

        UI.SetActive(true);

        SkinnedMeshRenderer[] _skinMeshForOther = avatarForOther.GetComponentsInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer[] _skinMeshForMe = avatarForMe.GetComponentsInChildren<SkinnedMeshRenderer>();

        for (int i = 0; i < _skinMeshForOther.Length; i++)
            _skinMeshForOther[i].gameObject.layer = 6;
        for (int i = 0; i < _skinMeshForMe.Length; i++)
            _skinMeshForMe[i].gameObject.layer = 8;
    }

    //입력 이벤트 -> 키보드 상태 변경 시 마다 호출


    /*
    기능 : 마우스를 움직이면 시야를 이동할 수 있음
    인자 ->
    inputValue : 마우스 입력을 Vector2 형식으로 받음
    -> 카메라 좌, 우 : x좌표 출력
    -> 카메라 상, 하 : y좌표 출력
    */
    virtual protected void OnMouseMove(InputValue inputValue)
    {
        if (!isCameraLocked)
        {
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + inputValue.Get<Vector2>().x / 5f, 0);

            float _nextAngle = cameraMain.transform.eulerAngles.x - inputValue.Get<Vector2>().y / 5f;
            float _normalizedAngle = GlobalOperation.Instance.NormalizeAngle(_nextAngle);
            if (_normalizedAngle > 60)
                _normalizedAngle = 60;
            else if (_normalizedAngle < -60)
                _normalizedAngle = -60;
            cameraMain.transform.localRotation = Quaternion.Euler(_normalizedAngle, 0, 0);
        }
    }

    /*
    기능 : 키보드 WSAD를 입력하면 호출되어 움직이는 방향을 결정하고 캐릭터 하반신 애니메이션 이벤트를 실행
    인자 ->
    inputValue : 키보드 입력을 Vector2 형식으로 받음
    -> W, S : x좌표 출력
    -> A, D : y좌표 출력
    */
    virtual protected void OnMove(InputValue inputValue)
    {
        moveDirection = new Vector2(inputValue.Get<Vector2>().x, inputValue.Get<Vector2>().y);
        UpdateLowerAnimation?.Invoke(moveDirection, isRunning);
    }

    virtual protected void OnJump()
    {
        if(isGround && isAlive && photonView.IsMine)
            rigidbodyMain.AddForce(Vector3.up * jumpForce * jumpForceRate, ForceMode.Impulse);
    }

    virtual protected void OnRun()
    {
        isRunning = !isRunning;
        UpdateLowerAnimation?.Invoke(moveDirection, isRunning);
    }

    virtual protected void OnSkill1()
    {
        SetSkillReady(0);
    }

    virtual protected void OnSkill2()
    {
        SetSkillReady(1);
    }

    virtual protected void OnSkill3()
    {
        SetSkillReady(2);
    }

    virtual protected void OnSkill4()
    {
        SetSkillReady(3);
    }

    virtual protected void OnButtonCancel()
    {
        CancelSkill();
    }

    
    /*
    
    */
    virtual protected void OnMouseButton()
    {
        isClicked = !isClicked;
        if (isClicked)
        {
            //스킬 진행 중에는 다른 행동 불가
            if (IsProgressing())
                return;

            int _index = GetIndexofSkillReady();//스킬 사용 확인
            if(_index >= 0)
            {
                //이미 스킬 쿨타임을 확인하기 때문에 바로 스킬 상태를 변경해도됨
                ChangeNextRoot(CallType.Skill, _index);
            }
            else
            {
                if(plainDatas.Count > 0)
                {
                    _index = ChangePlainIndex(plainIndex, 0);

                    if (_index >= 0)
                    {
                        ChangeNextRoot(CallType.Plain, _index);
                        plainIndex = _index;
                    }
                }
            }
        }
    }
    virtual protected void OnMouseButton2()
    {

    }

    virtual protected void OnInteraction()
    {

    }

    virtual protected void OnFly()
    {

    }

    virtual protected void SetSkillReady(int index)
    {
        if (!IsProgressing() && skillDatas[index].isReady)
        {
            for (int i = 0; i < skillDatas.Count; i++)
                skillDatas[i].isLocalReady = false;
            skillDatas[index].isLocalReady = true;
        }
    }

    virtual protected void CancelSkill()
    {
        if(!IsProgressing())
        {
            for(int i = 0; i < skillDatas.Count; i++)
            {
                skillDatas[i].routeIndex = 0;
                skillDatas[i].isLocalReady = false;
            }
        }
    }

    //스킬이 준비되어 있는지 확인
    protected int GetIndexofSkillReady()
    {
        for (int i = 0; i < skillDatas.Count; i++)
            if (skillDatas[i].isLocalReady)
                return i;
        return -1;
    }

    //평타가 준비되어있는지 확인
    protected int GetIndexofPlainReady()
    {
        for (int i = 0; i < plainDatas.Count; i++)
            if (plainDatas[i].isLocalReady)
                return i;
        return -1;
    }

    protected bool IsProgressing()
    {
        for (int i = 0; i < skillDatas.Count; i++)
            if (skillDatas[i].progressTime > 0f)
                return true;

        for (int i = 0; i < plainDatas.Count; i++)
            if (plainDatas[i].progressTime > 0f)
                return true;
        return false;
    }


    //평타 진행 구조 -> 오버라이드 해서 사용
    virtual protected int ChangePlainIndex(int start, int type)
    {
        return -1;
    }

    [PunRPC]
    public void AddPower(Vector3 power)
    {
        externalForce += power;
    }

    [PunRPC]
    public void AddYPower(float power)
    {
        Vector3 _velocity = rigidbodyMain.velocity;
        _velocity.y += power;
        rigidbodyMain.velocity = _velocity;
    }


    /*
    기능 : 스킬의 routeIndex를 받음
    인자 ->
    callType : 스킬인지 평타인지를 받아옴
    index : 몇 번 스킬인지 받아옴
    */
    [PunRPC]
    virtual public void NotifyUseSkill(int callType, int index)
    {
        ChangeNextRoot((CallType)callType, index);
    }

    protected void CallPlayAnimation(AnimationChangeType changeType, CallType callType, int index)
    {
        PlayAnimation.Invoke(changeType, callType, index);
    }

    public void DownCamera()
    {
        float _nextAngle = cameraMain.transform.eulerAngles.x - 0.2f;
        float _normalizedAngle = GlobalOperation.Instance.NormalizeAngle(_nextAngle);
        if (_normalizedAngle > 60)
            _normalizedAngle = 60;
        else if (_normalizedAngle < -60)
            _normalizedAngle = -60;
        cameraMain.transform.localRotation = Quaternion.Euler(_normalizedAngle, 0, 0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    virtual protected void PlayLogic(CallType callType, SkillData.State state, int index)
    {
    }
}
