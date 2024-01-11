using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviourTree;
using Managers;

public class AloyBasicAttack : Node
{
    public enum MoveWay
    {
        Forward,
        Back,
        Left,
        Right
    }

    private MoveWay way;
    private float avoidSpeed = 1.2f;
    private Animator bowAnimator;
    private GameObject arrowAniObj;

    private PoolManager basicAttackArrows;

    private Transform playerTransform;
    private Transform attackTransform;
    private Transform enemyTransform;

    private CheckGauge coolTime;
    private NavMeshAgent agent;
    private Animator animator;

    private float avoidTiming = 1.0f;
    private float rotateSpeed = 7.5f;

    private CheckGauge checkAvoid;

    public AloyBasicAttack() { }

    public AloyBasicAttack(Transform _playerTransform, Transform _attackTransform, GameObject _bowAniObj, GameObject _arrowAniObj,
        GameObject _arrowPool, CheckGauge _coolTime)
    {
        playerTransform = _playerTransform;
        attackTransform = _attackTransform;

        bowAnimator = _bowAniObj.GetComponent<Animator>();
        if (bowAnimator == null) Debug.LogError("bowAnimator가 할당되지 않았습니다");
        agent = playerTransform.GetComponent<NavMeshAgent>();
        if (agent == null) Debug.LogError("NavMeshAgent가 할당되지 않았습니다");
        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator가 할당되지 않았습니다");

        coolTime = _coolTime;
        arrowAniObj = _arrowAniObj;

        basicAttackArrows = _arrowPool.transform.GetChild(0).GetComponent<PoolManager>();
        if (basicAttackArrows == null) Debug.LogError("basicAttackArrows 할당되지 않았습니다");

        checkAvoid = new CheckGauge(avoidTiming);
    }

    public override NodeState Evaluate()
    {
        if (GetData("Enemy") != null)
        {
            enemyTransform = (Transform)GetData("Enemy");
            Avoiding();
            Attack();
        }

        else
        {
            Debug.LogError("적이 할당되지 않았습니다");
        }

        return NodeState.Running;
    }

    void Avoiding()
    {
        agent.isStopped = true;
        animator.SetBool("Move", false);
        checkAvoid.UpdateCurCoolTime();
        Moving();
    }

    void Moving()
    {
        if (checkAvoid.CheckCoolTime())
        {
            checkAvoid.UpdateCurCoolTime(0.0f);
            way = (MoveWay)Random.Range(0, 4);

            animator.SetBool("AvoidForward", false);
            animator.SetBool("AvoidBack", false);
            animator.SetBool("AvoidLeft", false);
            animator.SetBool("AvoidRight", false);
        }

        switch (way)
        {
            // Forward
            case MoveWay.Forward:
                playerTransform.Translate(avoidSpeed * 0.5f * Vector3.forward * Time.deltaTime);
                animator.SetBool("AvoidForward", true);
                break;
            // Back
            case MoveWay.Back:
                playerTransform.Translate(avoidSpeed * 0.5f * Vector3.back * Time.deltaTime);
                animator.SetBool("AvoidBack", true);
                break;
            // Left
            case MoveWay.Left:
                playerTransform.Translate(avoidSpeed * Vector3.left * Time.deltaTime);
                animator.SetBool("AvoidLeft", true);
                break;
            // Right
            case MoveWay.Right:
                playerTransform.Translate(avoidSpeed * Vector3.right * Time.deltaTime);
                animator.SetBool("AvoidRight", true);
                break;
            default:
                Debug.LogError("잘못된 랜덤 수 발생!!");
                break;
        }
    }

    void Attack()
    {
        Vector3 dest = enemyTransform.position - playerTransform.position;
        dest.y = 0;
        float distance = dest.magnitude;
        attackTransform.localRotation = Quaternion.Euler(-distance * 0.125f,0,0);

        Vector3 targetDir = (enemyTransform.position - playerTransform.position).normalized;
        targetDir.y = 0;
        playerTransform.forward =
            Vector3.Lerp(playerTransform.forward, targetDir, rotateSpeed * Time.deltaTime);

        if (coolTime.CheckCoolTime())
        {
            coolTime.UpdateCurCoolTime(0.0f);
            Debug.Log("AloyBasicAttack to " + "<color=magenta>"
            + enemyTransform.name + "</color>");
            bowAnimator.SetBool("Shoot", true);
            animator.SetBool("Shoot", true);

            basicAttackArrows.GetObject(attackTransform);

            if(bowAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shoot"))
            {
                arrowAniObj.SetActive(false);
            }
        }

        else
        {
            bowAnimator.SetBool("Shoot", false);
            animator.SetBool("Shoot", false);

            if (bowAnimator.GetCurrentAnimatorStateInfo(0).IsName("Draw"))
            {
                arrowAniObj.SetActive(true);
            }

            else
            {
                arrowAniObj.SetActive(false);
            }
        }
    }
}