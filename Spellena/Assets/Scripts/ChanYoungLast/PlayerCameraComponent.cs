using CalculatorSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraComponent : MonoBehaviour
{
    private Camera playerCamera;

    /*
    기능 : 카메라의 x회전 적용
    */
    public void OnMouseMove(InputAction.CallbackContext context)
    {
        float _nextAngle = transform.localEulerAngles.x - context.ReadValue<Vector2>().y / 5f;
        float _normalizedAngle = Calculator.NormalizeAngle(_nextAngle);

        if (_normalizedAngle > 60)
            _normalizedAngle = 60;
        else if (_normalizedAngle < -60)
            _normalizedAngle = -60;

        transform.localRotation = Quaternion.Euler(_normalizedAngle, 0, 0);
    }
}
