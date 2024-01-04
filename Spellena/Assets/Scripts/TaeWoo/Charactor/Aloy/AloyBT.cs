using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree;

public class AloyBT : BehaviourTree.Tree
{
    public Transform occupationPoint;

    //Start()
    protected override Node SetupTree()
    {
        CheckEnemy checkEnemy = new CheckEnemy(transform, 120f, 10f, LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Wall"));

        Node root = new Sequence(new List<Node>
        {
            new Condition(checkEnemy.EnemyInRange,
            new Selector(new List<Node>
            {
                new AloyBasicAttack(checkEnemy, new CheckGauge(2.0f), 10),
                new Condition(new CheckGauge(5.0f).CheckCoolTime,
                new AloyLaserAttack(checkEnemy, new CheckGauge(5.0f), 20), null)
            }),
            new GotoOccupationArea(transform, occupationPoint)
            )
        });

        return root;
    }
}
