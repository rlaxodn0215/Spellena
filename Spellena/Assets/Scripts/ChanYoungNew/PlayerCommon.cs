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

    //speedRate�� ������ �÷��̾��� �̵��ӵ� ����
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

    //�������� �ܺ� ��
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

        //�ڽ��� Ŭ���̾�Ʈ������ �帧
        public List<State> route = new List<State>();
        public bool isLocalReady = false;

        //�ٸ� Ŭ���̾�Ʈ������ �帧
        public List<State> networkRoute = new List<State>();
        public bool isReady = false;

        //���� �ð� -> 
        public float progressTime = 0f;

        //��Ÿ�������� ��� Ŭ���̾�Ʈ�� ����� -> isReady�� �����Ϳ����� ��ȣ�� ����
        public float coolDownTime = 0f;

    }

    virtual protected void Awake()
    {
        InitCommonComponents();
        InitUniqueComponents();
    }

    //ĳ���� �������� ����Ǵ� ����
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


        //��ų
        AddSkill(playerData.skillCastingTime.Count);
        AddPlain(playerData.plainCastingTime.Count);
    }


    //ĳ���� ���� �ٸ��� ����Ǵ� ����
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

    //��ų ����, ��Ÿ�� ����
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
                //���� ���·� �����Ǹ鼭 ���� ��ȯ �̺�Ʈ �߻�
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

    //ĳ������ ��Ÿ, ��ų�� �غ� �Ϸ� ���¸� Ŭ���̾�Ʈ���� ����
    [PunRPC]
    public void NotifyReady(int callType, int index)
    {
        if ((CallType)callType == CallType.Skill)
            skillDatas[index].isReady = true;
        else
            plainDatas[index].isReady = true;
    }

    /*
    ��� : ĳ���� ��ų�� ���¸� ���� ���·� ��ȯ
    ���� ->
    callType : ��ų���� ��Ÿ������ �޾ƿ�
    index : �� �� ��ų���� �޾ƿ�
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
    ��� : ���� ��ų�� ���°� ����Ǵ� �� �� �����Ӹ��� Ȯ��
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

    //��ų, ��Ÿ ���� ���� �� ȣ���
    virtual protected void CallStateChangeEvent(CallType callType, int index)
    {
        //�������� ���� �̵�
        if (callType == CallType.Skill)
            PlaySkillLogic(index);
        else
            PlayPlainLogic(index);
    }

    //��ų ,��Ÿ ���� ���� -> ĳ���͸��� �������̵� ���� ����
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
    ��� : moveDirection�� ���� externalForce�� ���� Rigidbody�� velocity�� ����
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
    ��� : ĳ���� ���� �� �÷��̾� Ŭ���̾�Ʈ���� �ڽ��� ĳ���Ϳ� ������ ����
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

    //�Է� �̺�Ʈ -> Ű���� ���� ���� �� ���� ȣ��


    /*
    ��� : ���콺�� �����̸� �þ߸� �̵��� �� ����
    ���� ->
    inputValue : ���콺 �Է��� Vector2 �������� ����
    -> ī�޶� ��, �� : x��ǥ ���
    -> ī�޶� ��, �� : y��ǥ ���
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
    ��� : Ű���� WSAD�� �Է��ϸ� ȣ��Ǿ� �����̴� ������ �����ϰ� ĳ���� �Ϲݽ� �ִϸ��̼� �̺�Ʈ�� ����
    ���� ->
    inputValue : Ű���� �Է��� Vector2 �������� ����
    -> W, S : x��ǥ ���
    -> A, D : y��ǥ ���
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
            //��ų ���� �߿��� �ٸ� �ൿ �Ұ�
            if (IsProgressing())
                return;

            int _index = GetIndexofSkillReady();//��ų ��� Ȯ��
            if(_index >= 0)
            {
                //�̹� ��ų ��Ÿ���� Ȯ���ϱ� ������ �ٷ� ��ų ���¸� �����ص���
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

    //��ų�� �غ�Ǿ� �ִ��� Ȯ��
    protected int GetIndexofSkillReady()
    {
        for (int i = 0; i < skillDatas.Count; i++)
            if (skillDatas[i].isLocalReady)
                return i;
        return -1;
    }

    //��Ÿ�� �غ�Ǿ��ִ��� Ȯ��
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


    //��Ÿ ���� ���� -> �������̵� �ؼ� ���
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
    ��� : ��ų�� routeIndex�� ����
    ���� ->
    callType : ��ų���� ��Ÿ������ �޾ƿ�
    index : �� �� ��ų���� �޾ƿ�
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
