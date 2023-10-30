using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstFlare
{
    int maxBullet = 5;
    int currentBullet = 0;

    float shootCoolDownTime = 0.3f;
    float currentShootCoolDownTime = 0f;
    float skillCoolDownTime = 5f;

    public void Initialize()
    {
        currentBullet = maxBullet;
    }

    public bool ShootBullet()
    {
        currentBullet--;
        currentShootCoolDownTime = shootCoolDownTime;
        if(currentBullet <= 0)
        {
            return true;
        }
        return false;
    }

    public float GetSkillCoolDownTime()
    {
        return skillCoolDownTime;
    }

    public int CheckCurrentBullet()
    {
        return currentBullet;
    }

    public bool CheckCoolDown()
    {
        if (currentShootCoolDownTime <= 0f)
            return true;
        return false;
    }

    public void ShootCoolDown()
    {
        if (currentShootCoolDownTime > 0f)
        {
            currentShootCoolDownTime -= Time.deltaTime;
        }
    }
}