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
    기능 : 하반신 애니메이션을 부드럽게 전환
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
    기능 : 하반신 BlendTree의 목표 값을 설정
    인자 -> 
    direction : 목표 방향
    */
    private void ChangeAnimationDirection(Vector2 direction)
    {
        targetDirection = direction;
        photonView.RPC("SyncTargetDirection", RpcTarget.Others, targetDirection);
    }

    /*
    기능 : 캐릭터가 달리고 있는지 확인
    인자 -> 
    moveType : 달리는 상태인지를 받아옴
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
    기능 : 이 캐릭터의 이동 방향을 공유
    인자 ->
    direction : 목표 방향
    */
    [PunRPC]
    public void SyncTargetDirection(Vector2 direction)
    {
        targetDirection = direction;
    }

    /*
    기능 : 이 캐릭터가 달리고 있는지를 공유
    인자 ->
    isRunning : 달리는 상태인지를 받아옴
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
