using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using Managers;
using CoroutineMaker;
using DefineDatas;
public class BallArrowAttack : ActionNode
{
    public enum MoveWay
    {
        Forward,
        Back,
        Left,
        Right
    }

    private MoveWay way;
    private Animator bowAnimator;
    private GameObject arrowAniObj;

    private Transform playerTransform;
    private Transform attackTransform;

    private GameObject aimParticle;
    private NavMeshAgent agent;
    private Animator animator;

    private float avoidSpeed;
    private float avoidTiming;
    private float rotateSpeed;

    private CoolTimer avoidTimer;

    private MakeCoroutine coroutine;

    public BallArrowAttack(BehaviorTree.Tree tree, List<Transform> actionObjectTransforms, ScriptableObject data) : 
        base(tree, ActionName.BallArrowAttack, ((SkillData)data).coolTime)
    {
        playerTransform = actionObjectTransforms[(int)ActionObjectName.CharacterTransform];
        attackTransform = actionObjectTransforms[(int)ActionObjectName.AimingTransform].GetChild(1);
        bowAnimator = actionObjectTransforms[(int)ActionObjectName.BowAniObject].GetComponent<Animator>();
        arrowAniObj = actionObjectTransforms[(int)ActionObjectName.ArrowAniObject].gameObject;
        aimParticle = attackTransform.transform.GetChild(0).gameObject;
        agent = playerTransform.GetComponent<NavMeshAgent>();
        animator = playerTransform.GetComponent<Animator>();

        avoidSpeed = ((SkillData)data).avoidSpeed;
        avoidTiming = ((SkillData)data).avoidTiming;
        rotateSpeed = ((SkillData)data).rotateSpeed;

        avoidTimer = new CoolTimer(avoidTiming);
    }

    public override NodeState Evaluate()
    {
        if (((AloyBT)tree).lookTransform != null)
        {
            Avoiding();
            Attack();
            if(GetData(NodeData.FixNode) == this)
                SetDataToRoot(NodeData.NodeStatus, this);
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
        avoidTimer.UpdateCoolTime(Time.deltaTime);
        Moving();
    }

    void Moving()
    {
        if (avoidTimer.IsCoolTimeFinish())
        {
            avoidTimer.ChangeCoolTime(0.0f);
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
        Vector3 targetDir = (((AloyBT)tree).lookTransform.position - playerTransform.position).normalized;
        playerTransform.forward =
            Vector3.Lerp(playerTransform.forward, targetDir, rotateSpeed * Time.deltaTime);

        if (coolTimer.IsCoolTimeFinish() &&
            animator.GetCurrentAnimatorStateInfo(2).IsName("Aim"))
        {
            coolTimer.ChangeCoolTime(0.0f);

            Debug.Log("AloyPreciseShot to " + "<color=magenta>"
            + ((AloyBT)tree).lookTransform.name + "</color>");

            coroutine = MakeCoroutine.Start_Coroutine(MutipleShoot());
        }

        else
        {
            bowAnimator.SetBool(PlayerAniState.Shoot, false);
            animator.SetBool(PlayerAniState.Shoot, false);

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

    IEnumerator MutipleShoot()
    {
        SetDataToRoot(NodeData.FixNode, this);

        bowAnimator.SetBool(PlayerAniState.Draw, true);
        animator.SetBool(PlayerAniState.CheckEnemy, true);

        aimParticle.SetActive(true);

        yield return new WaitForSeconds(0.8f);

        for (int i = 0; i < 5; i++)
        {
            PoolManager.Instance.GetObject(CharacterName.Character_2, PoolObjectName.Ball,attackTransform.position, attackTransform.rotation);
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(0.2f);
        aimParticle.SetActive(false);

        yield return new WaitForSeconds(0.8f);

        ClearData(NodeData.FixNode);
        coroutine.Stop();

    }

}
