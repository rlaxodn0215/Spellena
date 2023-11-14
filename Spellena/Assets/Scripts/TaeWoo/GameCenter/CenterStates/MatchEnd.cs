using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchEnd : CenterState
{
    public override void StateExecution()
    {
        gameCenter.currentGameState = GameCenterTest.GameState.GameResult;
    }
}
