using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using CoroutineMaker;
using GameCenterDataType;

namespace FSM
{
    public class LoadingScene : BaseState
    {
        public static string nextScene;
        public static List<int> redTeamActorNums = new List<int>();
        public static List<int> blueTeamActorNums = new List<int>();
        public float loadingTime = 5.0f;

        private GameObject betweenBGMObj;
        private SettingManager settingManager;
        private PhotonView photonView;

        public LoadingScene(StateMachine stateMachine) :
            base("LoadingScene", stateMachine)
        {
            photonView = stateMachine.gameObject.GetComponent<PhotonView>();
            if (photonView == null) Debug.LogError("Not Set photonView");
            ((GameCenter0)stateMachine).playerList.playersA = new List<PlayerStat>();
            ((GameCenter0)stateMachine).playerList.playersB = new List<PlayerStat>();
        }

        public static void LoadNextScene(string nextSceneName, string loadingSceneName,
            List<int> redTeam, List<int> blueTeam)
        {
            nextScene = nextSceneName;
            redTeamActorNums = redTeam;
            blueTeamActorNums = blueTeam;

            PhotonNetwork.LoadLevel(loadingSceneName);
        }

        public static void GoBackToMain(string mainSceneName, string mainLoadingScene)
        {
            nextScene = mainSceneName;
            PhotonNetwork.LoadLevel(mainLoadingScene);
        }

        public override void Enter()
        {
            if (PhotonNetwork.IsMasterClient) ToDoMaster();

            MakeCoroutine.Start_Coroutine(LoadSceneProcess());
            SetPlayerDatas();
        }

        void ToDoMaster()
        {
            photonView.RPC("SyncNextScene", RpcTarget.AllBufferedViaServer, nextScene);
        }

        [PunRPC]
        public void SyncNextScene(string sceneName)
        {
            nextScene = sceneName;
        }

        IEnumerator LoadSceneProcess()
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
            op.allowSceneActivation = false;

            float timer = 0.0f;

            while (!op.isDone)
            {
                timer += Time.deltaTime;
                yield return null;

                if (op.progress > 0.9f)
                {
                    if (timer >= loadingTime)
                    {
                        op.allowSceneActivation = true;
                        stateMachine.ChangeState(((GameCenter0)stateMachine).GameStates
                            [GameState.CharacterSelect]);
                        yield break;
                    }
                }
            }
        }

        void SetPlayerDatas()
        {
            for(int i = 0; i < redTeamActorNums.Count; i++)
            {
                int j;
                for(j  = 0; j < PhotonNetwork.PlayerList.Length; i++)
                {
                    if(PhotonNetwork.PlayerList[i].ActorNumber == redTeamActorNums[i])
                    {
                        ((GameCenter0)stateMachine).playerList.playersA
                            .Add(InitPlayerData(PhotonNetwork.PlayerList[i], i, true));
                        break;
                    }
                }
                if (j >= PhotonNetwork.PlayerList.Length)
                    Debug.LogError("액터넘버에 해당하는 플레이어 못 찾음");
            }

            for (int i = 0; i < blueTeamActorNums.Count; i++)
            {
                int j;
                for (j = 0; j < PhotonNetwork.PlayerList.Length; i++)
                {
                    if (PhotonNetwork.PlayerList[i].ActorNumber == blueTeamActorNums[i])
                    {
                        ((GameCenter0)stateMachine).playerList.playersB
                            .Add(InitPlayerData(PhotonNetwork.PlayerList[i], i, false));
                        break;
                    }
                }
                if (j >= PhotonNetwork.PlayerList.Length)
                    Debug.LogError("액터넘버에 해당하는 플레이어 못 찾음");
            }
        }

        PlayerStat InitPlayerData(Photon.Realtime.Player player, int index, bool isRedTeam)
        {
            PlayerStat playerData = new PlayerStat();

            playerData.index = index;
            playerData.name = player.NickName;
            playerData.player = player;

            if (isRedTeam) playerData.team = "A";
            else playerData.team = "B";

            playerData.isAlive = true;
            playerData.respawnTime = 10000000.0f;

            return playerData;
        }
    }
}