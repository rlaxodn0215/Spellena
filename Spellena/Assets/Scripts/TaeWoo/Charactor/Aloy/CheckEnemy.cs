using UnityEngine;
using System;
public class CheckEnemy
{
    private Transform playerTransform;

    private float viewAngle;
    private float viewRadius;
    private LayerMask targetMask;
    private LayerMask obstacleMask;

    private Transform enemy;
    public Transform Enemy { get { return enemy; } }
    

    public CheckEnemy()
    {
        viewAngle = 0;
        viewRadius = 0;
        targetMask = 0;
    }

    public CheckEnemy(Transform _playerTransform, float _viewAngle, float _viewRadius,
        LayerMask _targetMask, LayerMask _obstacleMask)
    {
        playerTransform = _playerTransform;
        viewAngle = _viewAngle;
        viewRadius = _viewRadius;
        targetMask = 1 << _targetMask;
        obstacleMask = 1 << _obstacleMask;
    }

    public bool EnemyInRange()
    {
        Vector3 SightPos = playerTransform.position + Vector3.up * 1.5f;
        float lookingAngle = playerTransform.eulerAngles.y;  //캐릭터가 바라보는 방향의 각도
        Vector3 rightDir = AngleToDir(playerTransform.eulerAngles.y + viewAngle * 0.5f);
        Vector3 leftDir = AngleToDir(playerTransform.eulerAngles.y - viewAngle * 0.5f);
        Vector3 lookDir = AngleToDir(lookingAngle);

        Debug.DrawRay(SightPos, rightDir * viewRadius, Color.blue);
        Debug.DrawRay(SightPos, leftDir * viewRadius, Color.blue);
        Debug.DrawRay(SightPos, lookDir * viewRadius, Color.cyan);

        Collider[] targets = Physics.OverlapSphere(SightPos, viewRadius, targetMask);

        if(targets.Length > 0)
        {
            foreach (Collider player in targets)
            {
                Vector3 targetPos = player.transform.position;
                Vector3 targetDir = (targetPos - SightPos).normalized;
                float targetAngle = Mathf.Acos(Vector3.Dot(lookDir, targetDir)) * Mathf.Rad2Deg;

                if (targetAngle <= viewAngle * 0.5f &&
                    !Physics.Raycast(SightPos, targetDir, viewRadius, obstacleMask))
                {
                    // 태그가 다른 적 구분
                    Debug.DrawLine(SightPos, targetPos, Color.red);
                    enemy = player.transform;
                    return true;
                }
            }
        }

        return false;

    }

    private Vector3 AngleToDir(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), 0f, Mathf.Cos(radian));
    }
}
