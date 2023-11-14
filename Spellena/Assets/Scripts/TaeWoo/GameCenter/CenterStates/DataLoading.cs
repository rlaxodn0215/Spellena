using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataLoading : CenterState
{
    public override void StateExecution()
    {
        gameCenter.gameStateString = "데이터 불러오는 중...";

        // 맵 및 캐릭터 데이터 로딩

        gameCenter.globalTimer -= Time.deltaTime;
        if (gameCenter.globalTimer <= 0.0f)
        {
            gameCenter.currentGameState = GameCenterTest.GameState.CharacterSelect;
            gameCenter.globalTimer = gameCenter.characterSelectTime;
        }
    }


}
