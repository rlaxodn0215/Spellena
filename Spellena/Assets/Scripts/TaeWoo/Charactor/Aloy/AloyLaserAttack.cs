using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class AloyLaserAttack : Node
{
    private CheckEnemy checkEnemy;
    private CheckGauge coolTime;

    private int damage = 0;

    public AloyLaserAttack() { }

    public AloyLaserAttack(CheckEnemy _checkEnemy, CheckGauge _coolTime, int _damage)
    {
        checkEnemy = _checkEnemy;
        coolTime = _coolTime;
        damage = _damage;
    }

    public override NodeState Evaluate()
    {
        if (checkEnemy.Enemy != null)
            Debug.Log("AloyLaserAttack to " + "<color=red>" + checkEnemy.Enemy.name + "</color>");
        return NodeState.Running;
    }
}
