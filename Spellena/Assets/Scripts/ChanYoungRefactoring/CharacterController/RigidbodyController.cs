using CalculatorSystem;
using InputData;
using ObserverData;
using UnityEngine;

public class RigidbodyController : MonoBehaviour
{
    public Camera mainCamera;

    private Rigidbody playerRigidBody;

    private FunctionFrame<Vector2> lowerDirReceiver;
    private FunctionFrame<MoveType> lowerTypeReceiver;
    private FunctionFrame<Vector3> externalForceReceiver;

    private FunctionFrame<Vector2> mouseMoveReceiver;


    private LayerMask layerMaskMap;
    private Vector2 targetDirection = new Vector2(0, 0);
    private bool isRunning = false;

    private float moveSpeed;
    private float jumpForce;
    private float runRate;

    private Vector3 externalForce = Vector3.zero;

    private void Start()
    {
        playerRigidBody = GetComponent<Rigidbody>();
        lowerDirReceiver = new FunctionFrame<Vector2>(ObserveType.Receive);
        lowerDirReceiver.SetNotify(ChangeVelocity);
        lowerTypeReceiver = new FunctionFrame<MoveType>(ObserveType.Receive);
        lowerTypeReceiver.SetNotify(ChangeMoveType);
        externalForceReceiver = new FunctionFrame<Vector3>(ObserveType.Receive);
        externalForceReceiver.SetNotify(AddExternalForce);

        GetComponent<LowerObserver>().RaiseFunction(lowerDirReceiver);
        GetComponent<LowerObserver>().RaiseFunction(lowerTypeReceiver);
        GetComponent<ExternalObserver>().RaiseFunction(externalForceReceiver);

        mouseMoveReceiver = new FunctionFrame<Vector2>(ObserveType.Receive);
        mouseMoveReceiver.SetNotify(RotateTransform);

        GetComponent<UpperObserver>().RaiseFunction(mouseMoveReceiver);

        DataController _tempDataController = GetComponent<DataController>();
        moveSpeed = _tempDataController.balanceData.moveSpeed;
        jumpForce = _tempDataController.balanceData.jumpForce;
        runRate = _tempDataController.balanceData.runRate;

        layerMaskMap = LayerMask.GetMask("Map");
    }

    private void FixedUpdate()
    {
        LerpExternalForce();
        SetVelocity();
    }


    /*
    기능 : 매 프레임마다 외부 힘을 부드럽게 감소시킴
    */
    private void LerpExternalForce()
    {
        externalForce = Vector3.Lerp(externalForce, Vector3.zero, Time.fixedDeltaTime);

        if (Mathf.Abs(externalForce.x) <= 0.01f)
            externalForce.x = 0;
        if (Mathf.Abs(externalForce.z) <= 0.01f)
            externalForce.z = 0;
        externalForce.y = 0;
    }

    /*
    기능 : 매 프레임마다 Rigidbody에 속도를 적용
    */
    private void SetVelocity()
    {
        Vector3 _velocity = Vector3.zero;
        if (targetDirection.x > 0)
            _velocity += transform.right;
        else if (targetDirection.x < 0)
            _velocity -= transform.right;

        if (targetDirection.y > 0)
            _velocity += transform.forward;
        else if (targetDirection.y < 0)
            _velocity -= transform.forward;

        _velocity.Normalize();


        if (isRunning)
            _velocity *= runRate;

        _velocity *= moveSpeed;
        _velocity += externalForce;

        playerRigidBody.velocity = new Vector3(_velocity.x, playerRigidBody.velocity.y, _velocity.z);

    }

    /*
    기능 : 속도에 적용하기 위한 방향을 적용
    인자 ->
    direction : 키보드 W, S, A, D의 입력을 Vector2형식으로 받아옴
    */
    private void ChangeVelocity(Vector2 direction)
    {
        targetDirection = direction;
    }

    /*
    기능 : 현재 이동 상태를 점프 또는 달리기 상태로 변경
    인자 ->
    moveType : 걷기, 뛰기, 점프 상태를 확인 가능
    */

    private void ChangeMoveType(MoveType moveType)
    {
        if (moveType == MoveType.None)
            isRunning = false;
        else if (moveType == MoveType.Run)
            isRunning = true;
        else if (moveType == MoveType.Jump)
            PlayJump();
    }

    /*
    기능 : 외부 힘을 추가
    인자 ->
    newForce : ExternalObserver에서 외부 힘을 받음
    */

    private void AddExternalForce(Vector3 newForce)
    {
        externalForce += newForce;
    }

    /*
    기능 : 마우스 이동 값을 받아 캐릭터의 y와 카메라의 x를 회전시킴
    인자 ->
    mouseMove : 마우스 이동 값을 Vector2 형식으로 받음
    */

    private void RotateTransform(Vector2 mouseMove)
    {
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + mouseMove.x / 5f, 0);

        float _nextAngle = mainCamera.transform.eulerAngles.x - mouseMove.y / 5f;
        float _normalizedAngle = Calculator.NormalizeAngle(_nextAngle);
        if (_normalizedAngle > 60)
            _normalizedAngle = 60;
        else if (_normalizedAngle < -60)
            _normalizedAngle = -60;
        mainCamera.transform.localRotation = Quaternion.Euler(_normalizedAngle, 0, 0);
    }

    /*
    기능 : 캐릭터가 바닥에 붙어있으면 velocity에 y값을 더해 
    */
    private void PlayJump()
    {
        if (IsGround())
            playerRigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }


    /*
    기능 : 캐릭터가 바닥에 붙어있는지 확인
    리턴 : 땅이 붙어있으면 true, 아니면 false
    */
    private bool IsGround()
    {
        Ray _jumpRay = new Ray();
        _jumpRay.direction = Vector3.down;
        _jumpRay.origin = transform.position + new Vector3(0, 0.01f, 0);
        RaycastHit _groundHit;

        if (Physics.Raycast(_jumpRay, out _groundHit, 0.02f, layerMaskMap))
            return true;
        else
            return false;
    }
}
