using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using Managers;
using DefineDatas;
public class ShootNormalArrow : AbilityNode
{
    enum AvoidWay
    {
        Forward =   0x0001,
        Back    =   0x0010,
        Left    =   0x0100,
        Right   =   0x1000,
    }

    private Animator bowAnimator;
    private GameObject arrowAniObj;
    private Transform playerTransform;
    private Transform attackTransform;
    private NavMeshAgent agent;
    private Animator animator;
    private float avoidSpeed;
    private float avoidTiming;
    private float rotateSpeed;
    private CoolTimer avoidTimer;
    private bool isNull;
    private AvoidWay way;
    private Vector3 avoidDir = Vector3.zero;
    private uint checkAvoidWay = 0x0000;
    private Ray ray = new Ray();

    public ShootNormalArrow(BehaviorTree.Tree tree, AbilityMaker abilityMaker, float coolTime) : base(tree,NodeName.Skill_1, coolTime)
    {
        playerTransform = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.CharacterTransform];
        attackTransform = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.AimingTransform].GetChild(0);
        bowAnimator = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.BowAniObject].GetComponent<Animator>();
        arrowAniObj = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.ArrowAniObject].gameObject;
        avoidSpeed = abilityMaker.data.avoidSpeed;
        avoidTiming = abilityMaker.data.avoidTiming;
        rotateSpeed = abilityMaker.data.rotateSpeed;
        agent = playerTransform.GetComponent<NavMeshAgent>();
        animator = playerTransform.GetComponent<Animator>();
        avoidTimer = new CoolTimer(avoidTiming);
        isNull = NullCheck();
    }

    public override NodeState Evaluate()
    {
        if (isNull) return NodeState.Failure;
        if (((AloyBT)tree).lookTransform != null)
        {
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
        avoidTimer.UpdateCoolTime(Time.deltaTime);
        Moving();
    }
    void Moving()
    {
        if (avoidTimer.IsCoolTimeFinish())
        {
            avoidTimer.ChangeCoolTime(0.0f);
            way = CheckAvoidable();
            Debug.Log((AvoidWay)way);
            animator.SetBool(PlayerAniState.AvoidForward, false);
            animator.SetBool(PlayerAniState.AvoidBack, false);
            animator.SetBool(PlayerAniState.AvoidLeft, false);
            animator.SetBool(PlayerAniState.AvoidRight, false);
        }

        switch (way)
        {
            // Forward
            case AvoidWay.Forward:
                playerTransform.Translate(avoidSpeed * Vector3.forward * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidForward, true);
                break;
            // Back
            case AvoidWay.Back:
                playerTransform.Translate(avoidSpeed * Vector3.back * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidBack, true);
                break;
            // Left
            case AvoidWay.Left:
                playerTransform.Translate(avoidSpeed * Vector3.left * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidLeft, true);
                break;
            // Right
            case AvoidWay.Right:
                playerTransform.Translate(avoidSpeed * Vector3.right * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidRight, true);
                break;
        }
    }

    AvoidWay CheckAvoidable()
    {
        if((checkAvoidWay & 0x1111) == 0x1111)
        {
            playerTransform.Rotate(Vector3.up, 10.0f);
            checkAvoidWay = 0x0000;
        }

        AvoidWay temp = (AvoidWay)(1 << Random.Range(0, 4) * 4);
        if((checkAvoidWay & (uint)temp) == (uint)temp) return CheckAvoidable();
        switch (temp)
        {
            case AvoidWay.Forward:  avoidDir = playerTransform.forward;     break;
            case AvoidWay.Back:     avoidDir = -playerTransform.forward;    break;
            case AvoidWay.Left:     avoidDir = -playerTransform.right;      break;
            case AvoidWay.Right:    avoidDir = playerTransform.right;       break;
        }
        ray.origin = playerTransform.position + Vector3.up;
        ray.direction = avoidDir;
        if(Physics.Raycast(ray, 1.5f))
        {
            checkAvoidWay |= (uint)temp;
            return CheckAvoidable();
        }
        else
        {
            checkAvoidWay = 0x0000;
            return temp;
        }
    }

    void Attack()
    {
        Vector3 targetDir = (((AloyBT)tree).lookTransform.position - playerTransform.position).normalized;
        targetDir.y = 0;
        playerTransform.forward =
            Vector3.Lerp(playerTransform.forward, targetDir, rotateSpeed * Time.deltaTime);

        bool isDrawing;

        if (coolTimer.IsCoolTimeFinish() && 
            animator.GetCurrentAnimatorStateInfo(2).IsName(PlayerAniState.Aim) &&
            Mathf.Acos(Vector3.Dot(playerTransform.forward, targetDir)) * Mathf.Rad2Deg <= 10.0f)
        {
            coolTimer.ChangeCoolTime(0.0f);
            Debug.Log("AloyBasicAttack to " + "<color=magenta>"+ ((AloyBT)tree).lookTransform.name + "</color>");
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

    bool NullCheck()
    {
        //if (animator == null)
        //{
        //    Debug.LogError("animator가 할당되지 않았습니다");
        //    return true;
        //}
        //if (abilityMaker == null)
        //{
        //    Debug.LogError("abilityMaker가 할당되지 않았습니다");
        //    return true;
        //}
        //if (aimingTrasform == null)
        //{
        //    Debug.LogError("aimingTrasform가 할당되지 않았습니다");
        //    return true;
        //}
        return false;
    }
}