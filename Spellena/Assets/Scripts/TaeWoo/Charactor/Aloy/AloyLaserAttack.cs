using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviourTree;

public class AloyLaserAttack : Node
{
    private Transform playerTransform;
    private CheckEnemy checkEnemy;
    private CheckGauge coolTime;
    private NavMeshAgent agent;

    private int damage = 0;

    public AloyLaserAttack() { }

    public AloyLaserAttack(Transform _playerTransform, CheckEnemy _checkEnemy,
        CheckGauge _coolTime, int _damage)
    {
        playerTransform = _playerTransform;
        agent = playerTransform.GetComponent<NavMeshAgent>();
        if (agent == null) Debug.LogError("NavMeshAgent가 할당되지 않았습니다");
        checkEnemy = _checkEnemy;
        coolTime = _coolTime;
        damage = _damage;
    }

    public override NodeState Evaluate()
    {
        if (checkEnemy.Enemy != null)
        {
            Avoiding();
            Attack();
        }

        else
        {
            Debug.LogError("적이 할당되지 않았습니다");
        }

        return NodeState.Running;
    }

    void Avoiding()
    {
        agent.isStopped = true;
    }

    void Attack()
    {
        if (coolTime.CheckCoolTime())
        {
            coolTime.UpdateCurCoolTime(0.0f);
            Debug.Log("AloyLaserAttack to " + "<color=magenta>"
                + checkEnemy.Enemy.name + "</color>");
        }
    }
}
