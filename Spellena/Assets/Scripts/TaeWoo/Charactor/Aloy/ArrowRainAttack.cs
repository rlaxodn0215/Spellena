using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using CoroutineMaker;
using UnityEngine.AI;
using DefineDatas;
public class ArrowRainAttack : ActionNode
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

    private float rotateSpeed;

    public ArrowRainAttack(BehaviorTree.Tree tree, List<Transform> actionObjectTransforms, ScriptableObject data)
        : base(tree, NodeType.Action, ((SkillData)data).coolTime)

    {
        playerTransform = actionObjectTransforms[(int)ActionObjectName.CharacterTransform];
        attackTransform = actionObjectTransforms[(int)ActionObjectName.ArrowStrikeStartPoint];
        downArrowPosObj = actionObjectTransforms[(int)ActionObjectName.DownArrowTransform];
        bowAnimator = actionObjectTransforms[(int)ActionObjectName.BowAniObject].GetComponent<Animator>();
        arrowAniObj = actionObjectTransforms[(int)ActionObjectName.ArrowAniObject].gameObject;
        ariseArrow = attackTransform.transform.GetChild(0).gameObject;
        ariseEnergy = attackTransform.transform.GetChild(1).gameObject;
        animator = playerTransform.GetComponent<Animator>();
        agent = playerTransform.GetComponent<NavMeshAgent>();

        rotateSpeed = ((SkillData)data).rotateSpeed;
    }

    public override NodeState Evaluate()
    {
        if (((AloyBT)tree).lookTransform != null)
        {
            Attack();
            SetDataToRoot(NodeData.NodeStatus, this);
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
        SetDataToRoot(NodeData.FixNode, this);

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

        ((AloyBT)tree).lookTransform = null;
        yield return new WaitForSeconds(0.5f);

        ClearData(NodeData.FixNode);
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
