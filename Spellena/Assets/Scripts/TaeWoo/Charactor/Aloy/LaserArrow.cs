using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using CoroutineMaker;
using DefineDatas;

public class LaserArrow : AbilityNode
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

    private GameObject beamParticle;
    private GameObject beamParticleHit;
    private Vector3[] particleScale = new Vector3[3];
    private float colliderHeight;
    private Vector3 colliderCenter;

    private NavMeshAgent agent;
    private Animator animator;

    private float avoidTiming = 1.0f;
    private float rotateSpeed = 8.0f;
    private float range = 8.5f;

    private CoolTimer avoidTimer;

    private MakeCoroutine coroutine;

    private RaycastHit hit;
    private CapsuleCollider collider;


    public LaserArrow(BehaviorTree.Tree tree, AbilityMaker abilityMaker, float coolTime): base(tree, NodeName.Skill_3, coolTime)
    {
        playerTransform = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.CharacterTransform];
        attackTransform = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.AimingTransform].GetChild(2);
        bowAnimator = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.BowAniObject].GetComponent<Animator>();
        arrowAniObj = abilityMaker.abilityObjectTransforms[(int)AbilityMaker.AbilityObjectName.ArrowAniObject].gameObject;
        beamParticle = attackTransform.transform.GetChild(0).gameObject;
        beamParticleHit = attackTransform.transform.GetChild(1).gameObject;
        agent = playerTransform.GetComponent<NavMeshAgent>();
        animator = playerTransform.GetComponent<Animator>();
        collider = attackTransform.GetChild(0).GetComponent<CapsuleCollider>();
        avoidTimer = new CoolTimer(avoidTiming);
        colliderHeight = collider.height;
        colliderCenter = collider.center;
        for(int i = 0; i < 3; i++)
        {
            particleScale[i] = beamParticle.transform.GetChild(i).localScale;
        }
    }

    public override NodeState Evaluate()
    {
        if (((AloyBT)tree).lookTransform != null)
        {
            Avoiding();
            Attack();
            SetDataToRoot(DataContext.NodeStatus, this);
            RangeChanging();
            return NodeState.Running;
        }

        else
        {
            Debug.LogError("적이 할당되지 않았습니다");
            return NodeState.Failure;
        }
    }

    void RangeChanging()
    {
        Ray ray = new Ray(attackTransform.position, attackTransform.forward);
        beamParticleHit.transform.position = hit.point;

        if (Physics.Raycast(ray, out hit, range))
        {
            collider.height = hit.distance / range * colliderHeight;
            collider.center
                = new Vector3(colliderCenter.x, colliderCenter.y, hit.distance / range * colliderCenter.z);

            Debug.DrawLine(ray.origin, hit.point, Color.green);
            for (int i = 0; i< 3; i++)
            {
                beamParticle.transform.GetChild(i).localScale
                    = new Vector3(particleScale[i].x, hit.distance / range * particleScale[i].y, particleScale[i].z);
            }
        }

        else
        {
            collider.height = colliderHeight;
            collider.center = colliderCenter;

            for (int i = 0; i < 3; i++)
            {
                beamParticle.transform.GetChild(i).localScale = particleScale[i];
            }
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
        if (avoidTimer.IsCoolTimeFinish() &&
            animator.GetCurrentAnimatorStateInfo(2).IsName(PlayerAniState.Aim))
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
        Vector3 targetDir = ((AloyBT)tree).lookTransform.position - playerTransform.position;
        float distance = targetDir.magnitude;
        targetDir.Normalize();
        targetDir.y = 0;
        playerTransform.forward =
            Vector3.Lerp(playerTransform.forward, targetDir, rotateSpeed * Time.deltaTime);

        if (coolTimer.IsCoolTimeFinish() && distance <= range)
        {
            coolTimer.ChangeCoolTime(0.0f);

            Debug.Log("AloyPurifyBeam to " + "<color=magenta>"
            + ((AloyBT)tree).lookTransform.name + "</color>");

            coroutine = MakeCoroutine.Start_Coroutine(ShootBeam());
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

    IEnumerator ShootBeam()
    {
        SetDataToRoot(DataContext.FixNode, this);

        bowAnimator.SetBool("Draw", true);
        animator.SetBool("CheckEnemy", true);

        beamParticle.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        beamParticleHit.SetActive(true);

        yield return new WaitForSeconds(4.3f);
        beamParticleHit.SetActive(false);
        yield return new WaitForSeconds(0.7f);
        beamParticle.SetActive(false);

        yield return new WaitForSeconds(0.8f);

        ClearData(DataContext.FixNode);
        coroutine.Stop();

    }
}
