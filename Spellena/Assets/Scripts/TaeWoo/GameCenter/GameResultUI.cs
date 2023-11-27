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

    int friendlyIndex = 0;
    int enemyIndex = 0;
    string team;
    bool isWin;

    // Start is called before the first frame update
    void Start()
    {
        Init();
        LinkDatas();
        ShowUI();
    }

    void Init()
    {
        if((string)PhotonNetwork.LocalPlayer.CustomProperties["Team"]=="A")
        {
            team = "A";
        }

        else
        {
            team = "B";
        }

        for (int i = 0; i < 5; i++)
        {
            UIObjects["player_" + i + "_Friendly"] = GameCenterTest.FindObject(charts, "Player" + i + "_Friendly");

            UIObjects["playerName_" + i + "_Friendly"] = GameCenterTest.FindObject(charts, "PlayerName_" + i + "_Friendly");
            friendyPlayerNames.Add(UIObjects["playerName_" + i + "_Friendly"].GetComponent<Text>());

            UIObjects["KDA_"+i+"_Friendly"] = GameCenterTest.FindObject(charts, "KDA_" + i + "_Friendly");
            friendyKDAs.Add(UIObjects["KDA_" + i + "_Friendly"].GetComponent<Text>());

            UIObjects["totalDamage_"+i+"_Friendly"] = GameCenterTest.FindObject(charts, "TotalDamage_" + i + "_Friendly");
            friendyTotalDamages.Add(UIObjects["totalDamage_" + i + "_Friendly"].GetComponent<Text>());


            UIObjects["player_" + i + "_Enemy"] = GameCenterTest.FindObject(charts, "Player" + i + "_Enemy");

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
            if((string)player.CustomProperties["Team"] == team)
            {
                friendyPlayerNames[friendlyIndex].text = (string)player.CustomProperties["Name"];
                friendyKDAs[friendlyIndex].text = string.Format((string)player.CustomProperties["KillCount"] + " / " 
                    + (string)player.CustomProperties["DeadCount"] + " / " + (string)player.CustomProperties["AsisstCount"]);
                friendyTotalDamages[friendlyIndex].text = (string)player.CustomProperties["TotalDamage"];
                friendlyIndex++;
            }

            else
            {
                enemyPlayerNames[enemyIndex].text = (string)player.CustomProperties["Name"];
                enemyKDAs[enemyIndex].text = string.Format((string)player.CustomProperties["KillCount"] + " / "
                    + (string)player.CustomProperties["DeadCount"] + " / " + (string)player.CustomProperties["AsisstCount"]);
                enemyTotalDamages[enemyIndex].text = (string)player.CustomProperties["TotalDamage"];
                enemyIndex++;
            }
        }

        if ((string)PhotonNetwork.LocalPlayer.CustomProperties["Team"] == "A")
        {
            friendyScore.text = GameCenterTest.roundA.ToString();
            enemyScore.text = GameCenterTest.roundB.ToString();

            if (GameCenterTest.roundA > GameCenterTest.roundB) isWin = true;
            else isWin = false;
        }

        else
        {
            friendyScore.text = GameCenterTest.roundB.ToString();
            enemyScore.text = GameCenterTest.roundA.ToString();

            if (GameCenterTest.roundB > GameCenterTest.roundA) isWin = true;
            else isWin = false;
        }

    }

    void ShowUI()
    {
        if(isWin)
        {
            UIObjects["victory"].SetActive(true);
        }

        else
        {
            UIObjects["defeat"].SetActive(true);
        }

        for(int i = 0; i < friendlyIndex; i++)
        {
            UIObjects["player_" + i + "_Friendly"].SetActive(true);
        }

        for(int i = 0; i < enemyIndex; i++)
        {
            UIObjects["player_" + i + "_Enemy"].SetActive(true);
        }

    }

    public void GoToMain()
    {
        string destRoom = (string)PhotonNetwork.CurrentRoom.Players[PhotonNetwork.CurrentRoom.MasterClientId].CustomProperties["RoomInfo"];

        if(destRoom == "UserGame")
        {
            GameCenterTest.roundA = 0;
            GameCenterTest.roundB = 0;

            LoadSceneManager.GoBackToMenu("SiHyun RoomLobby Test");
        }
    }
}
