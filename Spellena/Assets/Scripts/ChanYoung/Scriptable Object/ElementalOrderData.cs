using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/ElementalOrderData")]
public class ElementalOrderData : ScriptableObject
{
    //캐릭터 공통 데이터
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

    //스킬 데이터

    //스킬1
    public float ragnaEdgeFloorDamage;
    public float ragnaEdgeCylinderDamage;
    public float rangaEdgeCoolDownTime;
    public float ragnaEdgeCastingTime;
    public float ragnaEdgeFloorLifeTime;
    public float ragnaEdgeCylinderLifeTime;
    public string rangaEdgeCylinderDebuff;

    //스킬2
    public int burstFlareBullet;
    public float burstFlareDamage;
    public float burstFlareCoolDownTime;
    public float burstFlareCastingTime;
    public float burstFlareLifeTime;

    //스킬3
    public float[] gaiaTiedDamage;
    public float gaiaTiedCoolDownTime;
    public float gaiaTiedCastingTime;
    public float gaiaTiedMaxDistace;
    public float[] gaiaTiedLifeTime;

    //스킬4
    public float meteorStrikeCastingTime;
    public float meteorStrikeLifeTime;

    //스킬5
    public float terraBreakCastingTime;
    public float terraBreakLifeTime;

    //스킬6
    public float eterialStormCastingTime;
    public float eterialStormLifeTime;

}
