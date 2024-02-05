using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using DefineDatas;
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
        NormalArrowAttack,
        BallArrowAttack,
        LaserArrowAttack,
        ArrowRainAttack,
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
        if (tree == null)ErrorDataMaker.SaveErrorData(ErrorCode.AbilityMaker_Tree_NULL);
        data = tree.data;
        if (data == null)ErrorDataMaker.SaveErrorData(ErrorCode.AbilityMaker_data_NULL);
    }
    public AbilityNode MakeFunction(FunctionName functionName)
    {
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
        switch (skillName)
        {
            case SkillName.NormalArrowAttack:
                return new NormalArrowAttack(tree, this, data.coolTimes[(int)skillName]);
            case SkillName.BallArrowAttack:
                return new BallArrowAttack(tree, this, data.coolTimes[(int)skillName]);
            case SkillName.LaserArrowAttack:
                return new LaserArrowAttack(tree, this, data.coolTimes[(int)skillName]);
            case SkillName.ArrowRainAttack:
                return new ArrowRainAttack(tree, this, data.coolTimes[(int)skillName]);
            case SkillName.NONE:
                return new AbilityNode();
        }
        return null;
    }
}
