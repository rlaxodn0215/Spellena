using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BalanceData", menuName = "BalanceDatas/BalanceData")]
public class BalanceData : ScriptableObject
{
    public int hp;

    public float moveSpeed;
    public float jumpForce;
    public float runRate;

    public List<float> skillCoolDownTime;
    public List<float> skillCastingTime;
    public List<float> skillChannelingTime;

    public List<float> plainCastingTime;
    public List<float> plainChannelingTime;
}
