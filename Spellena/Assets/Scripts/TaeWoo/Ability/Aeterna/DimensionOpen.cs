using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class DimensionOpen : Ability
    {
        private Aeterna Player;
        public DimensionOpen(Aeterna player)
        {
            Player = player;
        }

        public void Execution()
        {
            Debug.Log("DimensionOpen");
        }
    }
}