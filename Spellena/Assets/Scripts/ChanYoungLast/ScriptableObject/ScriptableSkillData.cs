using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimatorInfo;

[CreateAssetMenu(fileName = "SkillData", menuName = "Datas/SkillData")]
public class ScriptableSkillData : ScriptableObject
{
    public float skillCoolDownTime;
    public float skillCastingTime;
    public float skillChannelingTime;
    public float distance;
    public float scale;
}
