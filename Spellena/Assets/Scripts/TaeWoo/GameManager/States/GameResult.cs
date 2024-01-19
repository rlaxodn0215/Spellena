using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    public class GameResult : BaseState
    {
        public GameResult(StateMachine stateMachine) :
            base("GameResult", stateMachine)
        {

        }
    }
}