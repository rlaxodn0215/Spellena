using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviourTree;

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
    private float moveSpeed = 1.5f;

    private Transform playerTransform;
    private CheckEnemy checkEnemy;
    private CheckGauge coolTime;
    private NavMeshAgent agent;
    private Animator animator;

    private int damage = 0;

    private float avoidTiming = 0.3f;
    private float rotateSpeed = 7.5f;
    private CheckGauge checkAvoid;

    public AloyBasicAttack() { }

    public AloyBasicAttack(Transform _playerTransform, CheckEnemy _checkEnemy,
        CheckGauge _coolTime, int _damage)
    {
        playerTransform = _playerTransform;
        agent = playerTransform.GetComponent<NavMeshAgent>();
        if (agent == null) Debug.LogError("NavMeshAgent가 할당되지 않았습니다");
        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator가 할당되지 않았습니다");
        checkEnemy = _checkEnemy;
        coolTime = _coolTime;
        damage = _damage;

        checkAvoid = new CheckGauge(avoidTiming);
    }

    public override NodeState Evaluate()
    {
        if (checkEnemy.Enemy != null)
        {
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
                playerTransform.Translate(moveSpeed * 0.5f * Vector3.forward * Time.deltaTime);
                animator.SetBool("AvoidForward", true);
                break;
            // Back
            case MoveWay.Back:
                playerTransform.Translate(moveSpeed * 0.5f * Vector3.back * Time.deltaTime);
                animator.SetBool("AvoidBack", true);
                break;
            // Left
            case MoveWay.Left:
                playerTransform.Translate(moveSpeed * Vector3.left * Time.deltaTime);
                animator.SetBool("AvoidLeft", true);
                break;
            // Right
            case MoveWay.Right:
                playerTransform.Translate(moveSpeed * Vector3.right * Time.deltaTime);
                animator.SetBool("AvoidRight", true);
                break;
            default:
                Debug.LogError("잘못된 랜덤 수 발생!!");
                break;
        }
    }

    void Attack()
    {
        Vector3 targetDir = (checkEnemy.Enemy.position - playerTransform.position).normalized;
        targetDir.y = 0;

        playerTransform.forward =
            Vector3.Lerp(playerTransform.forward, targetDir, rotateSpeed * Time.deltaTime);

        if (coolTime.CheckCoolTime())
        {
            coolTime.UpdateCurCoolTime(0.0f);
            Debug.Log("AloyBasicAttack to " + "<color=magenta>"
            + checkEnemy.Enemy.name + "</color>");
            animator.SetBool("Shoot", true);
        }

        else
        {
            animator.SetBool("Shoot", false);
        }
    }
}