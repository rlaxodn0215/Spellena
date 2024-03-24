using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCastingAuraComponent : MonoBehaviour
{
    private float maxDistance;
    private ParticleSystem castingAura;
    private LayerMask layerMaskMap;
    private LayerMask layerMaskWall;


    private void Start()
    {
        castingAura = GetComponent<ParticleSystem>();
        layerMaskMap = LayerMask.GetMask("Map");
        layerMaskWall = LayerMask.GetMask("Wall");
    }

    private void Update()
    {
        SetPosition();
    }

    private void FixedUpdate()
    {
        MaintainTime();
    }

    /*
    기능 : 캐스팅 오라의 위치를 매 프레임마다 설정
    */
    private void SetPosition()
    {
        if (castingAura.isPlaying)
        {
            Ray _frontRay = new Ray(transform.position, transform.forward);
            Ray _bottomRay;
            RaycastHit _hit;

            if (Physics.Raycast(_frontRay, out _hit, maxDistance, layerMaskMap | layerMaskWall))
                _bottomRay = new Ray(_hit.point, Vector3.down);
            else
                _bottomRay = new Ray(transform.position + transform.forward * maxDistance, Vector3.down);

            if (Physics.Raycast(_bottomRay, out _hit, Mathf.Infinity, layerMaskMap))
                transform.position = _hit.point + new Vector3(0, 0.02f, 0);
        }
    }

    /*
    기능 : 파티클 시스템을 계속 지속시킴
    */
    private void MaintainTime()
    {
        if(castingAura.isPlaying)
        {
            if (castingAura.time > 0.7f)
                castingAura.time = 0.3f;
        }
    }

    /*
    기능 : 캐스팅 오라 활성화
    */
    public void ActiveCastingAura(float distance, float scale)
    {
        maxDistance = distance;
        castingAura.Play(false);

        transform.localScale = new Vector3(scale, scale, scale);
    }

    /*
    기능 : 캐스팅 오라 비활성화
    */
    public void InactiveCastingAura()
    {
        maxDistance = 0f;
        castingAura.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
