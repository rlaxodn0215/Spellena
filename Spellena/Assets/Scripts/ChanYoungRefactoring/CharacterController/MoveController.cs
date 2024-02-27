using ListenerNodeData;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveController : MonoBehaviour
{
    private Vector3 externalForce;
    private Rigidbody playerRigidBody;
    private Vector2 targetDirection = new Vector2();
    private float speed;
    private float speedRate;

    private void Awake()
    {
        playerRigidBody = GetComponent<Rigidbody>();

    }

    private void FixedUpdate()
    {
        LerpData();
        SetVelocity();
    }

    /*
    기능 : 외부 힘을 목표 값으로 부드럽게 변하게 함
    */
    private void LerpData()
    {
        externalForce = Vector3.Lerp(externalForce, Vector3.zero, Time.fixedDeltaTime);

        if (Mathf.Abs(externalForce.x) <= 0.01f)
            externalForce.x = 0;
        if (Mathf.Abs(externalForce.z) <= 0.01f)
            externalForce.z = 0;
        externalForce.y = 0;
    }

    /*
    기능 : 리지드바디의 속도를 변경
    */
    private void SetVelocity()
    {
        Vector3 _direction = Vector3.zero;

        if (targetDirection.x > 0)
            _direction += transform.right;
        else if (targetDirection.x < 0)
            _direction -= transform.right;

        if (targetDirection.y > 0)
            _direction += transform.forward;
        else if (targetDirection.y < 0)
            _direction -= transform.forward;

        _direction.Normalize();

        Vector3 _velocity = _direction * speed * speedRate;
        
        playerRigidBody.velocity = new Vector3(_velocity.x, playerRigidBody.velocity.y, _velocity.z) + externalForce;
    }

    /*
    기능 : 키보드 W, S, A, D키의 입력이 변경되면 호출되어 Vector2 형식으로 받아와 targetVelocity에 저장
    인자 ->
    inputValue : 키보드 W, S, A, D키를 Vector2 형식으로 받아옴
    */
    private void OnMove(InputValue inputValue)
    {
        targetDirection = new Vector2(inputValue.Get<Vector2>().x, inputValue.Get<Vector2>().y);
    }


}
