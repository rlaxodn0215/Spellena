using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerStats : MonoBehaviourPunCallbacks
{
    public GameObject statAssemble;
    public GameObject blueTeam;
    public GameObject redTeam;

    Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();

    List<Text> bluePlayerNames = new List<Text>();
    List<Text> blueCharacters = new List<Text>();
    List<Text> blueUltimateCounts = new List<Text>();
    List<Text> blueKDAs = new List<Text>();
    List<Text> bluePings = new List<Text>();

    List<Text> redPlayerNames = new List<Text>();
    List<Text> redCharacters = new List<Text>();
    List<Text> redUltimateCounts = new List<Text>();
    List<Text> redKDAs = new List<Text>();
    List<Text> redPings = new List<Text>();

    int blueIndex = 0;
    int redIndex = 0;

    Dictionary<int, int> pings = new Dictionary<int, int>();

    void Start()
    {
        Init();
    }

    void Init()
    {
        for(int i = 0; i < 5; i++)
        {
            UIObjects["player_" + i + "_Blue_Name"] = GameCenterTest.FindObject(blueTeam, "Player_" + i + "_Blue_Name");
            UIObjects["player_" + i + "_Blue_Character"] = GameCenterTest.FindObject(blueTeam, "Player_" + i + "_Blue_Character");
            UIObjects["player_" + i + "_Blue_UltimateCount"] = GameCenterTest.FindObject(blueTeam, "Player_" + i + "_Blue_UltimateCount");
            UIObjects["player_" + i + "_Blue_KDA"] = GameCenterTest.FindObject(blueTeam, "Player_" + i + "_Blue_KDA");
            UIObjects["player_" + i + "_Blue_Ping"] = GameCenterTest.FindObject(blueTeam, "Player_" + i + "_Blue_Ping");

            UIObjects["player_" + i + "_Red_Name"] = GameCenterTest.FindObject(redTeam, "Player_" + i + "_Red_Name");
            UIObjects["player_" + i + "_Red_Character"] = GameCenterTest.FindObject(redTeam, "Player_" + i + "_Red_Character");
            UIObjects["player_" + i + "_Red_UltimateCount"] = GameCenterTest.FindObject(redTeam, "Player_" + i + "_Red_UltimateCount");
            UIObjects["player_" + i + "_Red_KDA"] = GameCenterTest.FindObject(redTeam, "Player_" + i + "_Red_KDA");
            UIObjects["player_" + i + "_Red_Ping"] = GameCenterTest.FindObject(redTeam, "Player_" + i + "_Red_Ping");
        }

        foreach(var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            pings[player.ActorNumber] = 0;

            if ((string)player.CustomProperties["Team"] == "A")
            {
                redPlayerNames.Add(UIObjects["player_" + redIndex + "_Red_Name"].GetComponent<Text>());
                redCharacters.Add(UIObjects["player_" + redIndex + "_Red_Character"].GetComponent<Text>());
                redUltimateCounts.Add(UIObjects["player_" + redIndex + "_Red_UltimateCount"].GetComponent<Text>());
                redKDAs.Add(UIObjects["player_" + redIndex + "_Red_KDA"].GetComponent<Text>());
                redPings.Add(UIObjects["player_" + redIndex + "_Red_Ping"].GetComponent<Text>());
                redIndex++;
            }

            else
            {
                bluePlayerNames.Add(UIObjects["player_" + blueIndex + "_Blue_Name"].GetComponent<Text>());
                blueCharacters.Add(UIObjects["player_" + blueIndex + "_Blue_Character"].GetComponent<Text>());
                blueUltimateCounts.Add(UIObjects["player_" + blueIndex + "_Blue_UltimateCount"].GetComponent<Text>());
                blueKDAs.Add(UIObjects["player_" + blueIndex + "_Blue_KDA"].GetComponent<Text>());
                bluePings.Add(UIObjects["player_" + blueIndex + "_Blue_Ping"].GetComponent<Text>());
                blueIndex++;
            }
        }

    }

    void Update()
    {
        pings[PhotonNetwork.LocalPlayer.ActorNumber] = PhotonNetwork.GetPing();
        photonView.RPC("SerializePings", RpcTarget.AllBufferedViaServer,
            PhotonNetwork.LocalPlayer.ActorNumber, pings[PhotonNetwork.LocalPlayer.ActorNumber]);

        if (Input.GetKey(KeyCode.Tab))
        {
            statAssemble.SetActive(true);
            ShowStats();
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            statAssemble.SetActive(false);
        }
    }

    void ShowStats()
    {
        int tempBlueIndex = 0;
        int tempRedIndex = 0;

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if ((string)player.CustomProperties["Team"] == "A")
            {
                redPlayerNames[tempRedIndex].text = (string)player.CustomProperties["Name"];
                redCharacters[tempRedIndex].text = (string)player.CustomProperties["Character"];
                redUltimateCounts[tempRedIndex].text = (int)player.CustomProperties["UltimateCount"] + " / 10";
                redKDAs[tempRedIndex].text = (int)player.CustomProperties["KillCount"] + " / "
                    + (int)player.CustomProperties["AsisstCount"] + " / " + (int)player.CustomProperties["DeadCount"]; 
                 redPings[tempRedIndex].text = pings[player.ActorNumber].ToString();
                tempRedIndex++;
            }

            else
            {
                bluePlayerNames[tempBlueIndex].text = (string)player.CustomProperties["Name"];
                blueCharacters[tempBlueIndex].text = (string)player.CustomProperties["Character"];
                blueUltimateCounts[tempBlueIndex].text = (int)player.CustomProperties["UltimateCount"] + " / 10";
                blueKDAs[tempBlueIndex].text = (int)player.CustomProperties["KillCount"] + " / "
                    + (int)player.CustomProperties["AsisstCount"] + " / " + (int)player.CustomProperties["DeadCount"]; 
                 bluePings[tempBlueIndex].text = pings[player.ActorNumber].ToString();
                tempBlueIndex++;
            }
        }
    }

    [PunRPC]
    public void SerializePings(int actorNumber, int ping)
    {
        if(pings.ContainsKey(actorNumber))
        {
            pings[actorNumber] = ping;
        }
    }
}
