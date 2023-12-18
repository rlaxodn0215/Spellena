using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdCamControl : MonoBehaviour
{
    public Transform target;  // 카메라가 따라다닐 대상 오브젝트
    public float rotationSpeed = 2.0f;  // 마우스 회전 속도
    public float distance = 5.0f;  // 카메라와 대상 오브젝트 간의 거리

    private void Update()
    {
        // 마우스 입력을 받아 카메라 회전
        float horizontalInput = Input.GetAxis("Mouse X") * Input.GetJoystickNames().Length == 0 ? Input.GetAxis("Mouse X") : Input.GetAxis("Horizontal");

        // 현재 카메라의 높이 저장
        float currentHeight = transform.position.y;

        // 대상 오브젝트를 중점으로 왼쪽 또는 오른쪽으로 회전
        transform.RotateAround(target.position, Vector3.up, horizontalInput);

        // 카메라의 위치를 대상 오브젝트 주변으로 조절하면서 높이 유지
        Vector3 desiredPosition = target.position - transform.forward * distance;
        desiredPosition.y = currentHeight;  // 높이 유지
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 5.0f);
    }
}
