using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class AloyBasicAttack : Node
{
    private CheckEnemy checkEnemy;
    private CheckGauge coolTime;

    private int damage = 0;
    
    public AloyBasicAttack() { }

    public AloyBasicAttack(CheckEnemy _checkEnemy, CheckGauge _coolTime, int _damage)
    {
        checkEnemy = _checkEnemy;
        coolTime = _coolTime;
        damage = _damage;
    }

    public override NodeState Evaluate()
    {
        if(checkEnemy.Enemy !=null)
            Debug.Log("AloyBasicAttack to " + "<color=blue>" + checkEnemy.Enemy.name + "</color>");
        return NodeState.Running;
    }
}
