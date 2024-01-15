using UnityEngine;
using BehaviourTree;
public class CheckEnemy : Condition
{
    private Transform playerTransform;

    private Animator animator;
    private float viewAngle;
    private float viewRadius;
    private LayerMask targetMask;

    private Animator bowAnimator;
    
    public CheckEnemy() : base()
    {
        viewAngle = 0;
        viewRadius = 0;
        targetMask = 0;
    }

    public CheckEnemy(Node _TNode , Transform _playerTransform, GameObject _bowAniObj,
        float _viewAngle, float _viewRadius, LayerMask _targetMask) : base(null, _TNode)
    {
        condition += EnemyInRange;

        playerTransform = _playerTransform;
        bowAnimator = _bowAniObj.GetComponent<Animator>();
        if (bowAnimator == null) Debug.LogError("bowAnimator가 할당되지 않았습니다");
        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator가 할당되지 않았습니다");
        viewAngle = _viewAngle;
        viewRadius = _viewRadius;
        targetMask =  _targetMask;
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

                Ray ray = new Ray(SightPos, targetDir);
                RaycastHit hit;

                if (targetAngle <= viewAngle * 0.5f 
                    && Physics.Raycast(ray, out hit, viewRadius))
                {
                    if (hit.collider.gameObject.layer
                        == LayerMask.NameToLayer("Player"))
                    {
                        // 태그가 다른 적 구분
                        Debug.DrawLine(SightPos, targetPos, Color.red);
                        if (GetData("EnemyCheck") == null)
                        {
                            SetDataToRoot("Enemy", player.transform);
                            animator.SetBool("CheckEnemy", true);
                            bowAnimator.SetBool("Draw", true);
                        }

                        return true;
                    }
                }

            }
        }

        ClearData("Enemy");
        animator.SetBool("CheckEnemy", false);
        bowAnimator.SetBool("Draw", false);
        return false;
    }

    private Vector3 AngleToDir(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), 0f, Mathf.Cos(radian));
    }
}
