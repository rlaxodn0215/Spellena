using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Data", menuName ="ScriptableObject/CharactorData")]
public class CharacterData : ScriptableObject
{
    public int Hp;
    public float moveSpeed;
    public float jumpHeight;
    public float[] skillTimer;
}
