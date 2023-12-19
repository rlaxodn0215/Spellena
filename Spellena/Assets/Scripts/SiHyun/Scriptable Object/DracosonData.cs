using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DracosonData", menuName = "ScriptableObject/DracosonData")]
public class DracosonData : ScriptableObject
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

    public float dragonicSightHoldingTime0;
    public float dragonicSightHoldingTime1;
    public float dragonicSightHoldingTime2;
    public float dragonicSightHoldingTime3;
    public float dragonSightAttackTime;
    public float dragonSightChargePhase1Damage;
    public float dragonSightChargePhase2Damage;
    public float dragonSightChargePhase3Damage;
    public float skill1CastingTime;
    public float skill1ChannelingTime;
    public float skill1Damage;
    public float skill2CastingTime;
    public float skill2ChannelingTime;
    public float skill2Damage;
    public float skill3CastingTime;
    public float skill3HoldingTime;
    public float skill3ShieldGage;
    public float skill4CastingTime;
    public float skill4DurationTime;
    public float dragonicBreatheHoldingTime;
    public float dragonicBreatheDamage;

    public float dragonSightCoolDownTime;
    public float skill1CoolDownTime;
    public float skill2CoolDownTime;
    public float skill3CoolDownTime;
    public float skill4CoolDownTime;
}