using UnityEngine;

namespace DefineDatas
{
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
        public static string Move                                   = "Move";
        public static string CheckEnemy                             = "CheckEnemy";
        public static string AvoidRight                             = "AvoidRight";
        public static string AvoidLeft                              = "AvoidLeft";
        public static string AvoidForward                           = "AvoidForward";
        public static string AvoidBack                              = "AvoidBack";
        public static string Aim                                    = "Aim";
        public static string Draw                                   = "Draw";
        public static string Shoot                                  = "Shoot";
    }

    static class PlayerAniLayerIndex
    {
        public static int BaseLayer                                 = 0;
        public static int MoveLayer                                 = 1;
        public static int AttackLayer                               = 2;
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

    static class ErrorCode
    {
        public static int AbilityMaker_Tree_NULL                    = 0;
        public static int AbilityMaker_data_NULL                    = 1;
        public static int AloyBT_animator_NULL                      = 2;
        public static int AloyBT_abilityMaker_NULL                  = 3;
        public static int AloyBT_aimingTrasform_NULL                = 4;
        public static int EnemyDetector_condition_NULL              = 5;
        public static int EnemyDetector_playerTransform_NULL        = 6;
        public static int EnemyDetector_bowAnimator_NULL            = 7;
        public static int EnemyDetector_animator_NULL               = 8;
        public static int NormalArrowAttack_playerTransform_NULL    = 9;
        public static int NormalArrowAttack_attackTransform_NULL    = 10;
        public static int NormalArrowAttack_bowAnimator_NULL        = 11;
        public static int NormalArrowAttack_arrowAniObj_NULL        = 12;
        public static int NormalArrowAttack_agent_NULL              = 13;
        public static int NormalArrowAttack_animator_NULL           = 14;
        public static int NormalArrowAttack_avoidTimer_NULL         = 15;
        public static int GotoOccupationArea_arrowAniObj_NULL       = 16;
        public static int GotoOccupationArea_agent_NULL             = 17;
        public static int GotoOccupationArea_animator_NULL          = 18;
    }

}
