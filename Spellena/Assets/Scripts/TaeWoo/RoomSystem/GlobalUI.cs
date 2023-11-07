using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GlobalUI : MonoBehaviourPunCallbacks,IPunObservable
{
    public GameObject inGameUI;
    public GameObject etcUI;

    Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();

    Text timerText;
    Text gameStateUIText;
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

    int killLogIndex = 1;

    List<Text> murderNames = new List<Text>();
    List<Text> victimNames = new List<Text>();
    List<Text> playerNames = new List<Text>();

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
    public string gameStateString;
    // 전체 타이머
    public float globalTimerUI;
    // 추가시간 타이머
    public float roundEndTimerUI;
    //추가 시간
    public float roundEndTimeUI = 5f;
    // A팀의 점령도
    public Occupation occupyingAUI;
    // B팀의 점령도
    public Occupation occupyingBUI;
    // 점령 게이지 바
    public OccupyingTeam occupyingTeamUI;
    // 킬 CrossHair 활성 시간
    public float killActiveTime = 1f;
    // 킬 로그 활성 시간
    public float killLogActiveTime = 2f;

    public Photon.Realtime.Player[] allPlayers;
    public List<GameObject> playersA = new List<GameObject>(); // Red
    public List<GameObject> playersB = new List<GameObject>(); // Blue
    
    void Start()
    {
        ConnectUI();
        InitUI();
    }

    void ConnectUI()
    {
        UIObjects["inGameUI"] = inGameUI;
        UIObjects["etcUI"] = etcUI;

        UIObjects["unContested"] = FindObject(inGameUI, "UnContested");
        UIObjects["captured_Red"] = FindObject(inGameUI, "RedCapture");
        UIObjects["captured_Blue"] = FindObject(inGameUI, "BlueCapture");
        UIObjects["redFillCircle"] = FindObject(inGameUI, "RedOutline");
        UIObjects["blueFillCircle"] = FindObject(inGameUI, "BlueOutline");
        UIObjects["fighting"] = FindObject(inGameUI, "Fighting");
        UIObjects["redPayload"] = FindObject(inGameUI, "RedPayload_Filled");
        UIObjects["bluePayload"] = FindObject(inGameUI, "BluePayload_Filled");
        UIObjects["redExtraUI"] = FindObject(inGameUI, "RedCTF");
        UIObjects["blueExtraUI"] = FindObject(inGameUI, "BlueCTF");
        UIObjects["redPercentage"] = FindObject(inGameUI, "RedOccupyingPercent");
        UIObjects["bluePercentage"] = FindObject(inGameUI, "BlueOccupyingPercent");
        UIObjects["extraObj"] = FindObject(inGameUI, "Extra");
        UIObjects["extraTimer"] = FindObject(inGameUI, "ExtaTimer");
        UIObjects["redExtraObj"] = FindObject(inGameUI, "Red");
        UIObjects["blueExtraObj"] = FindObject(inGameUI, "Blue");
        UIObjects["redCTF"] = FindObject(inGameUI, "RedCTF_Filled");
        UIObjects["blueCTF"] = FindObject(inGameUI, "BlueCTF_Filled");

        UIObjects["redFirstPoint"] = FindObject(inGameUI, "RedFirstPoint");
        UIObjects["redSecondPoint"] = FindObject(inGameUI, "RedSecondPoint");
        UIObjects["blueFirstPoint"] = FindObject(inGameUI, "BlueFirstPoint");
        UIObjects["blueSecondPoint"] = FindObject(inGameUI, "BlueSecondPoint");

        UIObjects["damage"] = FindObject(inGameUI, "Damage");
        UIObjects["killText"] = FindObject(inGameUI, "KillText");

        ConnectKillLogs();
        ConnectMyTeamStatus();
        ConnectOutcome();

        UIObjects["gameStateUI"] = FindObject(etcUI, "GameState");
        UIObjects["timer"] = FindObject(etcUI, "Timer");

        timerText = UIObjects["timer"].GetComponent<Text>();
        gameStateUIText = UIObjects["gameStateUI"].GetComponent<Text>();
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
        int n = FindObject(inGameUI, "PlayerKillLogs").transform.childCount;

        for(int i = 1; i <= n; i++)
        {
            UIObjects["killLog_" + i] = FindObject(inGameUI, "KillLog_" + i);
            UIObjects["killLog_" + i + "_BackImage_Red"] = FindObject(inGameUI, "KillLog_" + i + "_BackImage_Red");
            UIObjects["killLog_" + i + "BackImage_Blue"] = FindObject(inGameUI, "KillLog_" + i + "_BackImage_Blue");
            UIObjects["murder_" + i] = FindObject(inGameUI, "Murder_" + i);
            murderNames.Add(UIObjects["murder_" + i].GetComponent<Text>());
            UIObjects["victim_" + i] = FindObject(inGameUI, "Victim_" + i);
            victimNames.Add(UIObjects["victim_" + i].GetComponent<Text>());
        }
    }

    void ConnectMyTeamStatus()
    {
        int n = FindObject(inGameUI, "MyTeamPlayerStatus").transform.childCount;

        for(int i = 1; i <=n; i++)
        {
            UIObjects["player_" + i] = FindObject(inGameUI, "Player_" + i);
            UIObjects["playerName_" + i] = FindObject(inGameUI, "PlayerName_" + i);
            playerNames.Add(UIObjects["playerName_" + i].GetComponent<Text>());
            UIObjects["playerDead_" + i] = FindObject(inGameUI, "PlayerDead_" + i);
        }

    }

    void ConnectOutcome()
    {
        UIObjects["roundWin"] = FindObject(inGameUI, "RoundWin");
        UIObjects["roundLoose"] = FindObject(inGameUI, "RoundLoose");
        UIObjects["victory"] = FindObject(inGameUI, "Victory");
        UIObjects["defeat"] = FindObject(inGameUI, "Defeat");
    }

    void InitUI()
    {
        UIObjects["inGameUI"].SetActive(false);
        UIObjects["etcUI"].SetActive(true);
    }

    void Update()
    {
        timerText.text = ((int)globalTimerUI + 1).ToString();
        gameStateUIText.text = gameStateString;
        redPayloadImage.fillAmount = occupyingAUI.rate * 0.01f;
        bluePayloadImage.fillAmount = occupyingBUI.rate * 0.01f;
        redPercentageText.text = string.Format((int)occupyingAUI.rate + "%");
        bluePercentageText.text = string.Format((int)occupyingBUI.rate + "%");
        extraTimerText.text = string.Format("{0:F2}", roundEndTimerUI);

        if(occupyingTeamUI.name == "A")
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
    public void ActiveUI(string uiName, bool isActive)
    {
        if (UIObjects[uiName] == null) return;
        UIObjects[uiName].SetActive(isActive);
    }

    [PunRPC]
    public void ShowKillUI(string victim)
    {
        if (UIObjects["kill"] && UIObjects["killText"])
        {
            UIObjects["killText"].SetActive(true);
            killText.text = string.Format("<color=red>" + victim + "</color>" + " 처치");
            StartCoroutine(DisableUI("killText", killActiveTime));
        }
   
    }

    IEnumerator DisableUI(string name, float time)
    {
        yield return new WaitForSeconds(time);
        UIObjects[name].SetActive(false);
    }

    public static GameObject FindObject(GameObject parrent, string name)
    {
        GameObject foundObject = null;
        Transform[] array = parrent.GetComponentsInChildren<Transform>(true);

        foreach (Transform transform in array)
        {
            if (transform.name == name)
            {
                foundObject = transform.gameObject;
                break; // 찾았으면 루프를 종료.
            }
        }

        if (foundObject == null)
        {
            Debug.LogError("해당 이름의 게임 오브젝트를 찾지 못했습니다 : " + name);
        }

        return foundObject;
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
        }
    }
}
