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
    ��� : �� �����Ӹ��� �ܺ� ���� �ε巴�� ���ҽ�Ŵ
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
    ��� : �� �����Ӹ��� Rigidbody�� �ӵ��� ����
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
    ��� : �ӵ��� �����ϱ� ���� ������ ����
    ���� ->
    direction : Ű���� W, S, A, D�� �Է��� Vector2�������� �޾ƿ�
    */
    private void ChangeVelocity(Vector2 direction)
    {
        targetDirection = direction;
    }

    /*
    ��� : ���� �̵� ���¸� ���� �Ǵ� �޸��� ���·� ����
    ���� ->
    moveType : �ȱ�, �ٱ�, ���� ���¸� Ȯ�� ����
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
    ��� : �ܺ� ���� �߰�
    ���� ->
    newForce : ExternalObserver���� �ܺ� ���� ����
    */

    private void AddExternalForce(Vector3 newForce)
    {
        externalForce += newForce;
    }

    /*
    ��� : ���콺 �̵� ���� �޾� ĳ������ y�� ī�޶��� x�� ȸ����Ŵ
    ���� ->
    mouseMove : ���콺 �̵� ���� Vector2 �������� ����
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
    ��� : ĳ���Ͱ� �ٴڿ� �پ������� velocity�� y���� ���� 
    */
    private void PlayJump()
    {
        if (IsGround())
            playerRigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }


    /*
    ��� : ĳ���Ͱ� �ٴڿ� �پ��ִ��� Ȯ��
    ���� : ���� �پ������� true, �ƴϸ� false
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
