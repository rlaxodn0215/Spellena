using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataLoading : CenterState
{
    bool isCheckTimer = false;
    float tempTimer = 0.0f;
    public override void StateExecution()
    {
        if (!isCheckTimer)
        {
            isCheckTimer = !isCheckTimer;
            tempTimer = gameCenter.globalTimer;
            gameCenter.globalDesiredTimer = tempTimer + gameCenter.loadingTime;
        }

        gameCenter.gameStateString = "데이터 불러오는 중...";

        gameCenter.globalTimer += Time.deltaTime;

        // 맵 및 캐릭터 데이터 로딩

        if (gameCenter.globalTimer >= gameCenter.globalDesiredTimer)
        {
            gameCenter.currentGameState = GameCenterTest.GameState.CharacterSelect;
        }
    }


}
