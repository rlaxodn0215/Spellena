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
    ��� : �ܺ� ���� ��ǥ ������ �ε巴�� ���ϰ� ��
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
    ��� : ������ٵ��� �ӵ��� ����
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
    ��� : Ű���� W, S, A, DŰ�� �Է��� ����Ǹ� ȣ��Ǿ� Vector2 �������� �޾ƿ� targetVelocity�� ����
    ���� ->
    inputValue : Ű���� W, S, A, DŰ�� Vector2 �������� �޾ƿ�
    */
    private void OnMove(InputValue inputValue)
    {
        targetDirection = new Vector2(inputValue.Get<Vector2>().x, inputValue.Get<Vector2>().y);
    }


}
