using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class RagnaEdge
{
    float skillCoolDownTime = 10f;

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
