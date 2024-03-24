using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRigidbodyComponent : MonoBehaviour
{
    private Rigidbody playerRigidbody;
    private LayerMask layerMaskMap;

    private Vector3 direction = Vector3.zero;
    private Vector3 externalForce = Vector3.zero;

    private float speed = 5f;
    private float jumpPower = 5f;
    private float runRate = 1.2f;

    private bool isRunning = false;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        layerMaskMap = LayerMask.GetMask("Map");
    }

    private void FixedUpdate()
    {
        LerpExternalForce();
        MovePlayer();
    }

    /*
    ��� : �÷��̾�� �������� �ܺ� ���� Vector3.zero�� Lerp��Ŵ
    */
    private void LerpExternalForce()
    {
        externalForce.y = 0;
        externalForce = Vector3.Lerp(externalForce, Vector3.zero, Time.fixedDeltaTime);
    }

    /*
    ��� : �÷��̾� �̵��� ���� Rigidbody�� velocity ����
    */
    private void MovePlayer()
    {
        Vector3 _velocity = new Vector3(direction.x, playerRigidbody.velocity.y, direction.z);

        _velocity *= speed;

        if (isRunning)
            _velocity *= runRate;

        _velocity += externalForce;

        playerRigidbody.velocity = _velocity;
    }

    /*
    ��� : Ű���� �Է� W, S, A, D�� Vector2 �������� �޾ƿ� ĳ���� �̵� ���⿡ ����
    -> x : ��, �� y : ��, ��
    */
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 _inputData = context.ReadValue<Vector2>();

        direction.z = _inputData.y;
        direction.x = _inputData.x;
    }

    /*
    ��� : ���� Ű �Է� �� Vector3.down �������� Ray�� �߻��Ͽ� �ٴ� ���̾�� �浹�ϰ� ���� ��� ����
    */
    public void OnJump()
    {
        Ray _ray = new Ray(transform.position + new Vector3(0, 0.01f, 0), Vector3.down);

        if (Physics.Raycast(_ray, 0.02f, layerMaskMap))
            playerRigidbody.AddForce(transform.up * jumpPower, ForceMode.Impulse);
    }

    /*
    ��� : �޸��� Ű �Է� �߿� �޸��� ���°� ��
    */
    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.performed;
    }

    /*
    ��� : ���콺 �̵����� ĳ������ yȸ�� ����
    */
    public void OnMouseMove(InputAction.CallbackContext context)
    {
        float _rotation = context.ReadValue<Vector2>().x;
        transform.rotation =  Quaternion.Euler(0, transform.eulerAngles.y + _rotation / 5f, 0);
    }

    /*
    ��� : ���� �÷��̾��� ��ũ��Ʈ�� externalForce�� ���� ����
    */
    [PunRPC]
    public void AddExternalForce(Vector3 force)
    {
        externalForce += force;
    }
}
