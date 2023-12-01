using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class DeathCamUI : MonoBehaviourPunCallbacks
{
    public GameObject showKillerData;
    public GameObject otherPlayerCamUI;

    // 1:에테르나 / 2:엘리멘탈 오더
    public List<Sprite> characterIcons = new List<Sprite>();

    Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();

    Image killerIcon;
    Text killerCharacter;
    Text killerName;
    Text killerStat;

    Text killerCharacter1;
    Text killerName1;
    Text currentTime;
    Text rebornTimer;
    public Text showPlayerCamID;

    void Start()
    {
        Init();
    }

    void Init()
    {
        UIObjects["killerIcon"] = GameCenterTest.FindObject(showKillerData, "KillerIcon");
        UIObjects["killerCharacter"] = GameCenterTest.FindObject(showKillerData, "KillerCharacter");
        UIObjects["killerName"] = GameCenterTest.FindObject(showKillerData, "KillerName");
        UIObjects["killerStat"] = GameCenterTest.FindObject(showKillerData, "KillerStat");

        UIObjects["killerCharacter1"] = GameCenterTest.FindObject(otherPlayerCamUI, "KillerCharacter1");
        UIObjects["killerName1"] = GameCenterTest.FindObject(otherPlayerCamUI, "KillerName1");
        UIObjects["currentTime"] = GameCenterTest.FindObject(otherPlayerCamUI, "CurrentTime");
        UIObjects["rebornTimer"] = GameCenterTest.FindObject(otherPlayerCamUI, "RebornTimer");
        UIObjects["showPlayerCamID"] = GameCenterTest.FindObject(otherPlayerCamUI, "ShowPlayerCamID");

        killerIcon = UIObjects["killerIcon"].GetComponent<Image>();
        killerCharacter = UIObjects["killerCharacter"].GetComponent<Text>();
        killerName = UIObjects["killerName"].GetComponent<Text>();
        killerStat = UIObjects["killerStat"].GetComponent<Text>();

        killerCharacter1 = UIObjects["killerCharacter1"].GetComponent<Text>();
        killerName1 = UIObjects["killerName1"].GetComponent<Text>();
        currentTime = UIObjects["currentTime"].GetComponent<Text>();
        rebornTimer = UIObjects["rebornTimer"].GetComponent<Text>();
        showPlayerCamID = UIObjects["showPlayerCamID"].GetComponent<Text>();
    }

    void Update()
    {
        UpdateTimer();
    }

    [PunRPC]
    public void ShowKillerData(string killerID)
    {
        if (killerID == "World")
        {
            ShowCharacterIcon(killerID);

            killerCharacter.text = "처치자 : World" ;
            killerCharacter1.text = "내 영웅 처치 : World";
            killerName.text = killerName1.text = killerID;
            killerStat.text = "";
        }

        else
        {
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if ((string)player.CustomProperties["Name"] == killerID)
                {
                    ShowCharacterIcon((string)player.CustomProperties["Character"]);

                    killerCharacter.text = "처치자 : " + (string)player.CustomProperties["Character"];
                    killerCharacter1.text = "내 영웅 처치 : " + (string)player.CustomProperties["Character"];
                    killerName.text = killerName1.text = killerID;
                    killerStat.text = (int)player.CustomProperties["KillCount"] + " / " + (int)player.CustomProperties["DeadCount"]
                        + " / " + (int)player.CustomProperties["AsisstCount"];
                    break;
                }
            }
        }
        showKillerData.SetActive(true);
    }

    [PunRPC]
    public void DisableDeathCamUI()
    {
        if (showKillerData.activeSelf) showKillerData.SetActive(false);
        if (otherPlayerCamUI.activeSelf) otherPlayerCamUI.SetActive(false);
    }

    void ShowCharacterIcon(string name)
    {
        switch (name)
        {
            case "World":
                killerIcon.sprite = characterIcons[0];
                break;
            case "Aeterna":
                killerIcon.sprite = characterIcons[1];
                break;
            case "ElementalOrder":
                killerIcon.sprite = characterIcons[2];
                break;
            case "Dracoson":
                killerIcon.sprite = characterIcons[3];
                break;
            case "Cultist":
                killerIcon.sprite = characterIcons[4];
                break;
            default:
                break;
        }

    }

    public void SwitchCam()
    {
        if (showKillerData.activeSelf) showKillerData.SetActive(false);
        if (!otherPlayerCamUI.activeSelf) otherPlayerCamUI.SetActive(true);
    }

    void UpdateTimer()
    {
        rebornTimer.text = string.Format("{0:F0}",((float)PhotonNetwork.LocalPlayer.CustomProperties["ReSpawnTime"] - GameCenterTest.globalTimer));
        currentTime.text = "경기 시간 " + (int)GameCenterTest.globalTimer / 60 + " : " + (int)GameCenterTest.globalTimer % 60;
    }

}
