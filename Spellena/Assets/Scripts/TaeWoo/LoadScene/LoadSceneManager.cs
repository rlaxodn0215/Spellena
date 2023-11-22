using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    public static string nextScene;
    public float loadingTime = 5.0f;

    public static void LoadNextScene(string sceneName)
    {
        nextScene = sceneName;
        PhotonNetwork.LoadLevel("TaeWoo_LoadingScene");
    }

    void Start()
    {
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

            if(op.progress < 0.9f)
            {
                // 게이지 바 채움
                //progressbar.fillamount = op.progress
            }

            else
            {
                //progressbar.fillamount = Mathf.Lerp(0.9f,1f,timer)
                if(timer >= loadingTime)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
