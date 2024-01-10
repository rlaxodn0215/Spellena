using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;
using CoroutineMaker;
using Managers;

public class AloyArrowStrike : Node
{
    private Animator bowAnimator;
    private GameObject arrowAniObj;

    private Transform playerTransform;
    private Transform attackTransform;
    private GameObject ariseArrow;
    private GameObject downArrow;

    private PoolManager downArrows;

    private CheckEnemy checkEnemy;
    private CheckGauge coolTime;
    private Animator animator;

    private MakeCoroutine coroutine;

    public AloyArrowStrike() { }

    public AloyArrowStrike(Transform _playerTransform, Transform _attackTransform, GameObject _bowAniObj, GameObject _arrowAniObj,
         GameObject _arrowPool, CheckEnemy _checkEnemy, CheckGauge _coolTime)
    {
        playerTransform = _playerTransform;
        attackTransform = _attackTransform;
        ariseArrow = attackTransform.transform.GetChild(0).gameObject;
        if (ariseArrow == null) Debug.LogError("ariseArrow�� �Ҵ���� �ʾҽ��ϴ�");

        bowAnimator = _bowAniObj.GetComponent<Animator>();
        if (bowAnimator == null) Debug.LogError("bowAnimator�� �Ҵ���� �ʾҽ��ϴ�");
        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator�� �Ҵ���� �ʾҽ��ϴ�");

        checkEnemy = _checkEnemy;
        coolTime = _coolTime;
        arrowAniObj = _arrowAniObj;

        downArrows = _arrowPool.GetComponent<PoolManager>();
        if (downArrows == null) Debug.LogError("downArrows�� �Ҵ���� �ʾҽ��ϴ�");
    }

    public override NodeState Evaluate()
    {
        if (checkEnemy.Enemy != null)
        {
            Attack();
        }

        else
        {
            Debug.LogError("적이 할당되지 않았습니다");
        }

        return NodeState.Running;
    }

    void Attack()
    {
        if (coolTime.CheckCoolTime())
        {
            coolTime.UpdateCurCoolTime(0.0f);

            Debug.Log("AloyArrowStrike to " + "<color=magenta>"
            + checkEnemy.Enemy.name + "</color>");

            coroutine = MakeCoroutine.Start_Coroutine(ShootStrike());
        }

        else
        {
            bowAnimator.SetBool("Shoot", false);
            //animator.SetBool("Shoot", false);

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

    IEnumerator ShootStrike()
    {
        isNoSkillDoing = false;

        yield return new WaitForSeconds(0.8f);

        ariseArrow.SetActive(true);

        isNoSkillDoing = true;

        yield return new WaitForSeconds(3.0f);

        ariseArrow.SetActive(false);
        downArrow.SetActive(true);

        coroutine.Stop();
    }
}
