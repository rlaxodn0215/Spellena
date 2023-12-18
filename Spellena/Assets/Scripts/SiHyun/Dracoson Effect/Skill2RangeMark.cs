using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Skill2RangeMark : MonoBehaviour
{
    public Camera camera;
    // Start is called before the first frame update

    private void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        // 카메라의 전방 벡터를 이용하여 새로운 위치 계산
        Vector3 targetPosition = camera.transform.position + camera.transform.forward * 1.5f;

        // 오브젝트 위치 갱신
        targetPosition.y = Mathf.Max(targetPosition.y, 1.1f);
        transform.position = targetPosition;
    }
}
