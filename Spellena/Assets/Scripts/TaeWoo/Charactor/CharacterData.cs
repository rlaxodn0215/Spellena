using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Data", menuName ="ScriptableObject/CharactorData")]
public class CharacterData : ScriptableObject
{
    public int Hp;
    public float moveSpeed;
    public float jumpHeight;
    public float avoidSpeed;
    public float avoidTiming;
    public float rotateSpeed;

    public float basicAttackTime;

    public float skill1Time;
    public float skill1DoorRange;

    public float skill2DurationTime;
    public float skill2HoldTime;
    public float skill2CoolTime;

    public float skill3DurationTime;
    public float skill3CoolTime;

    public float skill4DurationTime;
}
