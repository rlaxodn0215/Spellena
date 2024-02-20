using UnityEngine;
using System.Collections.Generic;
using System;
using BehaviorTree;
using DefineDatas;
public class EnemyDetector: Condition
{
    public enum EnemyDetectType
    {
        CheckEnemyInSight
    }

    private Transform playerTransform;
    private Animator animator;
    private Animator bowAnimator;
    private float viewAngle;
    private float viewRadius;
    private LayerMask targetMask;
    private Ray ray;
    private RaycastHit hit;

    public EnemyDetector(BehaviorTree.Tree tree, Node _TNode, List<Transform> actionObjectTransforms)
        : base(tree, null, _TNode)
    {
        condition += CheckEnemyInSight;
        if (condition == null) ErrorManager.SaveErrorData(ErrorCode.EnemyDetector_condition_NULL);

        playerTransform = actionObjectTransforms[(int)ActionObjectName.CharacterTransform];
        if (playerTransform == null) ErrorManager.SaveErrorData(ErrorCode.EnemyDetector_playerTransform_NULL);

        bowAnimator = actionObjectTransforms[(int)ActionObjectName.BowAniObject].GetComponent<Animator>();
        if (bowAnimator == null) ErrorManager.SaveErrorData(ErrorCode.EnemyDetector_bowAnimator_NULL);

        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) ErrorManager.SaveErrorData(ErrorCode.EnemyDetector_animator_NULL);

        viewAngle = 120f;
        viewRadius = 35;
        targetMask = LayerMask.GetMask(LayerMaskName.Player);
        ray = new Ray();
    }

    public bool CheckEnemyInSight()
    {
        Vector3 SightPos = playerTransform.position + Vector3.up * DefineNumber.SightHeightRatio;
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
                ray.origin = SightPos; ray.direction = targetDir;

                if (targetAngle <= viewAngle * 0.5f && Physics.Raycast(ray, out hit, viewRadius))
                {
                    if (hit.collider.gameObject.layer != LayerMask.NameToLayer(LayerMaskName.Wall))
                    {
                        //
                        if (!player.gameObject.GetComponent<Player.Character>().isAlive) continue;
                        //
                        Debug.DrawLine(SightPos, targetPos, Color.red);
                        if (((AloyBT)tree).lookTransform == null)
                        {
                            ((AloyBT)tree).lookTransform = player.transform;
                            animator.SetBool(PlayerAniState.CheckEnemy, true);
                            bowAnimator.SetBool(PlayerAniState.Draw, true);
                        }
                        return true;
                    }
                }

            }
        }
        ((AloyBT)tree).lookTransform = null;
        animator.SetBool(PlayerAniState.CheckEnemy, false);
        bowAnimator.SetBool(PlayerAniState.Draw, false);
        return false;
    }
    private Vector3 AngleToDir(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), 0f, Mathf.Cos(radian));
    }
}
