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

    private Transform playerTransform;
    private Transform attackTransform;
    private Transform enemyTransform;

    private Gauge coolTime;
    private NavMeshAgent agent;
    private Animator animator;

    private float avoidTiming = 1.0f;
    private float rotateSpeed = 0.5f;

    private Gauge avoidTimer;

    public AloyBasicAttack() { }

    public AloyBasicAttack(Transform _playerTransform, Transform _aimingTransform,
        GameObject _bowAniObj, GameObject _arrowAniObj, Gauge _coolTime) : base(NodeName.Skill_1, null)
    {
        playerTransform = _playerTransform;
        attackTransform = _aimingTransform.GetChild(0);

        bowAnimator = _bowAniObj.GetComponent<Animator>();
        if (bowAnimator == null) Debug.LogError("bowAnimator가 할당되지 않았습니다");
        agent = playerTransform.GetComponent<NavMeshAgent>();
        if (agent == null) Debug.LogError("NavMeshAgent가 할당되지 않았습니다");
        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator가 할당되지 않았습니다");

        coolTime = _coolTime;
        arrowAniObj = _arrowAniObj;

        avoidTimer = new Gauge(avoidTiming);
    }

    public override NodeState Evaluate()
    {
        if (GetData(DataContext.EnemyTransform) != null)
        {
            enemyTransform = ((CheckEnemy)GetData(DataContext.EnemyTransform)).enemyTransform;
            Avoiding();
            Attack();
            SetDataToRoot(DataContext.NodeStatus, this);
            return NodeState.Running;
        }

        else
        {
            Debug.LogError("적이 할당되지 않았습니다");
            return NodeState.Failure;
        }
    }

    void Avoiding()
    {
        agent.isStopped = true;
        animator.SetBool(PlayerAniState.Move, false);
        avoidTimer.UpdateCurCoolTime(Time.deltaTime);
        Moving();
    }

    MoveWay CheckingMoveable()
    {
        MoveWay temp = MoveWay.Forward;
        bool notMoveable = true;

        while (notMoveable)
        {
            temp = (MoveWay)Random.Range(0, 4);
            Ray ray;
            Vector3 dir = Vector3.zero;

            switch (temp)
            {
                // Forward
                case MoveWay.Forward: dir = playerTransform.forward; break;
                case MoveWay.Back:dir = -playerTransform.forward; break;
                case MoveWay.Left:dir = -playerTransform.right; break;
                case MoveWay.Right:dir = playerTransform.right; break;
            }

            ray = new Ray(playerTransform.position + Vector3.up, dir);

            notMoveable = Physics.Raycast(ray, 0.5f, LayerMask.NameToLayer("Wall"));
        }


        return temp;
    }

    void Moving()
    {
        if (avoidTimer.IsCoolTimeFinish())
        {
            avoidTimer.UpdateCurCoolTime(0.0f);

            way = CheckingMoveable();

            animator.SetBool(PlayerAniState.AvoidForward, false);
            animator.SetBool(PlayerAniState.AvoidBack, false);
            animator.SetBool(PlayerAniState.AvoidLeft, false);
            animator.SetBool(PlayerAniState.AvoidRight, false);
        }

        switch (way)
        {
            // Forward
            case MoveWay.Forward:
                playerTransform.Translate(avoidSpeed * 0.5f * Vector3.forward * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidForward, true);
                break;
            // Back
            case MoveWay.Back:
                playerTransform.Translate(avoidSpeed * 0.5f * Vector3.back * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidBack, true);
                break;
            // Left
            case MoveWay.Left:
                playerTransform.Translate(avoidSpeed * Vector3.left * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidLeft, true);
                break;
            // Right
            case MoveWay.Right:
                playerTransform.Translate(avoidSpeed * Vector3.right * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidRight, true);
                break;
            default:
                Debug.LogError("잘못된 랜덤 수 발생!!");
                break;
        }
    }

    void Attack()
    {
        Vector3 targetDir = (enemyTransform.position - playerTransform.position).normalized;
        targetDir.y = 0;
        playerTransform.forward =
            Vector3.Lerp(playerTransform.forward, targetDir, rotateSpeed * Time.deltaTime);

        bool isDrawing;

        if (coolTime.IsCoolTimeFinish() && 
            animator.GetCurrentAnimatorStateInfo(2).IsName(PlayerAniState.Aim) &&
            Mathf.Acos(Vector3.Dot(playerTransform.forward, targetDir)) * Mathf.Rad2Deg <= 10.0f)
        {
            coolTime.ChangeCurCoolTime(0.0f);
            Debug.Log("AloyBasicAttack to " + "<color=magenta>"+ enemyTransform.name + "</color>");
            bowAnimator.SetBool(PlayerAniState.Shoot, true);
            animator.SetBool(PlayerAniState.Shoot, true);

            PoolManager.Instance.GetObject(PoolObjectName.Arrow, attackTransform.position, attackTransform.rotation);

            isDrawing = !bowAnimator.GetCurrentAnimatorStateInfo(0).IsName(PlayerAniState.Shoot);
        }
        else
        {
            bowAnimator.SetBool(PlayerAniState.Shoot, false);
            animator.SetBool(PlayerAniState.Shoot, false);
            isDrawing = bowAnimator.GetCurrentAnimatorStateInfo(0).IsName(PlayerAniState.Draw);

        }

        arrowAniObj.SetActive(isDrawing);
    }
}