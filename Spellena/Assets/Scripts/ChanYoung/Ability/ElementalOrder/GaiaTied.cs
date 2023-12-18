using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaiaTied
{
    public float maxDistance = 5f;
    float skillCoolDownTime = 12f;

    bool isReady = false;

    public GaiaTied(ElementalOrderData elementalOrderData)
    {
        maxDistance = elementalOrderData.gaiaTiedMaxDistace;
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
