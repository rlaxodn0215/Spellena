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

    bool isReady = false;

    public void EndSkill()
    {
        isReady = false;
    }
    public bool CheckReady()
    {
        return isReady;
    }

    public void Initialize()
    {
        currentBullet = maxBullet;
        currentShootCoolDownTime = 0f;
        isReady = true;
    }

    public bool ShootBullet()
    {
        currentBullet--;
        currentShootCoolDownTime = shootCoolDownTime;
        if(currentBullet <= 0)
            return true;
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

    public void SetReady(bool isNewReady)
    {
        isReady = isNewReady;
    }

    public void SetBullet(int newBullet)
    {
        currentBullet = newBullet;
    }
}