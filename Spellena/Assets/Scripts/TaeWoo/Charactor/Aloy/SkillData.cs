using UnityEngine;

namespace BehaviorTree
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObject/SkillData")]
    public class SkillData : ScriptableObject
    {
        public float avoidSpeed;
        public float avoidTiming;
        public float rotateSpeed;
        public float coolTime;
    }
}