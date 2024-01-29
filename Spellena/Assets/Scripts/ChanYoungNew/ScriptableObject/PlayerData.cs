using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObject/PlayerData")]
public class PlayerData : ScriptableObject
{
    public int dataHp;
    public float dataHeadShotRatio;
    public float dataSpeed;

    public List<float> skillCastingTime;
    public List<float> skillChannelingTime;
    public List<float> skillLifeTime;
    public List<float> skillDistance;

    public List<float> plainCastingTime;
    public List<float> plainChannelingTime;
    public List<float> plainLifeTime;
    public List<float> plainDistance;

}
