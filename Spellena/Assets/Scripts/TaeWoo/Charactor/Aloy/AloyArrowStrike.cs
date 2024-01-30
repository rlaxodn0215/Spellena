using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;
using CoroutineMaker;
using Managers;
using UnityEngine.AI;

public class AloyArrowStrike : Node
{
    private Animator bowAnimator;
    private GameObject arrowAniObj;

    private Transform playerTransform;
    public Transform attackTransform;
    private Transform enemyTransform;

    private GameObject ariseArrow;
    private GameObject ariseEnergy;
    private Transform downArrowPosObj;

    private Gauge coolTime;
    private Animator animator;
    private NavMeshAgent agent;

    private MakeCoroutine coroutine;
    private float rotateSpeed = 7.5f;

    public AloyArrowStrike(Transform _playerTransform, Transform _attackTransform, Transform _spawnPosObj,
        GameObject _bowAniObj, GameObject _arrowAniObj, Gauge _coolTime): base(NodeName.Skill_4, null)
    {
        playerTransform = _playerTransform;
        attackTransform = _attackTransform;
        downArrowPosObj = _spawnPosObj;

        ariseArrow = attackTransform.transform.GetChild(0).gameObject;
        ariseEnergy = attackTransform.transform.GetChild(1).gameObject;

        if (ariseArrow == null) Debug.LogError("ariseArrow�� �Ҵ���� �ʾҽ��ϴ�");

        bowAnimator = _bowAniObj.GetComponent<Animator>();
        if (bowAnimator == null) Debug.LogError("bowAnimator�� �Ҵ���� �ʾҽ��ϴ�");
        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator�� �Ҵ���� �ʾҽ��ϴ�");
        agent = playerTransform.GetComponent<NavMeshAgent>();
        if (agent == null) Debug.LogError("NavMeshAgent가 할당되지 않았습니다");

        coolTime = _coolTime;
        arrowAniObj = _arrowAniObj;
    }

    public override NodeState Evaluate()
    {
        if (GetData(DataContext.EnemyTransform) != null)
        {
            enemyTransform = ((CheckEnemy)GetData(DataContext.EnemyTransform)).enemyTransform;
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

    void Attack()
    {
        if (coolTime.IsCoolTimeFinish() &&
            animator.GetCurrentAnimatorStateInfo(2).IsName(PlayerAniState.Aim))
        {
            coolTime.ChangeCurCoolTime(0.0f);

            Debug.Log("AloyArrowStrike to " + "<color=magenta>"
            + enemyTransform.name + "</color>");

            coroutine = MakeCoroutine.Start_Coroutine(ShootStrike());
        }

        else
        {
            Vector3 targetDir = (enemyTransform.position - playerTransform.position).normalized;
            targetDir.y = 0;
            playerTransform.forward =
                Vector3.Lerp(playerTransform.forward, targetDir, rotateSpeed * Time.deltaTime);

            agent.isStopped = true;
            bowAnimator.SetBool(PlayerAniState.Shoot, false);
            animator.SetBool(PlayerAniState.Move, false);

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

    IEnumerator ShootStrike()
    {
        SetDataToRoot(DataContext.IsNoSkillDoing, this);
        SetDataToRoot(DataContext.NotSensingEnemy, this);

        animator.SetBool(PlayerAniState.AvoidRight, false);
        animator.SetBool(PlayerAniState.AvoidLeft, false);
        animator.SetBool(PlayerAniState.AvoidBack, false);
        animator.SetBool(PlayerAniState.AvoidForward, false);
        animator.SetBool(PlayerAniState.Move, false);

        ariseEnergy.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        ariseArrow.SetActive(true);
        ariseEnergy.SetActive(false);

        yield return new WaitForSeconds(0.75f);

        ClearData(DataContext.NotSensingEnemy);
        ClearData(DataContext.IsNoSkillDoing);

        yield return new WaitForSeconds(3.0f);

        ariseArrow.SetActive(false);
        downArrowPosObj.gameObject.SetActive(true);

        downArrowPosObj.position = enemyTransform.position;
        downArrowPosObj.rotation = Quaternion.Euler(90, 0, 0);
        downArrowPosObj.position += new Vector3(0, 25, 0);

        yield return new WaitForSeconds(10.0f);
        downArrowPosObj.gameObject.SetActive(false);

        coroutine.Stop();
    }
}
