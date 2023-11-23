using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// initPlayerData
public class InitPlayerData : CenterState
{
    public override void StateExecution()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameCenter.globalTimer = 0.0f;

            ConnectInGameUI();
            MakeSpawnPoint();

            gameCenter.currentGameState = GameCenterTest.GameState.CharacterSelect;
        }
    }

    void ConnectInGameUI()
    {
        if (gameCenter.inGameUIObj != null)
        {
            gameCenter.inGameUIView = gameCenter.inGameUIObj.GetComponent<PhotonView>();
            gameCenter.inGameUI = gameCenter.inGameUIObj.GetComponent<InGameUI>();
        }
    }


    void MakeSpawnPoint()
    {
        gameCenter.playerSpawnA = GameCenterTest.FindObject(gameCenter.playerSpawnPoints, "TeamA").GetComponentsInChildren<Transform>(true);
        gameCenter.playerSpawnB = GameCenterTest.FindObject(gameCenter.playerSpawnPoints, "TeamB").GetComponentsInChildren<Transform>(true);
    }

}
