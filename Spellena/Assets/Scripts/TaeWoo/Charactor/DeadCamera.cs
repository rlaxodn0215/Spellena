using System.Collections.Generic;
using UnityEngine;
using Player;
using System;

public class DeadCamera : MonoBehaviour
{
    [Header("Settings")]
    public Vector2 clampInDegrees = new Vector2(360, 120);
    public Vector2 Sensitivity = new Vector2(6, 6);
    public bool isOpposite = false;
    public float distance = 2;

    [HideInInspector]
    public DeathCamUI deathCamUI;

    int index = 0;
    GameObject targetPlayer;
    Vector3 rayDirection = new Vector3(0,0,1);
    Vector3 offset = new Vector3(0.0f, 1.5f, 0.0f);

    private Vector2 mouseAbsolute;
    private Vector2 smoothMouse;

    private Vector2 mouseDelta;

    private int layerMask;
    //private bool showSettings = false;

    List<Character> players = new List<Character>();
    private void Start()
    {
        deathCamUI = GameObject.Find("DeathUI").GetComponent<DeathCamUI>();

        layerMask = 1 << LayerMask.NameToLayer("Other");
        int temp = 1 << LayerMask.NameToLayer("OccupationArea");
        layerMask |= temp;
        int temp1 = 1 << LayerMask.NameToLayer("Me");
        layerMask |= temp1;

        layerMask = ~layerMask;
    }

    // 활성화 될 때 현재 같은 팀에있는 플레이어의 리스트를 받는다 -> 중간 탈주 고려
    void OnEnable()
    {
        targetPlayer = gameObject.transform.parent.gameObject;

        Character[] allPlayer = FindObjectsOfType<Character>();

        foreach (var player in allPlayer)
        {
            if (player.name == gameObject.name) continue;

            if (CompareTag(player.gameObject.tag))
            {
                players.Add(player);
            }
        }

    }

    private void Update()
    {
        if(SettingManager.Instance.SettingPanel.activeSelf)
        {
            SetData();
            ShowCursor();
        }

        else
        {
            EraseCursor();
            if (Input.GetMouseButtonDown(0))
            {
                ChangePlayerCam();
                ChangeCamera(targetPlayer.name);
            }

            TPSView();
        }
    }

    public void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void EraseCursor()
    {
        // 커서를 숨기고 고정시킨다.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SetData()
    {
        Sensitivity = new Vector2(SettingManager.Instance.sensitivityVal * 20, SettingManager.Instance.sensitivityVal * 20);
        isOpposite = SettingManager.Instance.isOpposite;
    }

    void ChangePlayerCam()
    {
        index++;
        if (index < 0) index = players.Count - 1;
        if (index >= players.Count) index = 0;
        targetPlayer = players[index].gameObject;
    }

    void ChangeCamera(string name)
    {
        deathCamUI.showPlayerCamID.text = name;
        deathCamUI.SwitchCam();
    }

    void TPSView()
    {
        if(isOpposite)
        {
            mouseDelta = new Vector2(-Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        }

        else
        {
            mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        }

        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(Sensitivity.x, Sensitivity.y));

        smoothMouse.x = Mathf.Lerp(smoothMouse.x, mouseDelta.x, 1f / Sensitivity.x);
        smoothMouse.y = Mathf.Lerp(smoothMouse.y, mouseDelta.y, 1f / Sensitivity.y);

        mouseAbsolute += smoothMouse;

        // 마우스 이동에 따는 ray 회전 
        float rotationY = -mouseAbsolute.x;
        float rotationX = mouseAbsolute.y;

        if (clampInDegrees.x < 360)
            mouseAbsolute.x = Mathf.Clamp(mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

        if (clampInDegrees.y < 360)
            mouseAbsolute.y = Mathf.Clamp(mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

        // 회전값을 Quaternion으로 변환
        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.right);
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.up);

        // Ray의 방향을 회전값으로 설정
        rayDirection = yQuaternion * xQuaternion * Vector3.forward;

        Ray ray = new Ray(targetPlayer.transform.position + offset, rayDirection);
        RaycastHit hit;

        if(Physics.Raycast(ray,out hit,distance,layerMask))
        {
            transform.position = hit.point;
        }

        else
        {
            transform.position = ray.GetPoint(distance);
        }

        transform.LookAt(targetPlayer.transform.position + offset);
    }
}
