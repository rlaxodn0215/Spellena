using UnityEngine;

namespace DefineDatas
{
    public enum PlayerAniLayerIndex
    {
        BaseLayer,
        MoveLayer,
        AttackLayer
    }
    // ��� ����
    public enum NodeState
    {
        Running,
        Success,
        Failure
    }
    // ��� ����
    public enum NodeType
    {
        NONE,
        Sequence,
        Selector,
        Parallel,
        Condition,
        Action
    }
    // ��忡 �����ϴ� ������ ����
    public enum NodeData
    {
        NodeStatus,
        FixNode
    }
    // Action�� �ʿ��� object
    public enum ActionObjectName
    {
        CharacterTransform,
        CharacterTree,
        BowAniObject,
        ArrowAniObject,
        AimingTransform,
        ArrowStrikeStartPoint,
        DownArrowTransform,
        OccupationPoint
    }
    // Action ����
    public enum ActionName
    {
        GotoOccupationArea,
        NormalArrowAttack,
        BallArrowAttack,
        ArrowRainAttack,
        NONE
    }
    public enum AvoidWay
    {
        Forward = 0x0001,
        Back = 0x0010,
        Left = 0x0100,
        Right = 0x1000,
    }
    public enum PoolObjectName
    {
        Arrow,
        ArrowExplode,
        ArrowStuck,
        Ball,
        BallExplode,
        Strike,
        StrikeExplode,
        StrikeStuck
    }

    static class Offset
    {
        public static Vector3 AimOffset = new Vector3(0.0f, 0.5f, 0.0f);
    }

    static class CharacterName
    {
        public static string Character_1                             = "Aeterna";
        public static string Character_2                             = "Aloy";
    }

    static class LayerMaskName
    {
        public static string Player                                 = "Player";
        public static string Wall                                   = "Wall";
    }

    static class TagName
    {
        public static string TeamA                                  = "TeamA";
        public static string TeamB                                  = "TeamB";
    }

    static class PlayerLookAtWeight
    {
        public static float weight                                  = 1.0f;
        public static float bodyWeight                              = 0.9f;
    }

    static class PlayerAniState
    {
        public readonly static int Move                             = Animator.StringToHash("Move");
        public readonly static int CheckEnemy                       = Animator.StringToHash("CheckEnemy");
        public readonly static int AvoidRight                       = Animator.StringToHash("AvoidRight");
        public readonly static int AvoidLeft                        = Animator.StringToHash("AvoidLeft");
        public readonly static int AvoidForward                     = Animator.StringToHash("AvoidForward");
        public readonly static int AvoidBack                        = Animator.StringToHash("AvoidBack");
        public readonly static int Aim                              = Animator.StringToHash("Aim");
        public readonly static int Draw                             = Animator.StringToHash("Draw");
        public readonly static int Shoot                            = Animator.StringToHash("Shoot");
    }

    static class DefineNumber
    {
        public static int RandomInitNum                             = 7;
        public static int AvoidWayCount                             = 4;
        public static int BitMove4                                  = 4;
        public static float ZeroCount                               = 0.0f;
        public static float MaxWallDistance                         = 0.5f;
        public static float AvoidRotateAngle                        = 15.0f;
        public static float AttackAngleDifference                   = 10.0f;
        public static float SightHeightRatio                        = 1.5f;
    }

    public static class Logging
    {
        // PlayerSettings�� ScriptCompilation���� ENABLE_DEBUG�� �߰� �Ǿ�� �Լ� ȣ��
        [System.Diagnostics.Conditional("ENABLE_DEBUG")]
        static public void Log(object message)
        {
            Debug.Log(message);
        }
    }

    public enum ErrorCode
    {
        // NULL ERROR
        AbilityMaker_Tree_NULL,
        AbilityMaker_data_NULL,
        AloyBT_animator_NULL,
        AloyBT_abilityMaker_NULL,
        AloyBT_aimingTrasform_NULL,
        EnemyDetector_condition_NULL,
        EnemyDetector_playerTransform_NULL,
        EnemyDetector_bowAnimator_NULL,
        EnemyDetector_animator_NULL,
        NormalArrowAttack_playerTransform_NULL,
        NormalArrowAttack_attackTransform_NULL,
        NormalArrowAttack_bowAnimator_NULL,
        NormalArrowAttack_arrowAniObj_NULL,
        NormalArrowAttack_agent_NULL,
        NormalArrowAttack_animator_NULL,
        NormalArrowAttack_avoidTimer_NULL,
        GotoOccupationArea_arrowAniObj_NULL,
        GotoOccupationArea_agent_NULL,
        GotoOccupationArea_animator_NULL,

        // PoolManager ERROR
        NoSpawnPoolObjects,
        CannotFindPoolObjectName,
        CannotFindPoolObjectID
    }
}
