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

    //��Ÿ�� ��ǥ Index -> ���콺 �Է¿� ���� ����
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
        //��ų ������ �Ϸ�Ǹ� ���� ��ų ���¸� �ʱ�ȭ��
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

        //��ų�� ��Ÿ ������ �ð� ���� ��Ҹ� �����Ű�� �ִϸ��̼� ���� �غ� �Ǹ� �ִϸ��̼� ����
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
            //������ Ŭ���̾�Ʈ���� ���� �� ����ڿ��� ��ų�� �غ� �ϷḦ �˸�
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
    ��� : ���콺 �Է��� ������ �߻��Ͽ� ���ο� ��ų ���·� ��ȯ�ϰų� ������ ��ų ���¸� ���� ���·� ��ȯ
    ���� ->
    inputSide : ���콺 ����, ������ �Ǻ�
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
    ��� : ��ų Ű �Է��� �޾� ����� ��ų�� ����
    ���� ->
    command : ��ų Ű ��ȣ�� ����
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
    ��� : ���� ���� ��ų Ȯ��
    ���� : ��ų, ��Ÿ�� �������̸� true, �ƴϸ� false
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
    ��� : ��ǥ index�� ��Ÿ�� Ȯ��
    ���� ->
    index : Ȯ���ϰ� ���� ��ų ������ index
    ���� : ��Ÿ�����̸� true, �ƴϸ� false
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
    ��� : ���� �غ����� ��ų�� index�� ������ -> �������̵��ؼ� ���
    ���� : ��ų�� index�� ������
    */
    virtual protected int GetSkillIndex()
    {
        return skillIndex;
    }

    /*
    ��� : �ִϸ��̼� ���� ������ ����
    ���� : ������ AnimationData Ŭ������ ����
    */
    protected AnimationData CreateNewAnimationData(AnimationType type, int index)
    {
        AnimationData _data = new AnimationData();
        _data.type = type;
        _data.index = index;
        return _data;
    }

    /*
    ��� : ��Ÿ�� ��ǥ Index�� ������ -> �������̵�
    */
    virtual protected void ChangePlainIndex()
    {
    }

    /*
    ��� : ���� �� �����ڿ��� ��ų�� ��Ÿ���� ����� ���� �˸�
    ���� ->
    index : ��Ÿ���� ����� ��ų�� index
    */
    [PunRPC]
    public void SetSkillReady(int index)
    {
        isReady[index] = true;
    }

    /*
    ��� : �ٸ� ��� �÷��̾ ���� �� �����ڿ� ���� ���� ��ų ���·� ��ȯ�ȴ�.
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
