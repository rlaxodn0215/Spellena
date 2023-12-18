using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerraBreak
{
    public float maxDistance = 5f;
    float skillCoolDownTime = 19f;

    bool isReady = false;

    public TerraBreak(ElementalOrderData elementalOrderData)
    {
        maxDistance = elementalOrderData.terraBreakMaxDistance;
    }

    public float GetSkillCoolDownTime()
    {
        return skillCoolDownTime;
    }

    public bool CheckReady()
    {
        return isReady;
    }

    public void Initialize()
    {
        isReady = true;
    }

    public void EndSkill()
    {
        isReady = false;
    }

}
