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
            ((GameCenter0)stateMachine).globalTimer.globalTime = 0.0f;
            ((GameCenter0)stateMachine).globalTimer.globalDesiredTime = gameReadyStandardData.readyTime;
        }

        public override void Update() 
        { 
            if(((GameCenter0)stateMachine).globalTimer.globalTime >= 
                ((GameCenter0)stateMachine).globalTimer.globalDesiredTime)
            {
                stateMachine.ChangeState(((GameCenter0)stateMachine).GameStates[GameState.DuringRound]);
            }
        }

        public override void Exit() { }
    }
}