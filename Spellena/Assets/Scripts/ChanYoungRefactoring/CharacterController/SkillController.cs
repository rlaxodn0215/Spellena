using InputData;
using ObserverData;
using Photon.Pun;
using System.Collections.Generic;
using AnimationDataRelated;

public class SkillController : MonoBehaviourPunCallbacks, IPunObservable
{
    private FunctionFrame<int> skillKeyReceiver;
    private FunctionFrame<InputSide> mouseClickReceiver;
    private FunctionFrame<AnimationData> animationSender;

    private int skillIndex = -1;

    //평타의 목표 Index -> 마우스 입력에 따라 변함
    private int plainRouteIndex = 0;

    private SkillLogicState currentState
    {
        get 
        { 
            return currentState; 
        }
        set
        {
            if (value == null)
                currentState.Quit();
            else
                currentState.Enter();
            currentState = value;
        }
    }

    public List<SkillLogicState> skillStates;
    public List<SkillLogicState> plainStates;

    public List<bool> isReady;

    private void Start()
    {
        skillKeyReceiver = new FunctionFrame<int>(ObserveType.Receive);
        skillKeyReceiver.SetNotify(GetSkillKey);
        mouseClickReceiver = new FunctionFrame<InputSide>(ObserveType.Receive);
        mouseClickReceiver.SetNotify(ClickMouse);

        animationSender = new FunctionFrame<AnimationData>(ObserveType.Send);

        AnimationData _data = new AnimationData();
        _data.type = AnimationType.None;
        _data.index = -1;

        animationSender.Data = _data;

        GetComponent<UpperObserver>().RaiseFunction(skillKeyReceiver);
        GetComponent<UpperObserver>().RaiseFunction(mouseClickReceiver);
        GetComponent<UpperObserver>().RaiseFunction(animationSender);
    }

    private void FixedUpdate()
    {
        //스킬 진행이 완료되면 현재 스킬 상태를 초기화함
        if(currentState != null)
        {
            currentState.PlayFixedUpdate();
            if (currentState.isOver)
            {
                if (!currentState.isSkill)
                    ChangePlainIndex();
                currentState = null;
            }
        }

        //스킬과 평타 상태의 시간 관련 요소를 진행시키며 애니메이션 진행 준비가 되면 애니메이션 실행
        for(int i = 0; i < skillStates.Count; i++)
        {
            skillStates[i].PlayTimer();
            if (skillStates[i].isReadyToAnimation)
            {
                animationSender.Data = CreateNewAnimationData(AnimationType.Skill, i);
                skillStates[i].isReadyToAnimation = false;
            }
        }

        for(int i = 0; i < plainStates.Count; i++)
        {
            plainStates[i].PlayTimer();
            if (plainStates[i].isReadyToAnimation)
            {
                animationSender.Data = CreateNewAnimationData(AnimationType.Plain, i);
                plainStates[i].isReadyToAnimation = false;
            }
        }


        if(PhotonNetwork.IsMasterClient)
        {
            //마스터 클라이언트에서 포톤 뷰 사용자에게 스킬의 준비 완료를 알림
            for(int i = 0; i < skillStates.Count; i++)
            {
                if (!skillStates[i].IsCoolDownTime() && !isReady[i])
                {
                    isReady[i] = true;
                    photonView.RPC("SetSkillReady", photonView.Owner, i);
                }
            }
        }
    }

    private void Update()
    {
        if(currentState != null)
            currentState.PlayUpdate();
    }

    /*
    기능 : 마우스 입력이 있으면 발생하여 새로운 스킬 상태로 전환하거나 현재의 스킬 상태를 다음 상태로 전환
    인자 ->
    inputSide : 마우스 왼쪽, 오른쪽 판별
    */
    virtual protected void ClickMouse(InputSide inputSide)
    {
        if (IsProgressing())
            return;

        int _index = GetSkillIndex();

        if (inputSide == InputSide.Left)
        {
            if (currentState == null)
            {
                if (_index < 0)
                {
                    currentState = plainStates[plainRouteIndex];
                    photonView.RPC("SyncRoute", RpcTarget.MasterClient, plainRouteIndex, true);
                }
                else
                {
                    currentState = skillStates[_index];
                    photonView.RPC("SyncRoute", RpcTarget.MasterClient, _index, true);
                }
                currentState.ChangeNextRoute();
            }
            else
            {
                currentState.ChangeNextRoute();
                photonView.RPC("SyncRoute", RpcTarget.MasterClient, _index, false);
            }
        }
    }

    /*
    기능 : 스킬 키 입력을 받아 사용할 스킬을 설정
    인자 ->
    command : 스킬 키 번호를 받음
    */
    virtual protected void GetSkillKey(int command)
    {
        if (command >= 1 && command <= 4)
        {
            if (IsCoolDownTime(command - 1))
                return;
            skillIndex = command - 1;
        }
        else
            skillIndex = -1;
    }
    
    /*
    기능 : 진행 중인 스킬 확인
    리턴 : 스킬, 평타가 진행중이면 true, 아니면 false
    */
    private bool IsProgressing()
    {
        for(int i = 0; i < skillStates.Count; i++)
            if (skillStates[i].IsProgressing())
                return true;
        for (int i = 0; i < plainStates.Count; i++)
            if (plainStates[i].IsProgressing())
                return true;
        return false;
    }

    /*
    기능 : 목표 index의 쿨타임 확인
    인자 ->
    index : 확인하고 싶은 스킬 상태의 index
    리턴 : 쿨타임중이면 true, 아니면 false
    */
    private bool IsCoolDownTime(int index)
    {
        if (index < 0)
            return true;
        if (!isReady[index])
            return true;
        return false;
    }

    /*
    기능 : 현재 준비중인 스킬의 index를 가져옴 -> 오버라이드해서 사용
    리턴 : 스킬의 index를 가져옴
    */
    virtual protected int GetSkillIndex()
    {
        return skillIndex;
    }

    /*
    기능 : 애니메이션 실행 정보를 생성
    리턴 : 생성한 AnimationData 클래스를 리턴
    */
    protected AnimationData CreateNewAnimationData(AnimationType type, int index)
    {
        AnimationData _data = new AnimationData();
        _data.type = type;
        _data.index = index;
        return _data;
    }

    /*
    기능 : 평타의 목표 Index를 변경함 -> 오버라이드
    */
    virtual protected void ChangePlainIndex()
    {
    }

    /*
    기능 : 포톤 뷰 소유자에게 스킬의 쿨타임이 종료된 것을 알림
    인자 ->
    index : 쿨타임이 종료된 스킬의 index
    */
    [PunRPC]
    public void SetSkillReady(int index)
    {
        isReady[index] = true;
    }

    /*
    기능 : 다른 모든 플레이어가 포톤 뷰 소유자와 같은 현재 스킬 상태로 전환된다.
    */
    [PunRPC]
    public void SyncRoute(int index, bool isSkill ,bool isFirst)
    {
        if (isFirst)
        {
            if (isSkill)
                currentState = skillStates[index];
            else
                currentState = plainStates[index];
        }
        currentState.ChangeNextRoute();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
