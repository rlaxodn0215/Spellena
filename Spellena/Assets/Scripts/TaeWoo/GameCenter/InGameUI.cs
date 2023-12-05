using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class InGameUI : MonoBehaviourPunCallbacks,IPunObservable
{
    Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();

    Image redPayloadImage;
    Image bluePayloadImage;
    Text redPercentageText;
    Text bluePercentageText;
    Text extraTimerText;
    Image redFillCircleImage;
    Image blueFillCircleImage;
    Image redCTFImage;
    Image blueCTFImage;
    Text killText;
    Text roundWinText;
    Text roundLooseText;

    List<Text> murderNames = new List<Text>();
    List<Text> victimNames = new List<Text>();
    List<Text> playerNames = new List<Text>();

    KillLogData[] playerKillLogDatas;

    public struct KillLogData
    {
        public bool isRed;
        public bool isMe;
        public float killLogTimer;
    }

    public struct OccupyingTeam
    {
        public string name;
        public float rate;
    }

    public struct Occupation
    {
        public float rate;
    }

    // 일시적인 게임 상태 string 테이터
    [HideInInspector]
    public string gameStateString;
    // 전체 타이머
    [HideInInspector]
    public float globalTimerUI;
    // 추가시간 타이머
    [HideInInspector]
    public float roundEndTimerUI;
    // 같은 팀원 수
    [HideInInspector]
    public int teamCount = 1;
    //추가 시간
    [HideInInspector]
    public float roundEndTimeUI = 5f;
    // A팀의 점령도
    [HideInInspector]
    public Occupation occupyingAUI;
    // B팀의 점령도
    [HideInInspector]
    public Occupation occupyingBUI;
    // 점령 게이지 바
    [HideInInspector]
    public OccupyingTeam occupyingTeamUI;
    // 데미지 CrossHair 활성 시간
    [HideInInspector]
    private float damageActiveTime = 0.75f;
    // 킬 CrossHair 활성 시간
    [HideInInspector]
    private float killActiveTime = 1f;
    // 킬 로그 활성 시간
    [HideInInspector]
    private float killLogActiveTime = 3f;
    // 끝 킬로그 인덱스
    [HideInInspector]
    public int endKillLogIndex;
    // 최대 킬로그 인덱스
    [HideInInspector]
    public int maxKillLogIndex;

    public Photon.Realtime.Player[] allPlayers;
    public List<GameObject> playersA = new List<GameObject>(); // Red
    public List<GameObject> playersB = new List<GameObject>(); // Blue
    
    void Start()
    {
        ConnectUI();
    }

    void ConnectUI()
    {
        UIObjects["unContested"] = GameCenterTest.FindObject(gameObject, "UnContested");
        UIObjects["captured_Red"] = GameCenterTest.FindObject(gameObject, "RedCapture");
        UIObjects["captured_Blue"] = GameCenterTest.FindObject(gameObject, "BlueCapture");
        UIObjects["redFillCircle"] = GameCenterTest.FindObject(gameObject, "RedOutline");
        UIObjects["blueFillCircle"] = GameCenterTest.FindObject(gameObject, "BlueOutline");
        UIObjects["fighting"] = GameCenterTest.FindObject(gameObject, "Fighting");
        UIObjects["redPayload"] = GameCenterTest.FindObject(gameObject, "RedPayload_Filled");
        UIObjects["bluePayload"] = GameCenterTest.FindObject(gameObject, "BluePayload_Filled");
        UIObjects["redExtraUI"] = GameCenterTest.FindObject(gameObject, "RedCTF");
        UIObjects["blueExtraUI"] = GameCenterTest.FindObject(gameObject, "BlueCTF");
        UIObjects["redPercentage"] = GameCenterTest.FindObject(gameObject, "RedOccupyingPercent");
        UIObjects["bluePercentage"] = GameCenterTest.FindObject(gameObject, "BlueOccupyingPercent");
        UIObjects["extraObj"] = GameCenterTest.FindObject(gameObject, "Extra");
        UIObjects["extraTimer"] = GameCenterTest.FindObject(gameObject, "ExtaTimer");
        UIObjects["redExtraObj"] = GameCenterTest.FindObject(gameObject, "Red");
        UIObjects["blueExtraObj"] = GameCenterTest.FindObject(gameObject, "Blue");
        UIObjects["redCTF"] = GameCenterTest.FindObject(gameObject, "RedCTF_Filled");
        UIObjects["blueCTF"] = GameCenterTest.FindObject(gameObject, "BlueCTF_Filled");

        UIObjects["redFirstPoint"] = GameCenterTest.FindObject(gameObject, "RedFirstPoint");
        UIObjects["redSecondPoint"] = GameCenterTest.FindObject(gameObject, "RedSecondPoint");
        UIObjects["blueFirstPoint"] = GameCenterTest.FindObject(gameObject, "BlueFirstPoint");
        UIObjects["blueSecondPoint"] = GameCenterTest.FindObject(gameObject, "BlueSecondPoint");

        UIObjects["damage"] = GameCenterTest.FindObject(gameObject, "Damage");
        UIObjects["killText"] = GameCenterTest.FindObject(gameObject, "KillText");

        ConnectKillLogs();
        ConnectMyTeamStatus();
        ConnectOutcome();

        redPayloadImage = UIObjects["redPayload"].GetComponent<Image>();
        bluePayloadImage = UIObjects["bluePayload"].GetComponent<Image>();
        redPercentageText = UIObjects["redPercentage"].GetComponent<Text>();
        bluePercentageText = UIObjects["bluePercentage"].GetComponent<Text>();
        extraTimerText = UIObjects["extraTimer"].GetComponent<Text>();
        redFillCircleImage = UIObjects["redFillCircle"].GetComponent<Image>();
        blueFillCircleImage = UIObjects["blueFillCircle"].GetComponent<Image>();
        redCTFImage = UIObjects["redCTF"].GetComponent<Image>();
        blueCTFImage = UIObjects["blueCTF"].GetComponent<Image>();
        killText = UIObjects["killText"].GetComponent<Text>();
    }

    void ConnectKillLogs()
    {
        int n = GameCenterTest.FindObject(gameObject, "PlayerKillLogs").transform.childCount;

        endKillLogIndex = 0;
        maxKillLogIndex = n;
        playerKillLogDatas = new KillLogData[n];

        for (int i = 1; i <= n; i++)
        {
            UIObjects["killLog_" + i] = GameCenterTest.FindObject(gameObject, "KillLog_" + i);
            UIObjects["killLog_" + i + "_BackImage_Red"] = GameCenterTest.FindObject(gameObject, "KillLog_" + i + "_BackImage_Red");
            UIObjects["killLog_" + i + "_BackImage_Blue"] = GameCenterTest.FindObject(gameObject, "KillLog_" + i + "_BackImage_Blue");
            UIObjects["isMe_" + i] = GameCenterTest.FindObject(gameObject, "IsMe_" + i);
            UIObjects["murder_" + i] = GameCenterTest.FindObject(gameObject, "Murder_" + i);
            murderNames.Add(UIObjects["murder_" + i].GetComponent<Text>());
            UIObjects["victim_" + i] = GameCenterTest.FindObject(gameObject, "Victim_" + i);
            victimNames.Add(UIObjects["victim_" + i].GetComponent<Text>());
        }
    }

    void ConnectMyTeamStatus()
    {
        int n = GameCenterTest.FindObject(gameObject, "MyTeamPlayerStatus").transform.childCount;

        for(int i = 1; i <= n; i++)
        {
            UIObjects["player_" + i] = GameCenterTest.FindObject(gameObject, "Player_" + i);
            UIObjects["playerName_" + i] = GameCenterTest.FindObject(gameObject, "PlayerName_" + i);
            UIObjects["player_" + i + "_Image"] = GameCenterTest.FindObject(gameObject, "Player_" + i + "_Image");
            playerNames.Add(UIObjects["playerName_" + i].GetComponent<Text>());
            UIObjects["playerDead_" + i] = GameCenterTest.FindObject(gameObject, "PlayerDead_" + i);
        }

    }

    void ConnectOutcome()
    {
        UIObjects["roundWin"] = GameCenterTest.FindObject(gameObject, "RoundWin");
        roundWinText = UIObjects["roundWin"].GetComponentInChildren<Text>();
        UIObjects["roundLoose"] = GameCenterTest.FindObject(gameObject, "RoundLoose");
        roundLooseText = UIObjects["roundLoose"].GetComponentInChildren<Text>();
        UIObjects["victory"] = GameCenterTest.FindObject(gameObject, "Victory");
        UIObjects["defeat"] = GameCenterTest.FindObject(gameObject, "Defeat");
    }

    void Update()
    {
        redPayloadImage.fillAmount = occupyingAUI.rate * 0.01f;
        bluePayloadImage.fillAmount = occupyingBUI.rate * 0.01f;
        redPercentageText.text = string.Format((int)occupyingAUI.rate + "%");
        bluePercentageText.text = string.Format((int)occupyingBUI.rate + "%");
        extraTimerText.text = string.Format("{0:F2}", roundEndTimerUI);

        DisableKillLog();

        if (occupyingTeamUI.name == "A")
            redFillCircleImage.fillAmount = occupyingTeamUI.rate * 0.01f;
        else if (occupyingTeamUI.name == "B")
            blueFillCircleImage.fillAmount = occupyingTeamUI.rate * 0.01f;
        else
        {
            redFillCircleImage.fillAmount = 0;
            blueFillCircleImage.fillAmount = 0;
        }

        redCTFImage.fillAmount = roundEndTimerUI / roundEndTimeUI;
        blueCTFImage.fillAmount = roundEndTimerUI / roundEndTimeUI;
        
    }

    [PunRPC]
    public void ActiveInGameUIObj(string uiName, bool isActive)
    {
        if (!UIObjects.ContainsKey(uiName)) return;
        UIObjects[uiName].SetActive(isActive);
    }

    [PunRPC]
    public void ShowRoundWin(int round)
    {
        ActiveInGameUIObj("roundWin", true);
        roundWinText.text = string.Format(round + " 라운드 승리!");
    }

    [PunRPC]
    public void ShowRoundLoose(int round)
    {
        ActiveInGameUIObj("roundLoose", true);
        roundLooseText.text = string.Format(round + " 라운드 패배!");
    }

    [PunRPC]
    public void ShowDamageUI()
    {
        if(UIObjects["damage"])
        {
            UIObjects["damage"].SetActive(true);
            StartCoroutine(DisableUI("damage", damageActiveTime));
        }
    }

    [PunRPC]
    public void ShowKillUI(string victim)
    {
        if (UIObjects["killText"])
        {
            UIObjects["killText"].SetActive(true);
            killText.text = string.Format("<color=red>" + victim + "</color>" + " 처치");
            StartCoroutine(DisableUI("killText", killActiveTime));
        }
   
    }

    [PunRPC]
    public void ShowTeamState(string playerName, string characterName)
    {
        UIObjects["player_" + teamCount].SetActive(true);
        playerNames[teamCount - 1].text = playerName;

        // 캐릭터 이미지 대입

        teamCount++;
    }

    [PunRPC]
    public void ShowTeamLifeDead(string playerName, bool isDead)
    {
        for(int i = 0; i < playerNames.Count; i++)
        {
            if(playerNames[i].text == playerName)
            {
                UIObjects["playerDead_" + (i + 1)].SetActive(isDead);
                break;
            }
        }
    }

    [PunRPC]
    public void ShowKillLog(string _killer, string _victim, bool _isRed, int _isMeActorNum)
    {
        MoveKillLog();

        Debug.Log("ShowKillLog : " + PhotonNetwork.IsMasterClient);

        murderNames[0].text = _killer;
        victimNames[0].text = _victim;
        playerKillLogDatas[0].isRed = _isRed;

        if(PhotonNetwork.LocalPlayer.ActorNumber == _isMeActorNum)
        {
            playerKillLogDatas[0].isMe = true;
        }

        else
        {
            playerKillLogDatas[0].isMe = false;
        }

        playerKillLogDatas[0].killLogTimer = globalTimerUI + killLogActiveTime;

        UIObjects["killLog_" + 1 + "_BackImage_Red"].SetActive(_isRed);
        UIObjects["killLog_" + 1 + "_BackImage_Blue"].SetActive(!_isRed);
        UIObjects["isMe_" + 1].SetActive(playerKillLogDatas[0].isMe);

    }

    void MoveKillLog()
    {
        if (endKillLogIndex == 0)
        {
            endKillLogIndex++;
            UIObjects["killLog_" + endKillLogIndex].SetActive(true);
        }

        else if (endKillLogIndex < maxKillLogIndex)
        {
            for (int i = endKillLogIndex - 1; i >= 0; i--)
            {
                murderNames[i + 1].text = murderNames[i].text;
                victimNames[i + 1].text = victimNames[i].text;
                playerKillLogDatas[i + 1] = playerKillLogDatas[i];

                UIObjects["killLog_" + (i + 2) + "_BackImage_Red"].SetActive(playerKillLogDatas[i+1].isRed);
                UIObjects["killLog_" + (i + 2) + "_BackImage_Blue"].SetActive(!playerKillLogDatas[i+1].isRed);
                UIObjects["isMe_" + (i + 2)].SetActive(playerKillLogDatas[i + 1].isMe);

            }

            endKillLogIndex++;

            UIObjects["killLog_" + endKillLogIndex].SetActive(true);

        }

        else
        {
            for (int i = maxKillLogIndex - 2; i >= 0; i--)
            {
                murderNames[i + 1].text = murderNames[i].text;
                victimNames[i + 1].text = victimNames[i].text;
                playerKillLogDatas[i + 1] = playerKillLogDatas[i];

                UIObjects["killLog_" + (i + 2) + "_BackImage_Red"].SetActive(playerKillLogDatas[i + 1].isRed);
                UIObjects["killLog_" + (i + 2) + "_BackImage_Blue"].SetActive(!playerKillLogDatas[i + 1].isRed);
                UIObjects["isMe_" + (i + 2)].SetActive(playerKillLogDatas[i + 1].isMe);
            }
        }


    }

    void DisableKillLog()
    {
        if (endKillLogIndex < 1) return;

        if(globalTimerUI >= playerKillLogDatas[endKillLogIndex-1].killLogTimer)
        {
            UIObjects["killLog_" + endKillLogIndex].SetActive(false);
            endKillLogIndex--;
        }
    }

    IEnumerator DisableUI(string name, float time)
    {
        yield return new WaitForSeconds(time);
        UIObjects[name].SetActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameStateString);
            stream.SendNext(globalTimerUI);
            stream.SendNext(roundEndTimerUI);
            stream.SendNext(occupyingAUI.rate);
            stream.SendNext(occupyingBUI.rate);
            stream.SendNext(occupyingTeamUI.name);
            stream.SendNext(occupyingTeamUI.rate);
            stream.SendNext(endKillLogIndex);
            stream.SendNext(maxKillLogIndex);
        }
        else
        {
            gameStateString = (string)stream.ReceiveNext();
            globalTimerUI = (float)stream.ReceiveNext();
            roundEndTimerUI = (float)stream.ReceiveNext();
            occupyingAUI.rate = (float)stream.ReceiveNext();
            occupyingBUI.rate = (float)stream.ReceiveNext();
            occupyingTeamUI.name = (string)stream.ReceiveNext();
            occupyingTeamUI.rate = (float)stream.ReceiveNext();

            endKillLogIndex = (int)stream.ReceiveNext();
            maxKillLogIndex = (int)stream.ReceiveNext();
        }
    }
}
