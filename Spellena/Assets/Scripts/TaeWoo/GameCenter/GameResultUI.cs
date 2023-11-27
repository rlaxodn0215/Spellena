using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameResultUI : MonoBehaviourPunCallbacks
{
    public GameObject charts;
    public GameObject scores;
    public GameObject ourTeamResult;

    Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();

    List<Text> friendyPlayerNames = new List<Text>();
    List<Text> friendyKDAs = new List<Text>();
    List<Text> friendyTotalDamages = new List<Text>();

    List<Text> enemyPlayerNames = new List<Text>();
    List<Text> enemyKDAs = new List<Text>();
    List<Text> enemyTotalDamages = new List<Text>();

    Text friendyScore;
    Text enemyScore;

    // Start is called before the first frame update
    void Start()
    {
        Init();
        LinkDatas();
        ShowUI();
    }

    void Init()
    {
       
        for (int i = 0; i < 5; i++)
        {
            UIObjects["playerName_" + i + "_Enemy"] = GameCenterTest.FindObject(charts, "PlayerName_" + i + "_Friendly");
            friendyPlayerNames.Add(UIObjects["playerName_" + i + "_Friendly"].GetComponent<Text>());

            UIObjects["KDA_"+i+"_Friendly"] = GameCenterTest.FindObject(charts, "KDA_" + i + "_Friendly");
            friendyKDAs.Add(UIObjects["KDA_" + i + "_Friendly"].GetComponent<Text>());

            UIObjects["totalDamage_"+i+"_Friendly"] = GameCenterTest.FindObject(charts, "TotalDamage_" + i + "_Friendly");
            friendyTotalDamages.Add(UIObjects["totalDamage_" + i + "_Friendly"].GetComponent<Text>());


            UIObjects["playerName_" + i + "_Enemy"] = GameCenterTest.FindObject(charts, "PlayerName_" + i + "_Enemy");
            enemyPlayerNames.Add(UIObjects["playerName_" + i + "_Enemy"].GetComponent<Text>());

            UIObjects["KDA_" + i + "_Enemy"] = GameCenterTest.FindObject(charts, "KDA_" + i + "_Enemy");
            enemyKDAs.Add(UIObjects["KDA_" + i + "_Enemy"].GetComponent<Text>());

            UIObjects["totalDamage_" + i + "_Enemy"] = GameCenterTest.FindObject(charts, "TotalDamage_" + i + "_Enemy");
            enemyTotalDamages.Add(UIObjects["totalDamage_" + i + "_Enemy"].GetComponent<Text>());
        }

        UIObjects["score_Friendly"] = GameCenterTest.FindObject(scores, "Score_Friendly");
        friendyScore = UIObjects["score_Friendly"].GetComponent<Text>();

        UIObjects["score_Enemy"] = GameCenterTest.FindObject(scores, "Score_Enemy");
        enemyScore = UIObjects["score_Enemy"].GetComponent<Text>();

        UIObjects["victory"] = GameCenterTest.FindObject(ourTeamResult, "Victory");
        UIObjects["defeat"] = GameCenterTest.FindObject(ourTeamResult, "Defeat");

    }

    void LinkDatas()
    {
        foreach(var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            //if()
        }
    }

    void ShowUI()
    {

    }



    public void GoToMain()
    {
        
    }
}
