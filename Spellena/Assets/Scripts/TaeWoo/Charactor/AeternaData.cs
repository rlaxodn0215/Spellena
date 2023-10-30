using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AeternaData", menuName = "ScriptableObject/AeternaData")]
public class AeternaData : ScriptableObject
{
    [Tooltip("에테르나 체력")]
    public int Hp;

    [Tooltip("앉아 있을때 걷는 속도")]
    public float sitSpeed;
    [Tooltip("걷는 속도")]
    public float walkSpeed;
    [Tooltip("달리는 속도")]
    public float runSpeed;
    [Tooltip("점프 높이")]
    public float jumpHeight;
    
    [Tooltip("기본 공격 쿨 타임")]
    public float basicAttackTime;

    [Tooltip("기본 공격 수명")]
    public float DimenstionSlash_0_lifeTime;
    [Tooltip("기본 공격 데미지")]
    public int DimenstionSlash_0_Damage;
    [Tooltip("기본 공격 스피드")]
    public int DimenstionSlash_0_Speed;
    [Tooltip("기본 공격 힐량")]
    public int DimenstionSlash_0_Healing;

    [Tooltip("스킬1 포탈 수명")]
    public float skill1Time;
    [Tooltip("스킬1 포탈 쿨 타임")]
    public float skill1DoorCoolTime;
    [Tooltip("스킬1 포탈 소환 최대 고리")]
    public float skill1DoorSpawnMaxRange;
    [Tooltip("스킬1 포탈 적용 범위")]
    public float skill1DoorRange;
    [Tooltip("스킬1 포탈 디버프 비율")]
    public float skill1DeBuffRatio;

    [Tooltip("스킬2 지속 시간")]
    public float skill2DurationTime;
    [Tooltip("스킬2 투사체 가지고 있는 시간")]
    public float skill2HoldTime;
    [Tooltip("스킬2 쿨 타임")]
    public float skill2CoolTime;

    [Tooltip("스킬3 지속 시간")]
    public float skill3DurationTime;
    [Tooltip("스킬3 쿨 타임")]
    public float skill3CoolTime;

    [Tooltip("스킬4 지속 시간")]
    public float skill4DurationTime;

    [Tooltip("스킬4 1단계 도달 시간")]
    public float skill4Phase1Time;
    [Tooltip("1단계 공격 수명")]
    public float DimenstionSlash_1_lifeTime;
    [Tooltip("1단계 공격 데미지")]
    public int DimenstionSlash_1_Damage;
    [Tooltip("1단계 공격 스피드")]
    public int DimenstionSlash_1_Speed;
    [Tooltip("1단계 공격 힐량")]
    public int DimenstionSlash_1_Healing;

    [Tooltip("스킬4 2단계 도달 시간")]
    public float skill4Phase2Time;
    [Tooltip("2단계 공격 수명")]
    public float DimenstionSlash_2_lifeTime;
    [Tooltip("2단계 공격 데미지")]
    public int DimenstionSlash_2_Damage;
    [Tooltip("2단계 공격 스피드")]
    public int DimenstionSlash_2_Speed;
    [Tooltip("2단계 공격 힐량")]
    public int DimenstionSlash_2_Healing;

    [Tooltip("스킬4 3단계 도달 시간")]
    public float skill4Phase3Time;
    [Tooltip("3단계 공격 수명")]
    public float DimenstionSlash_3_lifeTime;
    [Tooltip("3단계 공격 데미지")]
    public int DimenstionSlash_3_Damage;
    [Tooltip("3단계 공격 스피드")]
    public int DimenstionSlash_3_Speed;
    [Tooltip("3단계 공격 힐량")]
    public int DimenstionSlash_3_Healing;
}

