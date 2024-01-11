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
    private Transform enemyTransform;

    private GameObject ariseArrow;
    private PoolManager downArrows;

    private CheckGauge coolTime;
    private Animator animator;

    private MakeCoroutine coroutine;

    public AloyArrowStrike() { }

    public AloyArrowStrike(Transform _playerTransform, Transform _attackTransform, GameObject _bowAniObj, GameObject _arrowAniObj,
         GameObject _arrowPool, CheckGauge _coolTime)
    {
        playerTransform = _playerTransform;
        attackTransform = _attackTransform;
        ariseArrow = attackTransform.transform.GetChild(0).gameObject;
        if (ariseArrow == null) Debug.LogError("ariseArrow�� �Ҵ���� �ʾҽ��ϴ�");

        bowAnimator = _bowAniObj.GetComponent<Animator>();
        if (bowAnimator == null) Debug.LogError("bowAnimator�� �Ҵ���� �ʾҽ��ϴ�");
        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator�� �Ҵ���� �ʾҽ��ϴ�");

        coolTime = _coolTime;
        arrowAniObj = _arrowAniObj;

        downArrows = _arrowPool.transform.GetChild(2).GetComponent<PoolManager>();
        if (downArrows == null) Debug.LogError("downArrows�� �Ҵ���� �ʾҽ��ϴ�");
    }

    public override NodeState Evaluate()
    {
        if (GetData("Enemy") != null)
        {
            enemyTransform = (Transform)GetData("Enemy");
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
            + enemyTransform.name + "</color>");

            coroutine = MakeCoroutine.Start_Coroutine(ShootStrike());
        }

        else
        {
            bowAnimator.SetBool("Shoot", false);

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
        SetDataToRoot("IsNoSkillDoing", true);

        yield return new WaitForSeconds(0.8f);

        ariseArrow.SetActive(true);

        ClearData("IsNoSkillDoing");

        yield return new WaitForSeconds(3.0f);

        ariseArrow.SetActive(false);
        downArrows.gameObject.SetActive(true);

        downArrows.transform.position = enemyTransform.position;
        downArrows.transform.rotation = Quaternion.Euler(90, 0, 0);
        downArrows.transform.position += new Vector3(0, 25, 0);

        yield return new WaitForSeconds(10.0f);
        downArrows.gameObject.SetActive(false);

        coroutine.Stop();
    }
}
