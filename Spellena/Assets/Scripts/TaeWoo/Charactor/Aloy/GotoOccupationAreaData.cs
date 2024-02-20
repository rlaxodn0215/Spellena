using UnityEngine;

namespace BehaviorTree
{
    [CreateAssetMenu(fileName = "GotoOccupationAreaData", menuName = "ScriptableObject/GotoOccupationAreaData")]
    public class GotoOccupationAreaData : ScriptableObject
    {
        public float moveSpeed;
        public float rotateSpeed;
    }
}