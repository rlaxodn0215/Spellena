using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviourTree;
using Managers;
using CoroutineMaker;

public class AloyPreciseShot : Node
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

    private GameObject aimParticle;
    private Gauge coolTime;
    private NavMeshAgent agent;
    private Animator animator;

    private float avoidTiming = 1.0f;
    private float rotateSpeed = 7.5f;

    private Gauge checkAvoid;

    private MakeCoroutine coroutine;

    public AloyPreciseShot() { }

    public AloyPreciseShot(Transform _playerTransform, Transform _aimingTransform,
        GameObject _bowAniObj, GameObject _arrowAniObj, Gauge _coolTime) : base(NodeName.Skill_2, null)
    {
        playerTransform = _playerTransform;
        attackTransform = _aimingTransform.GetChild(1);
        aimParticle = attackTransform.transform.GetChild(0).gameObject;
        if(aimParticle == null) Debug.LogError("aimParticle�� �Ҵ���� �ʾҽ��ϴ�");

        bowAnimator = _bowAniObj.GetComponent<Animator>();
        if (bowAnimator == null) Debug.LogError("bowAnimator�� �Ҵ���� �ʾҽ��ϴ�");
        agent = playerTransform.GetComponent<NavMeshAgent>();
        if (agent == null) Debug.LogError("NavMeshAgent�� �Ҵ���� �ʾҽ��ϴ�");
        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator�� �Ҵ���� �ʾҽ��ϴ�");

        coolTime = _coolTime;
        arrowAniObj = _arrowAniObj;

        checkAvoid = new Gauge(avoidTiming);
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

    MoveWay CheckingMoveable()
    {
        MoveWay temp = MoveWay.Forward;
        bool notMoveable = true;

        while (notMoveable)
        {
            temp = (MoveWay)Random.Range(0, 4);
            Ray ray;

            switch (temp)
            {
                // Forward
                case MoveWay.Forward:
                    ray = new Ray(playerTransform.position + Vector3.up,
                        playerTransform.forward);
                    break;
                // Back
                case MoveWay.Back:
                    ray = new Ray(playerTransform.position + Vector3.up,
                        -playerTransform.forward);
                    break;
                // Left
                case MoveWay.Left:
                    ray = new Ray(playerTransform.position + Vector3.up,
                        -playerTransform.right);
                    break;
                // Right
                case MoveWay.Right:
                    ray = new Ray(playerTransform.position + Vector3.up,
                        playerTransform.right);
                    break;
                default:
                    Debug.LogError("잘못된 랜덤 수 발생!!");
                    ray = new Ray(playerTransform.position + Vector3.up,
                        playerTransform.forward);
                    break;
            }

            notMoveable = Physics.Raycast(ray, 0.5f, LayerMask.NameToLayer("Wall"));

        }


        return temp;
    }

    void Avoiding()
    {
        agent.isStopped = true;
        animator.SetBool(PlayerAniState.Move, false);
        checkAvoid.UpdateCurCoolTime(Time.deltaTime);
        Moving();
    }

    void Moving()
    {
        if (checkAvoid.IsCoolTimeFinish())
        {
            checkAvoid.UpdateCurCoolTime(0.0f);
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
                Debug.LogError("�߸��� ���� �� �߻�!!");
                break;
        }
    }

    void Attack()
    {
        Vector3 targetDir = (enemyTransform.position - playerTransform.position).normalized;
        playerTransform.forward =
            Vector3.Lerp(playerTransform.forward, targetDir, rotateSpeed * Time.deltaTime);

        if (coolTime.IsCoolTimeFinish() &&
            animator.GetCurrentAnimatorStateInfo(2).IsName(PlayerAniState.Aim))
        {
            coolTime.ChangeCurCoolTime(0.0f);

            Debug.Log("AloyPreciseShot to " + "<color=magenta>"
            + enemyTransform.name + "</color>");

            //preciseAttackArrows.GetObject(attackTransform);
            coroutine = MakeCoroutine.Start_Coroutine(MutipleShoot());
        }

        else
        {
            bowAnimator.SetBool(PlayerAniState.Shoot, false);
            animator.SetBool(PlayerAniState.Shoot, false);

            if (bowAnimator.GetCurrentAnimatorStateInfo(0).IsName(PlayerAniState.Draw))
            {
                arrowAniObj.SetActive(true);
            }

            else
            {
                arrowAniObj.SetActive(false);
            }

        }
    }

    IEnumerator MutipleShoot()
    {
        SetDataToRoot(DataContext.IsNoSkillDoing, this);

        bowAnimator.SetBool(PlayerAniState.Draw, true);
        animator.SetBool(PlayerAniState.CheckEnemy, true);

        aimParticle.SetActive(true);

        yield return new WaitForSeconds(0.8f);

        for (int i = 0; i < 5; i++)
        {
            PoolManager.Instance.GetObject(PoolObjectName.Ball,attackTransform.position, attackTransform.rotation);
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(0.2f);
        aimParticle.SetActive(false);

        yield return new WaitForSeconds(0.8f);

        ClearData(DataContext.IsNoSkillDoing);
        coroutine.Stop();

    }

}
