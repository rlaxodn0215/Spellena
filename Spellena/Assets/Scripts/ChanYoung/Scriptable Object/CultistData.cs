using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/CultistData")]
public class CultistData : ScriptableObject
{
    public int hp;
    public float moveSpeed;
    public float backSpeed;
    public float sideSpeed;
    public float runSpeedRatio;
    public float sitSpeed;
    public float sitSideSpeed;
    public float sitBackSpeed;
    public float jumpHeight;
    public float headShotRatio;

    public float invocationCastingTime;
    public float lungeHoldingTime;
    public float lungeAttackTime;

    public float lungeAttackDamage;

    public float throwTime;
    public float skill1CastingTime;
    public float skill1Time;
    public float skill1Damage;
    public float skill1CoolDownTime;

    public float skill2CastingTime;
    public float skill2ChannelingTime;
    public float skill2CoolDownTime;

    public float skill3CastingTime;
    public float skill3ChannelingTime;
    public float skill3CoolDownTime;

    public float skill4CastingTime;
    public float skill4CoolDownTime;
}