using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class AbilityMaker : MonoBehaviour
{
    public enum AbilityObjectName
    {
        CharacterTransform,
        CharacterTree,
        BowAniObject,
        ArrowAniObject,
        AimingTransform,
        ArrowStrikeStartPoint,
        DownArrowTransform,
        OccupationPoint,
    }

    public enum SkillName
    {
        ShootNormalArrow,
        ShootBallArrow,
        LaserArrow,
        ArrowRain,
        NONE
    }

    public enum FunctionName
    {
        GotoOccupationArea,
        NONE
    }

    public List<Transform> abilityObjectTransforms;

    [HideInInspector]
    public BehaviorTree.CharacterData data;
    private BehaviorTree.Tree tree;

    private void Awake()
    {
        InitalizeSkillData();
    }

    void InitalizeSkillData()
    {
        tree = GetComponent<BehaviorTree.Tree>();
        if (tree == null)
        {
            Debug.LogError("Tree가 할당되지 않았습니다");
            return;
        }

        data = tree.data;
        if (data == null)
        {
            Debug.LogError("Data가 할당되지 않았습니다");
            return;
        }
    }

    public AbilityNode MakeFunction(FunctionName functionName)
    {
        if (tree == null) return null;
        switch (functionName)
        {
            case FunctionName.GotoOccupationArea:
                return new GotoOccupationArea(tree, this, -1);
            case FunctionName.NONE:
                return new AbilityNode();
        }
        return null;
    }

    public AbilityNode MakeSkill(SkillName skillName)
    {
        if (tree == null || data == null) return null;
        switch (skillName)
        {
            case SkillName.ShootNormalArrow:
                return new ShootNormalArrow(tree, this, data.coolTimes[(int)skillName]);
            case SkillName.ShootBallArrow:
                return new ShootBallArrow(tree, this, data.coolTimes[(int)skillName]);
            case SkillName.LaserArrow:
                return new LaserArrow(tree, this, data.coolTimes[(int)skillName]);
            case SkillName.ArrowRain:
                return new ArrowRain(tree, this, data.coolTimes[(int)skillName]);
            case SkillName.NONE:
                return new AbilityNode();
        }
        return null;
    }
}
