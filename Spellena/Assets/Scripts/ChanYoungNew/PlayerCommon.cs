using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;
using System;
using System.Reflection;
using Unity.VisualScripting;

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

    public Camera cameraMain;
    public Camera cameraOverlay;

    protected GameObject unique;

    protected Rigidbody rigidbodyMain;
    protected GameObject AvatarForMe;
    protected GameObject AvatarForOther;

    public Vector2 moveDirection;
    public bool isRunning = false;
    protected bool isClicked = false;

    //가해지는 외부 힘
    protected Vector3 externalForce = Vector3.zero;
    protected LayerMask layerMaskMap;
    protected LayerMask layerMaskWall;

    public event Action<Vector2, bool> UpdateLowerAnimation;
    public event Action<int> PlayAnimation;

    protected List<SkillData> skillDatas = new List<SkillData>();

    protected class SkillData
    {
        public float skillCoolDownTime;
        public float skillUniqueTime;
        public float skillCastingTime;
        public float skillChannelingTime;

        public SkillState skillState = SkillState.None;
        public enum SkillState
        {
            None, Unique, Casting, Channeling
        }

        //마스터에서 보내는 확인 신호
        public bool isReady = false;
        //로컬에서 준비 상태 확인
        public bool isLocalReady = false;
        //홀딩, 지점 타격 등 특수 준비 상태가 있는지 확인
        public bool isUnique = false;
    }

    public enum SkillTiming
    {
        Immediately, AfterCasting
    }

    //로컬 -> None : 비어있는 상태, Ready : 준비 상태
    //마스터 -> Casting : 캐스팅으로 바뀌기 전에 서버에서 쿨타임 체크, Channeling : 채널링이 끝나고 쿨타임 적용
    //모든 서버 이벤트 -> 
    

    virtual protected void Awake()
    {
        rigidbodyMain = GetComponent<Rigidbody>();

        InitCommonComponents();
        InitUniqueComponents();
    }

    virtual protected void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            MovePlayer();
            CheckPlayerFunction();
        }

        ProgressTime();

    }

    virtual protected void ProgressTime()
    {
        for(int i = 0; i < skillDatas.Count; i++)
        {
            //쿨타임 관련
            if (skillDatas[i].skillCoolDownTime > 0f)
                skillDatas[i].skillCoolDownTime -= Time.fixedDeltaTime;

            if (PhotonNetwork.IsMasterClient
                && skillDatas[i].skillCoolDownTime <= 0f && skillDatas[i].isReady == false
                && skillDatas[i].skillCastingTime <= 0f && skillDatas[i].skillChannelingTime <= 0f)
            {
                skillDatas[i].isReady = true;
                photonView.RPC("NotifySkillIsReady", player, i);
            }

            //캐스팅 시간 관련
            if (skillDatas[i].skillCastingTime > 0f)
            {
                skillDatas[i].skillCastingTime -= Time.fixedDeltaTime;
                if (skillDatas[i].skillCastingTime <= 0f)
                {
                    if (photonView.IsMine)
                        skillDatas[i].skillState = SkillData.SkillState.Channeling;
                    //스킬 채널링 시간 같은 타이밍에 동시에 실행되므로 fixedDeltaTime 한 프레임 추가
                    skillDatas[i].skillChannelingTime = playerData.skillChannelingTime[i] + Time.fixedDeltaTime;
                    //캐스팅 시간이 있는 스킬은 이 곳에서 로직이 실행됨
                    PlaySkillLogic(i, SkillTiming.AfterCasting);
                }
            }

            //채널링 시간 관련
            if (skillDatas[i].skillChannelingTime > 0f)
            {
                skillDatas[i].skillChannelingTime -= Time.fixedDeltaTime;
                if (skillDatas[i].skillChannelingTime <= 0 && PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("NotifySetSkillCoolDownTime", RpcTarget.All, i);
                }
            }
        }
    }

    [PunRPC]
    public void NotifySkillIsReady(int index)
    {
        skillDatas[index].isReady = true;
        Debug.Log("스킬 " + index + " 준비됨");
    }

    [PunRPC]
    public void NotifySetSkillCoolDownTime(int index)
    {
        //쿨타임 적용
        skillDatas[index].skillCoolDownTime = 1f;
        if (photonView.IsMine)
            skillDatas[index].skillState = SkillData.SkillState.None;
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

    protected void InitCommonComponents()
    {
        layerMaskMap = LayerMask.GetMask("Map");
        layerMaskWall = LayerMask.GetMask("Wall");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        AddSkill(4);

        player = photonView.Owner;

        AvatarForOther = transform.GetChild(1).gameObject;
        AvatarForMe = transform.GetChild(2).gameObject;

        unique = transform.GetChild(0).GetChild(1).gameObject;
    }

    protected void AddSkill(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SkillData _skillData = new SkillData();
            skillDatas.Add(_skillData);
        }
    }

    virtual protected void InitUniqueComponents()
    {

    }

    virtual public void SetLocalPlayer(string team)
    {
        cameraOverlay.gameObject.SetActive(true);
        cameraMain.gameObject.SetActive(true);
        SkinnedMeshRenderer[] _skinMeshForOther = AvatarForOther.GetComponentsInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer[] _skinMeshForMe = AvatarForMe.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < _skinMeshForOther.Length; i++)
            _skinMeshForOther[i].gameObject.layer = 6;
        for (int i = 0; i < _skinMeshForMe.Length; i++)
            _skinMeshForMe[i].gameObject.layer = 8;

        // 플레이어 캐릭터 선택시 팀 태그 추가
        photonView.RPC("SetTag", RpcTarget.All, team);
    }

    [PunRPC]
    protected virtual void SetTag(string team)
    {
        this.tag = team;

        Transform[] allChildren = GetComponentsInChildren<Transform>();
        if (allChildren == null) return;

        foreach (Transform child in allChildren)
        {
            child.gameObject.tag = team;
        }
    }

    //입력 이벤트 -> 키보드 상태 변경 시 마다 호출

    virtual protected void OnMouseMove(InputValue inputValue)
    {
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + inputValue.Get<Vector2>().x / 5f, 0);

        float _nextAngle = cameraMain.transform.eulerAngles.x - inputValue.Get<Vector2>().y / 5f;
        float _normalizedAngle = GlobalOperation.Instance.NormalizeAngle(_nextAngle);
        if (_normalizedAngle > 60)
            _normalizedAngle = 60;
        else if (_normalizedAngle < -60)
            _normalizedAngle = -60;
        cameraMain.transform.localRotation = Quaternion.Euler(_normalizedAngle , 0, 0);
    }
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
        if (photonView.IsMine)
            if (skillDatas[0].isReady)
                SetSkillReady(0);
    }

    virtual protected void OnSkill2()
    {
        if (photonView.IsMine)
            if (skillDatas[1].isReady)
                SetSkillReady(1);
    }

    virtual protected void OnSkill3()
    {
        if (photonView.IsMine)
            if (skillDatas[2].isReady)
                SetSkillReady(2);
    }

    virtual protected void OnSkill4()
    {
        if (photonView.IsMine)
            if (skillDatas[3].isReady)
                SetSkillReady(3);
    }

    virtual protected void OnButtonCancel()
    {
        if (photonView.IsMine)
            if(!IsSkillProgressing())
                CancelSkill();
    }


    virtual protected void OnMouseButton()
    {
        isClicked = !isClicked;
        if (isClicked)
        {
            int _index = GetIndexofSkillReady();
            bool _isUnique = false;

            if (_index >= 0)
            {
                if (skillDatas[_index].skillState == SkillData.SkillState.Unique)
                    _isUnique = true;
            }

            photonView.RPC("ClickMouse", RpcTarget.MasterClient, _index, _isUnique);
        }
    }
    virtual protected void OnMouseButton2()
    {

    }

    virtual protected void OnSit()
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
        if (!IsSkillProgressing())
        {
            for (int i = 0; i < skillDatas.Count; i++)
                skillDatas[i].isLocalReady = false;
            skillDatas[index].isLocalReady = true;
            Debug.Log("스킬 " + (index + 1) + " : 준비");
        }
    }

    virtual protected void CancelSkill()
    {
        for(int i = 0; i < skillDatas.Count; i++)
        {
            if (skillDatas[i].skillState <= SkillData.SkillState.Unique)
            {
                if (skillDatas[i].skillState == SkillData.SkillState.Unique)
                    PlayUniqueState(i, false);
                skillDatas[i].skillState = SkillData.SkillState.None;
                skillDatas[i].isLocalReady = false;
            }
        }
    }

    protected int GetIndexofSkillReady()
    {
        for (int i = 0; i < skillDatas.Count; i++)
            if (skillDatas[i].isLocalReady)
                return i;
        return -1;
    }

    protected bool IsSkillProgressing()
    {
        for (int i = 0; i < skillDatas.Count; i++)
            if (skillDatas[i].skillState > SkillData.SkillState.Unique)
                return true;
        return false;
    }



    [PunRPC]
    virtual public void ClickMouse(int index, bool isUnique)
    {
        if(index >= 0)
        {
            //마스터 클라이언트에서 한 번 더 확인
            if (skillDatas[index].isReady)
            {
                if (skillDatas[index].isUnique && !isUnique)
                    photonView.RPC("SetSkillPlayer", player, index, (int)SkillData.SkillState.Unique);
                else
                    photonView.RPC("SetSkillPlayer", RpcTarget.All, index, (int)SkillData.SkillState.Casting);
            }
        }
        else
        {
            //평타
        }
    }

    [PunRPC]
    virtual public void SetSkillPlayer(int index, int nextSkillState)
    {
        //스킬 사용 타이밍
        SkillData.SkillState _nextSkillState = (SkillData.SkillState)nextSkillState;
        if (photonView.IsMine)
        {
            PlayUniqueState(index, false);

            skillDatas[index].skillState = _nextSkillState;
            if (_nextSkillState == SkillData.SkillState.Casting)
            {
                if (playerData.skillCastingTime[index] <= 0f)
                {
                    _nextSkillState = SkillData.SkillState.Channeling;
                    skillDatas[index].skillState = _nextSkillState;
                    skillDatas[index].skillChannelingTime = playerData.skillChannelingTime[index];
                }
                else
                    skillDatas[index].skillCastingTime = playerData.skillCastingTime[index];

                for (int i = 0; i < skillDatas.Count; i++)
                    skillDatas[i].isLocalReady = false;
                skillDatas[index].isReady = false;
                InvokeAnimation(index, true);

                if (_nextSkillState == SkillData.SkillState.Channeling)
                    //바로 채널링으로 이행되는 스킬은 바로 스킬 작동
                    PlaySkillLogic(index, SkillTiming.Immediately);
            }
            //Unique일때
            else
                PlayUniqueState(index, true);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
                skillDatas[index].isReady = false;
            if (playerData.skillCastingTime[index] <= 0f)
            {
                _nextSkillState = SkillData.SkillState.Channeling;
                skillDatas[index].skillChannelingTime = playerData.skillChannelingTime[index];
            }
            else
                skillDatas[index].skillCastingTime = playerData.skillCastingTime[index];
            if (_nextSkillState == SkillData.SkillState.Casting
                || _nextSkillState == SkillData.SkillState.Channeling)
                InvokeAnimation(index, true);
        }
    }

    virtual protected void PlayUniqueState(int index, bool IsOn)
    {

    }

    //스킬 로직 구현은 여기에서 오버라이드로 구현
    virtual protected void PlaySkillLogic(int index, SkillTiming timing)
    {

    }

    virtual protected void InvokeAnimation(int index, bool isPlay)
    {
        if (isPlay)
            PlayAnimation.Invoke(index);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new NotImplementedException();
    }
}
