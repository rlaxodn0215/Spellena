using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstFlare : MonoBehaviour
{
    int maxBullet = 5;
    int currentBullet = 0;

    public void Initialize()
    {
        currentBullet = maxBullet;
    }

    public bool ShootBullet(Vector3 startPos, Vector3 shootDirection)
    {
        currentBullet--;

        if(currentBullet <= 0)
        {
            return true;
        }
        return false;
    }
}