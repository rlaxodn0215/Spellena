using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class DimensionIO : Ability
    {
        private Aeterna Player;
        public DimensionIO(Aeterna player)
        {
            Player = player;
        }

        public void Execution()
        {
            Debug.Log("DimensionIO");
        }
    }
}