using InputData;
using ObserverData;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowerAnimationController : MonoBehaviourPunCallbacks, IPunObservable
{
    private FunctionFrame<Vector2> moveLowerDirReceiver;
    private FunctionFrame<MoveType> moveLowerTypeReceiver;

    private Animator animator;

    private Vector2 targetDirection = Vector2.zero;
    private Vector2 currentDirection = Vector2.zero;

    private bool isRunning = false;

    private void Start()
    {
        moveLowerDirReceiver = new FunctionFrame<Vector2>(ObserveType.Receive);
        moveLowerTypeReceiver = new FunctionFrame<MoveType>(ObserveType.Receive);

        transform.root.GetComponent<LowerObserver>().RaiseFunction(moveLowerDirReceiver);
        transform.root.GetComponent<LowerObserver>().RaiseFunction(moveLowerTypeReceiver);

        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        LerpLowerAnimation();
    }

    /*
    ��� : �Ϲݽ� �ִϸ��̼��� �ε巴�� ��ȯ
    */
    private void LerpLowerAnimation()
    {
        Vector3 _targetDirection = targetDirection;

        if (isRunning)
            _targetDirection *= 2;

        currentDirection = Vector2.Lerp(currentDirection, _targetDirection, Time.fixedDeltaTime * 20f);
        animator.SetFloat("moveVertical", currentDirection.x);
        animator.SetFloat("moveHorizontal", currentDirection.y);
    }

    /*
    ��� : �Ϲݽ� BlendTree�� ��ǥ ���� ����
    ���� -> 
    direction : ��ǥ ����
    */
    private void ChangeAnimationDirection(Vector2 direction)
    {
        targetDirection = direction;
        photonView.RPC("SyncTargetDirection", RpcTarget.Others, targetDirection);
    }

    /*
    ��� : ĳ���Ͱ� �޸��� �ִ��� Ȯ��
    ���� -> 
    moveType : �޸��� ���������� �޾ƿ�
    */
    private void ChangeAnimationType(MoveType moveType)
    {
        if (moveType == MoveType.None)
            isRunning = false;
        else if (moveType == MoveType.Run)
            isRunning = true;
        photonView.RPC("SyncRun", RpcTarget.Others, isRunning);
    }

    /*
    ��� : �� ĳ������ �̵� ������ ����
    ���� ->
    direction : ��ǥ ����
    */
    [PunRPC]
    public void SyncTargetDirection(Vector2 direction)
    {
        targetDirection = direction;
    }

    /*
    ��� : �� ĳ���Ͱ� �޸��� �ִ����� ����
    ���� ->
    isRunning : �޸��� ���������� �޾ƿ�
    */
    [PunRPC]
    public void SyncRun(bool isRunning)
    {
        this.isRunning = isRunning;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
