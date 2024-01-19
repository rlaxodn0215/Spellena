using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM
{
    public class InTheRoom : BaseState
    {
        public InTheRoom(StateMachine stateMachine) :
            base("InTheRoom", stateMachine)
        {

        }
    }
}