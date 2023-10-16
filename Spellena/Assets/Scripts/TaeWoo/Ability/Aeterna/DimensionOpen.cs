using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class DimensionOpen : Ability
    {
        private Aeterna Player;
        //private GameObject

        public override void AddPlayer(Character player)
        {
            Player = (Aeterna)player;
        }

        public override void Execution()
        {
            Debug.Log("DimensionOpen");
        }
    }
}