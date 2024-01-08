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
    private PoolManager arrowPool;

    private Transform playerTransform;
    private CheckEnemy checkEnemy;
    private CheckGauge coolTime;
    private NavMeshAgent agent;
    private Animator animator;

    private float avoidTiming = 1.0f;
    private float rotateSpeed = 7.5f;

    private CheckGauge checkAvoid;

    public AloyBasicAttack() { }

    public AloyBasicAttack(Transform _playerTransform, GameObject _bowAniObj, GameObject _arrowAniObj,
        GameObject _arrowPool, CheckEnemy _checkEnemy, CheckGauge _coolTime)
    {
        playerTransform = _playerTransform;
        bowAnimator = _bowAniObj.GetComponent<Animator>();
        if (bowAnimator == null) Debug.LogError("bowAnimator가 할당되지 않았습니다");
        agent = playerTransform.GetComponent<NavMeshAgent>();
        if (agent == null) Debug.LogError("NavMeshAgent가 할당되지 않았습니다");
        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator가 할당되지 않았습니다");
        checkEnemy = _checkEnemy;
        coolTime = _coolTime;
        arrowAniObj = _arrowAniObj;
        arrowPool = _arrowPool.GetComponent<PoolManager>();
        if (arrowPool == null) Debug.LogError("arrowPool 할당되지 않았습니다");

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
        Vector3 targetDir = (checkEnemy.Enemy.position - playerTransform.position).normalized;
        targetDir.y = 0;

        playerTransform.forward =
            Vector3.Lerp(playerTransform.forward, targetDir, rotateSpeed * Time.deltaTime);

        if (coolTime.CheckCoolTime())
        {
            coolTime.UpdateCurCoolTime(0.0f);
            Debug.Log("AloyBasicAttack to " + "<color=magenta>"
            + checkEnemy.Enemy.name + "</color>");
            bowAnimator.SetBool("Shoot", true);
            animator.SetBool("Shoot", true);

            arrowPool.GetObject(targetDir);

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