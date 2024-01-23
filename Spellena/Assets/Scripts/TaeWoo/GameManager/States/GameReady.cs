using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCenterDataType;

namespace FSM
{
    public class GameReady : BaseState
    {
        public GameReadyStandardData gameReadyStandardData;
        public GameReady(StateMachine stateMachine) :
            base("GameReady", stateMachine)
        {
            gameReadyStandardData.readyTime = 1f;
        }

        public override void Enter() 
        {
            ((GameCenter0)stateMachine).globalTimer.globalDesiredTime 
                = Time.time + gameReadyStandardData.readyTime;
        }
        public override void Update() 
        { 
            if(Time.time >= ((GameCenter0)stateMachine).globalTimer.globalDesiredTime)
            {
                stateMachine.ChangeState(((GameCenter0)stateMachine).GameStates[GameState.DuringRound]);
            }
        }
        public override void Exit() { }
    }
}