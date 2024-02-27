using Photon.Pun;
using UnityEngine;
using GlobalEnum;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PlayerCommonAnimation : MonoBehaviourPunCallbacks, IPunObservable
{

    public Camera cameraMain;
    protected Transform sightMain;
    protected Transform handSightMain;

    protected Animator animator;
    protected PlayerCommon playerCommon;
    protected PlayerData playerData;

    protected float moveDirectionVertical = 0;
    protected float moveDirectionHorizontal = 0;
    protected float targetMoveDirectionVertical = 0;
    protected float targetMoveDirectionHorizontal = 0;

    protected float rightHandWeight = 0f;
    protected float leftHandWeight = 0f;

    protected string nextAnimationState = string.Empty;
    private Quaternion networkRotation;


    public enum AnimationType
    {
        None, Casting, Channeling
    }

    public class AnimationRoute
    {
        public List<AnimationType> route = new List<AnimationType>();
        public List<float> routeTime = new List<float>();
        public int routeIndex = 0;
    }

    protected List<AnimationRoute> skillRoutes = new List<AnimationRoute>();
    protected List<AnimationRoute> plainRoutes = new List<AnimationRoute>();
    protected CallType currentAnimationType = CallType.None;
    protected int currentRoute = -1;
    protected int currentAnimationIndex = -1;
    protected string parameterName = "";

    int animationListener;

    virtual protected void Start()
    {
        InitCommonComponents();
        InitUniqueComponents();
    }

    virtual protected void Update()
    {
        LerpLowerAnimation();

        if (!photonView.IsMine)
            cameraMain.transform.localRotation = Quaternion.Lerp(cameraMain.transform.localRotation, networkRotation, Time.deltaTime * 8);

        if(ListenAnimatorState())
            PlayAnimationChangeEvent();
    }

    private void LerpLowerAnimation()
    {
        moveDirectionVertical = Mathf.Lerp(moveDirectionVertical, targetMoveDirectionVertical, Time.deltaTime * 20f);
        moveDirectionHorizontal = Mathf.Lerp(moveDirectionHorizontal, targetMoveDirectionHorizontal, Time.deltaTime * 20f);

        animator.SetFloat("MoveDirectionVertical", moveDirectionVertical);
        animator.SetFloat("MoveDirectionHorizontal", moveDirectionHorizontal);
    }

    protected void InitCommonComponents()
    {
        networkRotation = cameraMain.transform.localRotation;

        playerCommon = transform.root.GetComponent<PlayerCommon>();

        playerCommon.UpdateLowerAnimation += UpdateLowerAnimation;

        sightMain = cameraMain.transform.GetChild(0);
        handSightMain = cameraMain.transform.GetChild(1);
        animator = GetComponent<Animator>();
        playerData = playerCommon.playerData;

        playerCommon.PlayAnimation += PlayAnimation;

        animationListener = animator.GetCurrentAnimatorStateInfo(1).fullPathHash;

        AddSkillRoute(playerData.skillCastingTime.Count);
        AddPlainRoute(playerData.plainCastingTime.Count);
    }

    virtual protected void InitUniqueComponents()
    {

    }

    protected void AddSkillRoute(int count)
    {
        for(int i = 0; i < count; i++)
        {
            AnimationRoute _route = new AnimationRoute();
            skillRoutes.Add(_route);
        }
    }

    protected void AddPlainRoute(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AnimationRoute _route = new AnimationRoute();
            plainRoutes.Add(_route);
        }
    }

    /*
    기능 : 하반신 애니메이션 파라미터 값의 목표를 변경
    */
    virtual protected void UpdateLowerAnimation(Vector2 moveDirection, bool isRunning)
    {
        //y가 앞뒤, x가 좌우 x가 0보다 크면 오른쪽
        if(isRunning)
            moveDirection *= 2;

        targetMoveDirectionVertical = moveDirection.y;
        targetMoveDirectionHorizontal = moveDirection.x;

        animator.SetFloat("TargetMoveDirectionVertical", moveDirection.y);
        animator.SetFloat("TargetMoveDirectionHorizontal", moveDirection.x);

        photonView.RPC("ChangeLowerParameter", RpcTarget.Others, moveDirection.x, moveDirection.y);
    }

    /*
    기능 : 포톤 뷰를 가진 플레이어에게서 하반신 애니메이션의 목표 파라미터값을 받아 적용합니다.
    인자 ->
    directionX : 애니메이션의 앞, 뒤 방향
    directionY : 애니메이션의 좌, 우 방향
    */
    [PunRPC]
    public void ChangeLowerParameter(int directionX, int directionY)
    {
        animator.SetFloat("TargetMoveDirectionVertical", directionY);
        animator.SetFloat("TargetMoveDirectionHorizontal", directionX);
    }

    /*
    기능 : 애니메이션이 변경되는 것을 확인
    */
    virtual protected bool ListenAnimatorState()
    {
        int _hash = animator.GetNextAnimatorStateInfo(1).fullPathHash;
        if (_hash == 0)
            return false;
        if (_hash != animationListener)
        {
            animationListener = _hash;
            return true;
        }
        return false;
    }

    /*
    기능 : 애니메이션이 변경될 때 1회 실행되는 함수
    */
    virtual protected void PlayAnimationChangeEvent()
    {
        AnimatorStateInfo _info = animator.GetNextAnimatorStateInfo(1);
        ChangeAnimationRoot(_info);
    }

    /*
    기능 : 애니메이션의 routeIndex를 변경하여 routeIndex에 있는 route의 AnimationType에 따라
    애니메이션의 속도를 변경합니다.
    인자 ->
    info 
    */
    virtual protected void ChangeAnimationRoot(AnimatorStateInfo info)
    {
        if(currentAnimationType == CallType.Skill)
        {
            skillRoutes[currentAnimationIndex].routeIndex++;
            CallAnimationEvent();
            if (skillRoutes[currentAnimationIndex].routeIndex >= skillRoutes[currentAnimationIndex].route.Count)
            {
                skillRoutes[currentAnimationIndex].routeIndex = 0;

                currentAnimationType = CallType.None;
                currentAnimationIndex = -1;
            }
            else
                SetAnimationSpeed(info);
        }
        else if(currentAnimationType == CallType.Plain)
        {
            plainRoutes[currentAnimationIndex].routeIndex++;
            CallAnimationEvent();
            if (plainRoutes[currentAnimationIndex].routeIndex >= plainRoutes[currentAnimationIndex].route.Count)
            {
                plainRoutes[currentAnimationIndex].routeIndex = 0;

                currentAnimationType = CallType.None;
                currentAnimationIndex = -1;
            }
            else
                SetAnimationSpeed(info);
        }
    }

    virtual protected void CallAnimationEvent()
    {

    }

    /*
    기능 : 애니메이션 state의 정보를 받아 새로 변경되는 애니메이션의 속도를 변경
    인자 ->
    info : 다음 애니메이션 상태의 정도
    */
    virtual protected void SetAnimationSpeed(AnimatorStateInfo info)
    {
        float _length = info.length;
        float _time = -1f;
        string _parameter = "";

        if (_length > 0.99f && _length < 1.01f)
            return;

        if (currentAnimationType == CallType.Skill)
        {
            _parameter = "Skill" + (currentAnimationIndex + 1);
            _time = skillRoutes[currentAnimationIndex].routeTime[skillRoutes[currentAnimationIndex].routeIndex];
            AnimationType _tempType = skillRoutes[currentAnimationIndex].route[skillRoutes[currentAnimationIndex].routeIndex];
            AddAnimationType(ref _parameter, _tempType);
        }
        else if (currentAnimationType == CallType.Plain)
        {
            _parameter = "Plain" + (currentAnimationIndex + 1);
            _time = plainRoutes[currentAnimationIndex].routeTime[plainRoutes[currentAnimationIndex].routeIndex];
            AnimationType _tempType = plainRoutes[currentAnimationIndex].route[plainRoutes[currentAnimationIndex].routeIndex];
            AddAnimationType(ref _parameter, _tempType);
        }

        if(_time > 0f)
        {
            float _speed = _length / _time;
            animator.SetFloat(_parameter, _speed);
        }
    }

    protected void AddAnimationType(ref string parameter, AnimationType targetType)
    {
        if (targetType == AnimationType.Casting)
            parameter += "Casting";
        else if (targetType == AnimationType.Channeling)
            parameter += "Channeling";
    }

    virtual protected void OnAnimatorIK()
    {
        animator.SetLookAtPosition(sightMain.position);
        animator.SetLookAtWeight(1f);
    }

    /*
    기능 : 애니메이션 파라미터 변경
    인자 ->
    changeType : 애니메이션을 강제로 바꾸는 지를 결정하는 인자
    callType : 스킬인지 평타인지 구분
    index : 스킬 인덱스
    */
    virtual protected void PlayAnimation(AnimationChangeType changeType, CallType callType, int index)
    {
        if (index < 0)
            return;
        string _parameter = "";
        CallType _callType = CallType.None;

        if (callType == CallType.Skill)
        {
            _callType = CallType.Skill;
            _parameter += "Skill";
        }
        else if (callType == CallType.Plain)
        {
            _callType = CallType.Plain;
            _parameter += "Plain";
        }

        if (changeType == AnimationChangeType.Invoke)
        {
            _parameter += (index + 1);
            animator.SetBool(_parameter, true);
            currentAnimationType = _callType;
            currentAnimationIndex = index;
        }
        else if (changeType == AnimationChangeType.Change)
            ChangeAnimation(callType, index);
    }

    virtual protected void ChangeAnimation(CallType callType, int index)
    {

    }
    
    /*
    IPunObservable 인터페이스(Photon.Pun기능)
    기능 : 초당 20회 정도 호출되어 모든 클라이언트에 값을 연동함
    */
    virtual public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(cameraMain.transform.localRotation);
        else
            networkRotation = (Quaternion)stream.ReceiveNext();
    }

}
