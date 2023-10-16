using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class DimensionIO : Ability
    {
        private Aeterna Player;

        public override void AddPlayer(Charactor player)
        {
            Player = (Aeterna)player;
        }

        public override void Execution()
        {
            Debug.Log("DimensionIO");
        }
    }
}