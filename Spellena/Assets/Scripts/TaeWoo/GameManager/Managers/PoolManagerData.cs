using System.Collections.Generic;
using UnityEngine;
using Managers;

[CreateAssetMenu(fileName = "PoolManagerData", menuName = "ScriptableObject/PoolManagerData")]
public class PoolManagerData : ScriptableObject
{
    [System.Serializable]
    public struct PoolObjectInitData
    {
        public PoolObjectName name;
        public GameObject obj;
        public int initObjectNum;
    }

    [Header("PoolManager Data List")]
    public List<PoolObjectInitData> poolObjectDatas;
}
