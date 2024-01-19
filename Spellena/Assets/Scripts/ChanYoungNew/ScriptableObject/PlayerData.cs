using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObject/PlayerData")]
public class PlayerData : ScriptableObject
{
    public List<float> skillCastingTime;
    public List<float> skillChannelingTime;
    public List<float> skillLifeTime;
    public List<float> skillDistance;
}
