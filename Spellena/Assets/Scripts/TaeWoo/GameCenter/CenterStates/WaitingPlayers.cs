using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class WaitingPlayers : CenterState
{
    private int tempNum = 2;
    public override void StateExecution()
    {
        gameCenter.gameStateString = "다른 플레이어 기다리는 중...";

        if (PhotonNetwork.CurrentRoom.PlayerCount >= tempNum)
        {
            gameCenter.globalTimer = 0.0f;

            SetPlayerDatas();

            gameCenter.globalUIObj = PhotonNetwork.Instantiate("TaeWoo/Prefabs/UI/GlobalUI", Vector3.zero, Quaternion.identity);
            gameCenter.globalUIView = gameCenter.globalUIObj.GetComponent<PhotonView>();
            gameCenter.globalUI = gameCenter.globalUIObj.GetComponent<GlobalUI>();

            gameCenter.playerSpawnPoints = PhotonNetwork.Instantiate("TaeWoo/Prefabs/PlayerSpawnPoints", Vector3.zero, Quaternion.identity);
            MakeSpawnPoint();

            gameCenter.currentGameState = GameCenterTest.GameState.DataLoading;

        }
    }

    void SetPlayerDatas()
    {
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            // 플레이어 이름, 캐릭터의 게임 오브젝트, 팀, 총 데미지 수, 킬 수, 죽은 수
            Hashtable playerData = new Hashtable();

            // 보여주는 데이터
            playerData.Add("Name", player.ActorNumber.ToString());
            playerData.Add("Team", "none");
            playerData.Add("TotalDamage", 0);
            playerData.Add("TotalHeal", 0);
            playerData.Add("KillCount", 0);
            playerData.Add("DeadCount", 0);
            playerData.Add("IsAlive", true);
            playerData.Add("AngelStatueCoolTime", 0.0f);
            playerData.Add("KillerName", null);

            // 보여주지 않는 데이터
            playerData.Add("CharacterViewID", 0);
            playerData.Add("ReSpawnTime", 0.0f);
            playerData.Add("SpawnPoint", new Vector3(0, 0, 0));

            // 동기화 되지 않고 마스터 클라이언트만 가지는 Parameter / 플레이어 사망시 사용
            playerData.Add("ParameterName", null);

            playerData.Add("DamagePart", null);
            playerData.Add("DamageDirection", null);
            playerData.Add("DamageForce", null);

            playerData.Add("PlayerAssistViewID", null);

            player.SetCustomProperties(playerData);
        }

    }

    void MakeSpawnPoint()
    {
        gameCenter.playerSpawnA = gameCenter.FindObject(gameCenter.playerSpawnPoints, "TeamA").GetComponentsInChildren<Transform>(true);
        gameCenter.playerSpawnB = gameCenter.FindObject(gameCenter.playerSpawnPoints, "TeamB").GetComponentsInChildren<Transform>(true);
    }


}
