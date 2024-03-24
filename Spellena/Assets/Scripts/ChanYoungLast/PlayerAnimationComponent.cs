using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using AnimatorInfo;
using System.Collections.Generic;

public class PlayerAnimationComponent : MonoBehaviourPunCallbacks, IPunObservable
{
    private Animator animator;
    private LayerMask layerMaskMap;

    private Vector2 targetMoveDirection = Vector2.zero;
    private Vector2 moveDirection = Vector2.zero;
    private bool isRunning = false;

    private float lowerWeight = 1f;

    public List<ScriptableSkillData> skillDatas;
    public List<ScriptableAnimationRouteData> skillAnims;

    public List<ScriptableSkillData> plainDatas;
    public List<ScriptableAnimationRouteData> plainAnims;

    private void Start()
    {
        animator = GetComponent<Animator>();
        layerMaskMap = LayerMask.GetMask("Map");
    }

    private void FixedUpdate()
    {
        LerpAnimatorParameter();
        CheckLevitate();
    }


    /*
    기능 : 애니메이션의 부드러운 전환을 위하여 Lerp를 사용하여 애니메이터 파라미터를 설정
    */
    private void LerpAnimatorParameter()
    {
        Vector2 _direction = targetMoveDirection;

        if (isRunning)
            _direction *= 2;

        moveDirection = Vector2.Lerp(moveDirection, _direction, Time.fixedDeltaTime);

        animator.SetFloat("MoveVertical", moveDirection.y);
        animator.SetFloat("MoveHorizontal", moveDirection.x);
    }

    /*
    기능 : 플레이어가 공중에 있는 지 확인 후 애니메이션 레이어들의 Weight 변경
    */
    private void CheckLevitate()
    {
        Ray _ray = new Ray(transform.position + new Vector3(0, 0.01f, 0), Vector3.down);

        if (Physics.Raycast(_ray, 0.02f, layerMaskMap))
            lowerWeight = Mathf.Lerp(lowerWeight, 0f, Time.fixedDeltaTime);
        else
            lowerWeight = Mathf.Lerp(lowerWeight, 1f, Time.fixedDeltaTime);

        animator.SetLayerWeight((int)AnimatorLayer.Lower, lowerWeight);
        animator.SetLayerWeight((int)AnimatorLayer.Levitate, 1f - lowerWeight);
    }

    /*
    기능 : 키보드 W, S, A, D 입력에 따라 이동 애니메이션의 목표값 설정
    */
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 _direction = context.ReadValue<Vector2>();
        targetMoveDirection = _direction;
    }

    /*
    기능 : 달리기 키 입력 중에 달리기 상태가 됨
    */
    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.performed;
    }

    /*
    기능 : 애니메이션 실행
    */
    public void PlayAnimation(AnimationType type, int index, AnimationTiming timing)
    {
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(targetMoveDirection);
            stream.SendNext(isRunning);
        }
        else
        {
            targetMoveDirection = (Vector2)stream.ReceiveNext();
            isRunning = (bool)stream.ReceiveNext();
        }
    }
}
