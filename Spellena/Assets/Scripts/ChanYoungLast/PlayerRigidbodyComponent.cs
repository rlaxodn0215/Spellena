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
    기능 : 플레이어에게 가해지는 외부 힘을 Vector3.zero로 Lerp시킴
    */
    private void LerpExternalForce()
    {
        externalForce.y = 0;
        externalForce = Vector3.Lerp(externalForce, Vector3.zero, Time.fixedDeltaTime);
    }

    /*
    기능 : 플레이어 이동을 위한 Rigidbody의 velocity 설정
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
    기능 : 키보드 입력 W, S, A, D를 Vector2 형식으로 받아와 캐릭터 이동 방향에 적용
    -> x : 좌, 우 y : 상, 하
    */
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 _inputData = context.ReadValue<Vector2>();

        direction.z = _inputData.y;
        direction.x = _inputData.x;
    }

    /*
    기능 : 점프 키 입력 시 Vector3.down 방향으로 Ray를 발사하여 바닥 레이어와 충돌하고 있을 경우 점프
    */
    public void OnJump()
    {
        Ray _ray = new Ray(transform.position + new Vector3(0, 0.01f, 0), Vector3.down);

        if (Physics.Raycast(_ray, 0.02f, layerMaskMap))
            playerRigidbody.AddForce(transform.up * jumpPower, ForceMode.Impulse);
    }

    /*
    기능 : 달리기 키 입력 중에 달리기 상태가 됨
    */
    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.performed;
    }

    /*
    기능 : 마우스 이동으로 캐릭터의 y회전 적용
    */
    public void OnMouseMove(InputAction.CallbackContext context)
    {
        float _rotation = context.ReadValue<Vector2>().x;
        transform.rotation =  Quaternion.Euler(0, transform.eulerAngles.y + _rotation / 5f, 0);
    }

    /*
    기능 : 로컬 플레이어의 스크립트에 externalForce의 값을 더함
    */
    [PunRPC]
    public void AddExternalForce(Vector3 force)
    {
        externalForce += force;
    }
}
