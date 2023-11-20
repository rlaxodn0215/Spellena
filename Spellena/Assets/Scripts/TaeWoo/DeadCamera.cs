using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using UnityEngine.InputSystem;

public class DeadCamera : MonoBehaviour
{
    [Header("Settings")]
    public Vector2 sensitivity = new Vector2(6, 6);
    public float distance = 5;

    int index = 0;
    bool isButtonDown = false;

    GameObject targetPlayer;
    Vector3 rayDirection = new Vector3(0,0,1);

    private Vector2 mouseAbsolute;
    private Vector2 smoothMouse;

    private Vector2 mouseDelta;

    List<Character> players = new List<Character>();

    // 활성화 될 때 현재 같은 팀에있는 플레이어의 리스트를 받는다 -> 중간 탈주 고려
    void OnEnable()
    {
        targetPlayer = gameObject.transform.parent.gameObject;

        Character[] allPlayer = FindObjectsOfType<Character>();

        foreach (var player in allPlayer)
        {
            if (CompareTag(player.gameObject.tag))
            {
                players.Add(player);
            }
        }

    }

    void OnMouseButton()
    {
        if (!isButtonDown)
        {
            index++;
            if (index < 0) index = players.Count - 1;
            if (index >= players.Count) index = 0;
            targetPlayer = players[index].gameObject;
            isButtonDown = !isButtonDown;
        }
    }

    private void Update()
    {
        TPSView();
    }

    void TPSView()
    {
        mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x, sensitivity.y));

        smoothMouse.x = Mathf.Lerp(smoothMouse.x, mouseDelta.x, 1f / sensitivity.x);
        smoothMouse.y = Mathf.Lerp(smoothMouse.y, mouseDelta.y, 1f / sensitivity.y);

        mouseAbsolute += smoothMouse;

        // 회전값을 Quaternion으로 변환
        Quaternion xQuaternion = Quaternion.AngleAxis(-mouseAbsolute.y, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(mouseAbsolute.x, Vector3.up);

        // Ray의 방향을 회전값으로 설정
        rayDirection = xQuaternion * yQuaternion * Vector3.forward;

        Ray ray = new Ray(targetPlayer.transform.position, rayDirection);
        RaycastHit hit;

        if(Physics.Raycast(ray,out hit,distance,LayerMask.NameToLayer("Ground") | LayerMask.NameToLayer("Wall")))
        {
            transform.position = hit.point;
        }

        else
        {
            transform.position = ray.GetPoint(distance);
        }

        transform.LookAt(targetPlayer.transform);
    }
}
