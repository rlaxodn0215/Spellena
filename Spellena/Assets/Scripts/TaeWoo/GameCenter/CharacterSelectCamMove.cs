using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectCamMove : MonoBehaviour
{
    public GameObject routes;

    private List<Transform> points = new List<Transform>();
    private int currentPointIndex = 0;

    private float moveSpeed = 4.0f;
    private float rotationSpeed = 6.0f;

    void Start()
    {
        for (int i = 0; i < routes.transform.childCount; i++)
        {
            points.Add(routes.transform.GetChild(i));
        }

        StartCoroutine(FollowRoute());
    }

    IEnumerator FollowRoute()
    {
        while (currentPointIndex < points.Count)
        {
            // 끝만 lerp 적용
            if(currentPointIndex-1 == points.Count)
            {
                // 이동
                transform.position = Vector3.Lerp(transform.position, points[currentPointIndex].position, moveSpeed * Time.deltaTime);
                // 회전
                transform.rotation = Quaternion.Slerp(transform.rotation, points[currentPointIndex].rotation, rotationSpeed * Time.deltaTime);
            }

            else
            {
                transform.Translate(transform.forward*moveSpeed*Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, points[currentPointIndex].rotation, rotationSpeed * Time.deltaTime);
            }

            // 만약 현재 지점에 및 각에 도달했다면 다음 지점으로 이동
            if (Vector3.Distance(transform.position, points[currentPointIndex].position) < 0.1f
                && Vector3.Magnitude(Vector3.Cross(transform.TransformDirection(transform.localPosition),
                points[currentPointIndex].TransformDirection(points[currentPointIndex].localPosition))) < 0.01f)
            {
                SetCameraToCurrentPoint();
                currentPointIndex++;
            }

            yield return null;
        }
    }

    void SetCameraToCurrentPoint()
    {
        if (currentPointIndex < points.Count)
        {
            transform.position = points[currentPointIndex].position;
            transform.rotation = points[currentPointIndex].rotation;
        }
    }
}
