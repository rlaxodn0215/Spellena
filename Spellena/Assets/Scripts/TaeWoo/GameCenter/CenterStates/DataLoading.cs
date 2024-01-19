using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace temp
{
    public class DataLoading : CenterState
    {
        bool isCheckTimer = false;
        float tempTimer = 0.0f;
        public override void StateExecution()
        {
            if (!isCheckTimer)
            {
                isCheckTimer = !isCheckTimer;
                tempTimer = GameCenterTest.globalTimer;
                gameCenter.globalDesiredTimer = tempTimer + gameCenter.loadingTime;
            }

            GameCenterTest.globalTimer += Time.deltaTime;

            // 맵 및 캐릭터 데이터 로딩

            if (GameCenterTest.globalTimer >= gameCenter.globalDesiredTimer)
            {
                gameCenter.currentGameState = GameCenterTest.GameState.CharacterSelect;
            }
        }


    }
}