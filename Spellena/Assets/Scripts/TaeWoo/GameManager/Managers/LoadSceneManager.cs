using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using FSM;
using GameCenterDataType;

public class LoadSceneManager : MonoBehaviourPunCallbacks
{
    public static string nextScene;
    public static List<int> redTeamActorNums = new List<int>();
    public static List<int> blueTeamActorNums = new List<int>();

    public float loadingTime = 5.0f;

    private GameCenter0 gameCenter;
    private SettingManager settingManager;

    public static void LoadNextScene(string sceneName, string loadingSceneName,
        List<int> redTeam, List<int> blueTeam)
    {
        nextScene = sceneName;
        redTeamActorNums = redTeam;
        blueTeamActorNums = blueTeam;

        PhotonNetwork.LoadLevel(loadingSceneName);
    }

    public static void GoBackToMenu(string mainSceneName, string mainLoadingScene)
    {
        nextScene = mainSceneName;
        PhotonNetwork.LoadLevel(mainLoadingScene);
    }

    private void Awake()
    {
        gameCenter = GameObject.Find("GameCenter").GetComponent<GameCenter0>();
    }

    void OnEnable()
    {
        ((LoadingScene)gameCenter.GameStates[GameState.LoadingScene]).redTeamActorNums = redTeamActorNums;
        ((LoadingScene)gameCenter.GameStates[GameState.LoadingScene]).blueTeamActorNums = blueTeamActorNums;
        gameCenter.ChangeState(gameCenter.GameStates[GameState.LoadingScene]);

        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0.0f;

        while(!op.isDone)
        {
            timer += Time.deltaTime;
            yield return null;

            if(op.progress >= 0.9f)
            {
                if(timer >= loadingTime)
                {
                    gameCenter.ChangeState(gameCenter.GameStates[GameState.CharacterSelect]);
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }


}
