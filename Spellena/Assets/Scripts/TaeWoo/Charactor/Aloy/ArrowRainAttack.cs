using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using CoroutineMaker;
using UnityEngine.AI;
using DefineDatas;
public class ArrowRainAttack : AbilityNode
{
    private Animator bowAnimator;
    private GameObject arrowAniObj;

    private Transform playerTransform;
    private Transform targetTransform;
    public Transform attackTransform;

    private GameObject ariseArrow;
    private GameObject ariseEnergy;
    private Transform downArrowPosObj;

    private Animator animator;
    private NavMeshAgent agent;

    private MakeCoroutine coroutine;
    private float rotateSpeed = 7.5f;

    public ArrowRainAttack(BehaviorTree.Tree tree, AbilityMaker abilityMaker, float coolTime): base(tree, NodeName.Skill_4, coolTime)
    {
        playerTransform = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.CharacterTransform];
        attackTransform = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.ArrowStrikeStartPoint];
        downArrowPosObj = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.DownArrowTransform];
        bowAnimator = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.BowAniObject].GetComponent<Animator>();
        arrowAniObj = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.ArrowAniObject].gameObject;
        ariseArrow = attackTransform.transform.GetChild(0).gameObject;
        ariseEnergy = attackTransform.transform.GetChild(1).gameObject;
        animator = playerTransform.GetComponent<Animator>();
        agent = playerTransform.GetComponent<NavMeshAgent>();
    }

    public override NodeState Evaluate()
    {
        if (((AloyBT)tree).lookTransform != null)
        {
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
        if (coolTimer.IsCoolTimeFinish() &&
            animator.GetCurrentAnimatorStateInfo(2).IsName(PlayerAniState.Aim))
        {
            coolTimer.ChangeCoolTime(0.0f);

            targetTransform = ((AloyBT)tree).lookTransform;
            Debug.Log("AloyArrowStrike to " + "<color=magenta>"
            + ((AloyBT)tree).lookTransform.name + "</color>");

            coroutine = MakeCoroutine.Start_Coroutine(ShootStrike());
        }

        else
        {
            Vector3 targetDir = (((AloyBT)tree).lookTransform.position - playerTransform.position).normalized;
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
        SetDataToRoot(DataContext.FixNode, this);

        animator.SetBool(PlayerAniState.AvoidRight, false);
        animator.SetBool(PlayerAniState.AvoidLeft, false);
        animator.SetBool(PlayerAniState.AvoidBack, false);
        animator.SetBool(PlayerAniState.AvoidForward, false);
        animator.SetBool(PlayerAniState.Move, false);
        ariseEnergy.SetActive(true);
        ((AloyBT)tree).lookTransform = attackTransform;

        yield return new WaitForSeconds(2.0f);

        ariseArrow.SetActive(true);
        ariseEnergy.SetActive(false);

        yield return new WaitForSeconds(0.75f);

        ClearData(DataContext.FixNode);
        ((AloyBT)tree).lookTransform = null;

        yield return new WaitForSeconds(3.0f);

        ariseArrow.SetActive(false);
        downArrowPosObj.gameObject.SetActive(true);

        downArrowPosObj.position = targetTransform.position;
        downArrowPosObj.rotation = Quaternion.Euler(90, 0, 0);
        downArrowPosObj.position += new Vector3(0, 25, 0);

        yield return new WaitForSeconds(10.0f);
        downArrowPosObj.gameObject.SetActive(false);

        coroutine.Stop();
    }
}
