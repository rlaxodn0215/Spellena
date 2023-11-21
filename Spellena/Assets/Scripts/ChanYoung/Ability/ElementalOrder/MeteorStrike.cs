using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorStrike
{
    public float maxDistance = 5f;

    float skillCoolDownTime = 15f;

    bool isReady = false;

    public float GetSkillCoolDownTime()
    {
        return skillCoolDownTime;
    }

    public void Initialize()
    {
        isReady = true;
    }

    public bool CheckReady()
    {
        return isReady;
    }

    public void EndSkill()
    {
        isReady = false;
    }
}
