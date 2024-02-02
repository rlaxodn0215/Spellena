using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefineDatas
{
    static class LayerMaskName
    {
        public static string Player = "Player";
        public static string Wall = "Wall";
    }

    static class PlayerLookAtWeight
    {
        public static float weight = 1.0f;
        public static float bodyWeight = 0.9f;
    }

    static class PlayerAniState
    {
        public static string Move = "Move";
        public static string CheckEnemy = "CheckEnemy";
        public static string AvoidRight = "AvoidRight";
        public static string AvoidLeft = "AvoidLeft";
        public static string AvoidForward = "AvoidForward";
        public static string AvoidBack = "AvoidBack";
        public static string Aim = "Aim";
        public static string Draw = "Draw";
        public static string Shoot = "Shoot";
    }

    static class DefineNumber
    {
        public static int RandonInitNum = 7;
    }

}
