using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameReady : CenterState
{
    bool isCheckTimer = false;
    float tempTimer = 0.0f;
    public override void StateExecution()
    {
        //Debug.Log("<color=blue>" + "GameReady" + "</color>");

        if (!isCheckTimer)
        {
            //Debug.Log("<color=green>" + "GameReady ONCE" + "</color>");
            isCheckTimer = !isCheckTimer;
            tempTimer = GameCenterTest.globalTimer;
            gameCenter.globalDesiredTimer = tempTimer + gameCenter.readyTime;
            gameCenter.bgmManagerView.RPC("PlayAudio", RpcTarget.AllBuffered, "DuringRound", 0.05f, true,true, "BGM");
        }

        GameCenterTest.globalTimer += Time.deltaTime;

        if (GameCenterTest.globalTimer >= gameCenter.globalDesiredTimer)
        {
            //Debug.Log("<color=yellow>" + "DuringRound start in GameReady" + "</color>");

            if (GameCenterTest.roundA == 0 && GameCenterTest.roundB == 0)
            {
                //Debug.Log("<color=red>" + "DuringRound start in GameReady ONCE" + "</color>");
                // 코루틴을 사용하여 데이터 손실 줄이기
                MakeEnemyShader();
                // (어시스트)
                StartCoroutine(SetAssistTimer());
                
            }

            else
            {
                gameCenter.currentGameState = GameCenterTest.GameState.DuringRound;
                isCheckTimer = !isCheckTimer;
            }
        }

    }

    void MakeEnemyShader()
    {
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            // 적 쉐이더 적용
            PhotonView view = PhotonView.Find((int)player.CustomProperties["CharacterViewID"]);
            if (view == null) return;
            view.RPC("SetEnemyLayer", player);
        }
    }

    IEnumerator SetAssistTimer()
    {
        // 어시스트 타이머 설정
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            yield return new WaitForSeconds(0.2f);

            Hashtable playerData = player.CustomProperties;
            // 어시스트 타이머 할당 -> AssistTime_ + 적 플레이어 캐릭터 ViewID
            if ((string)player.CustomProperties["Team"] == "A")
            {
                Dictionary<string, float> temp = new Dictionary<string, float>();
                Dictionary<string, float> temp1 = new Dictionary<string, float>();

                foreach (var enemyPlayer in gameCenter.playersB)
                {
                    if (temp.ContainsKey("AssistTime_" + (int)enemyPlayer.CustomProperties["CharacterViewID"])) continue;
                    temp.Add("AssistTime_" + (int)enemyPlayer.CustomProperties["CharacterViewID"], 0.0f);
                }

                if (!playerData.ContainsKey("DealAssist")) playerData.Add("DealAssist", temp);

                foreach (var enemyPlayer in gameCenter.playersA)
                {
                    if (temp1.ContainsKey("AssistTime_" + (int)enemyPlayer.CustomProperties["CharacterViewID"])) continue;
                    temp1.Add("AssistTime_" + (int)enemyPlayer.CustomProperties["CharacterViewID"], 0.0f);
                }

                if (!playerData.ContainsKey("HealAssist")) playerData.Add("HealAssist", temp1);

            }

            else if ((string)player.CustomProperties["Team"] == "B")
            {
                Dictionary<string, float> temp = new Dictionary<string, float>();
                Dictionary<string, float> temp1 = new Dictionary<string, float>();

                foreach (var enemyPlayer in gameCenter.playersA)
                {
                    if (temp.ContainsKey("AssistTime_" + (int)enemyPlayer.CustomProperties["CharacterViewID"])) continue;
                    temp.Add("AssistTime_" + (int)enemyPlayer.CustomProperties["CharacterViewID"], 0.0f);
                }

                if (!playerData.ContainsKey("DealAssist")) playerData.Add("DealAssist", temp);

                foreach (var enemyPlayer in gameCenter.playersB)
                {
                    if (temp1.ContainsKey("AssistTime_" + (int)enemyPlayer.CustomProperties["CharacterViewID"])) continue;
                    temp1.Add("AssistTime_" + (int)enemyPlayer.CustomProperties["CharacterViewID"], 0.0f);
                }

                if (!playerData.ContainsKey("HealAssist")) playerData.Add("HealAssist", temp1);
            }

            player.SetCustomProperties(playerData);
        }

        gameCenter.currentGameState = GameCenterTest.GameState.DuringRound;
        isCheckTimer = !isCheckTimer;
    }

}