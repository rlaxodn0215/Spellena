using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Datas/CharacterData")]
public class ScriptableCharacterData : ScriptableObject
{
    public float hp;
    public float moveSpeed;
    public float runRate;
    public float jumpForce;
}
