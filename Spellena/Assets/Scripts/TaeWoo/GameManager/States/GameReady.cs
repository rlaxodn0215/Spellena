using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    public class GameReady : BaseState
    {
        public GameReady(StateMachine stateMachine) :
            base("GameReady", stateMachine)
        {

        }
    }
}