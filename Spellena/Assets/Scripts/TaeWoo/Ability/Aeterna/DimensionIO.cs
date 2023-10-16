using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class DimensionIO : Ability
    {
        private Aeterna Player;

        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
        }

        public override void Execution()
        {
            Debug.Log("DimensionIO");
        }
    }
}