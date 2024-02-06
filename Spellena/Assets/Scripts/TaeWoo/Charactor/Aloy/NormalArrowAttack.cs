using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using Managers;
using DefineDatas;
public class NormalArrowAttack : AbilityNode
{
    enum AvoidWay
    {
        Forward         =   0x0001,
        Back            =   0x0010,
        Left            =   0x0100,
        Right           =   0x1000,
    }

    private uint checkAvoidWay = 0x0000;
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
    private AvoidWay way = AvoidWay.Forward;
    private Vector3 avoidDir = Vector3.zero;
    private Ray ray = new Ray();

    public NormalArrowAttack(BehaviorTree.Tree tree, AbilityMaker abilityMaker, float coolTime) : base(tree,NodeName.Skill_1, coolTime)
    {
        playerTransform = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.CharacterTransform];
        if (playerTransform == null) ErrorDataMaker.SaveErrorData(ErrorCode.NormalArrowAttack_playerTransform_NULL);

        attackTransform = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.AimingTransform].GetChild(0);
        if (attackTransform == null) ErrorDataMaker.SaveErrorData(ErrorCode.NormalArrowAttack_attackTransform_NULL);

        bowAnimator = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.BowAniObject].GetComponent<Animator>();
        if (bowAnimator == null) ErrorDataMaker.SaveErrorData(ErrorCode.NormalArrowAttack_bowAnimator_NULL);

        arrowAniObj = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.ArrowAniObject].gameObject;
        if (arrowAniObj == null) ErrorDataMaker.SaveErrorData(ErrorCode.NormalArrowAttack_arrowAniObj_NULL);

        agent = playerTransform.GetComponent<NavMeshAgent>();
        if (agent == null) ErrorDataMaker.SaveErrorData(ErrorCode.NormalArrowAttack_agent_NULL);

        animator = playerTransform.GetComponent<Animator>();
        if (animator == null) ErrorDataMaker.SaveErrorData(ErrorCode.NormalArrowAttack_animator_NULL);

        avoidSpeed = abilityMaker.data.avoidSpeed;
        avoidTiming = abilityMaker.data.avoidTiming;
        rotateSpeed = abilityMaker.data.rotateSpeed;

        avoidTimer = new CoolTimer(avoidTiming);
        if (avoidTimer == null) ErrorDataMaker.SaveErrorData(ErrorCode.NormalArrowAttack_avoidTimer_NULL);
    }
    public override NodeState Evaluate()
    {
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
        avoidTimer.UpdateCoolTime(Time.deltaTime);
        agent.isStopped = true;
        animator.SetBool(PlayerAniState.Move, false);
        if (avoidTimer.IsCoolTimeFinish())
        {
            avoidTimer.ChangeCoolTime(DefineNumber.ZeroCount);
            AvoidWayMove(way, false);
            way = CheckAvoidable();
        }
        else
        {
            AvoidWayMove(way, true);
        }
    }
    void AvoidWayMove(AvoidWay way, bool val)
    {
        switch (way)
        {
            // Forward
            case AvoidWay.Forward:
                playerTransform.Translate(avoidSpeed * Vector3.forward * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidForward, val);
                break;
            // Back
            case AvoidWay.Back:
                playerTransform.Translate(avoidSpeed * Vector3.back * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidBack, val);
                break;
            // Left
            case AvoidWay.Left:
                playerTransform.Translate(avoidSpeed * Vector3.left * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidLeft, val);
                break;
            // Right
            case AvoidWay.Right:
                playerTransform.Translate(avoidSpeed * Vector3.right * Time.deltaTime);
                animator.SetBool(PlayerAniState.AvoidRight, val);
                break;
        }
    }
    AvoidWay CheckAvoidable()
    {
        if ((checkAvoidWay & 0x1111) == 0x1111)
        {
            // 4방향이 막혀있을 때 캐릭터 회전 후 재탐색
            playerTransform.Rotate(Vector3.up, DefineNumber.AvoidRotateAngle);
            checkAvoidWay = 0x0000;
        }
        AvoidWay temp = (AvoidWay)(1 << Random.Range(0, DefineNumber.AvoidWayCount) * DefineNumber.BitMove4);
        if ((checkAvoidWay & (uint)temp) == (uint)temp) return CheckAvoidable();
        switch (temp)
        {
            case AvoidWay.Forward: avoidDir = playerTransform.forward; break;
            case AvoidWay.Back: avoidDir = -playerTransform.forward; break;
            case AvoidWay.Left: avoidDir = -playerTransform.right; break;
            case AvoidWay.Right: avoidDir = playerTransform.right; break;
        }
        ray.origin = playerTransform.position + Vector3.up;
        ray.direction = avoidDir;
        if (Physics.Raycast(ray, DefineNumber.MaxWallDistance))
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
        playerTransform.forward = Vector3.Lerp(playerTransform.forward, targetDir, rotateSpeed * Time.deltaTime);
        bool isDrawing;
        if (coolTimer.IsCoolTimeFinish() && 
            animator.GetCurrentAnimatorStateInfo(PlayerAniLayerIndex.AttackLayer).IsName(PlayerAniState.Aim) &&
            Mathf.Acos(Vector3.Dot(playerTransform.forward, targetDir)) * Mathf.Rad2Deg <= DefineNumber.AttackAngleDifference)
        {
            coolTimer.ChangeCoolTime(DefineNumber.ZeroCount);
            Debug.Log("AloyBasicAttack to " + "<color=magenta>"+ ((AloyBT)tree).lookTransform.name + "</color>");
            bowAnimator.SetBool(PlayerAniState.Shoot, true);
            animator.SetBool(PlayerAniState.Shoot, true);
            PoolManager.Instance.GetObject(PoolObjectName.Arrow, attackTransform.position, attackTransform.rotation);
            isDrawing = !bowAnimator.GetCurrentAnimatorStateInfo(PlayerAniLayerIndex.BaseLayer).IsName(PlayerAniState.Shoot);
        }
        else
        {
            bowAnimator.SetBool(PlayerAniState.Shoot, false);
            animator.SetBool(PlayerAniState.Shoot, false);
            isDrawing = bowAnimator.GetCurrentAnimatorStateInfo(PlayerAniLayerIndex.BaseLayer).IsName(PlayerAniState.Draw);
        }
        arrowAniObj.SetActive(isDrawing);
    }
}