using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using GameCenterDataType;
using GameCenterTest0;

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
    Image redAngelTimerImage;
    Image blueAngelTimerImage;

    Image damageImage_left;
    Image damageImage_right;

    List<Text> murderNames = new List<Text>();
    List<Text> victimNames = new List<Text>();
    List<Text> playerNames = new List<Text>();

    KillLogData[] playerKillLogDatas;

    public DuringRoundData duringRoundData;
    public DuringRoundStandardData duringRoundStandardData;
    public float globalTimer;

    // 데미지 CrossHair 활성 시간
    [HideInInspector]
    private float damageActiveTime = 0.75f;
    // 킬 CrossHair 활성 시간
    [HideInInspector]
    private float killActiveTime = 1f;

    // 킬 로그 활성 시간
    [HideInInspector]
    private float killLogActiveTime = 3f;
    [HideInInspector]
    public int teamCount = 1;
    // 끝 킬로그 인덱스
    [HideInInspector]
    public int endKillLogIndex;
    // 최대 킬로그 인덱스
    [HideInInspector]
    public int maxKillLogIndex;

    public Photon.Realtime.Player[] allPlayers;
    public List<GameObject> playersA = new List<GameObject>(); // Red
    public List<GameObject> playersB = new List<GameObject>(); // Blue
    public SoundManager soundManager;
    
    void Start()
    {
        ConnectUI();
    }

    void ConnectUI()
    {
        ConnectObjects();
        ConnectKillLogs();
        ConnectMyTeamStatus();
        ConnectOutcome();
        ConnectUIProperties();
    }

    void ConnectObjects()
    {
        //for(int i = 0; i < transform.childCount; i++)
        //{
        //    UIObjects[transform.GetChild(i).name] = Helper.FindObject(gameObject, transform.GetChild(i).name);
        //}

        UIObjects["crossHair"] = Helper.FindObject(gameObject, "CrossHair");
        UIObjects["unContested"] = Helper.FindObject(gameObject, "UnContested");
        UIObjects["captured_Red"] = Helper.FindObject(gameObject, "RedCapture");
        UIObjects["captured_Blue"] = Helper.FindObject(gameObject, "BlueCapture");
        UIObjects["redFillCircle"] = Helper.FindObject(gameObject, "RedOutline");
        UIObjects["blueFillCircle"] = Helper.FindObject(gameObject, "BlueOutline");
        UIObjects["fighting"] = Helper.FindObject(gameObject, "Fighting");
        UIObjects["redPayload"] = Helper.FindObject(gameObject, "RedPayload_Filled");
        UIObjects["bluePayload"] = Helper.FindObject(gameObject, "BluePayload_Filled");
        UIObjects["redExtraUI"] = Helper.FindObject(gameObject, "RedCTF");
        UIObjects["blueExtraUI"] = Helper.FindObject(gameObject, "BlueCTF");
        UIObjects["redPercentage"] = Helper.FindObject(gameObject, "RedOccupyingPercent");
        UIObjects["bluePercentage"] = Helper.FindObject(gameObject, "BlueOccupyingPercent");
        UIObjects["extraObj"] = Helper.FindObject(gameObject, "Extra");
        UIObjects["extraTimer"] = Helper.FindObject(gameObject, "ExtaTimer");
        UIObjects["redExtraObj"] = Helper.FindObject(gameObject, "Red");
        UIObjects["blueExtraObj"] = Helper.FindObject(gameObject, "Blue");
        UIObjects["redCTF"] = Helper.FindObject(gameObject, "RedCTF_Filled");
        UIObjects["blueCTF"] = Helper.FindObject(gameObject, "BlueCTF_Filled");

        UIObjects["redPoint_1"] = Helper.FindObject(gameObject, "RedPoint_1");
        UIObjects["redPoint_2"] = Helper.FindObject(gameObject, "RedPoint_2");

        UIObjects["bluePoint_1"] = Helper.FindObject(gameObject, "BluePoint_1");
        UIObjects["bluePoint_2"] = Helper.FindObject(gameObject, "BluePoint_2");

        UIObjects["damage"] = Helper.FindObject(gameObject, "Damage");
        UIObjects["damage_Left"] = Helper.FindObject(gameObject, "Damage_Left");
        UIObjects["damage_Right"] = Helper.FindObject(gameObject, "Damage_Right");
        UIObjects["killText"] = Helper.FindObject(gameObject, "KillText");

        UIObjects["angelTimer"] = Helper.FindObject(gameObject, "AngelTimer");
        UIObjects["redAngelTimer"] = Helper.FindObject(gameObject, "RedAngelTimer");
        UIObjects["blueAngelTimer"] = Helper.FindObject(gameObject, "BlueAngelTimer");
        UIObjects["useAngel"] = Helper.FindObject(gameObject, "UseAngel");
    }

    void ConnectKillLogs()
    {
        int n = Helper.FindObject(gameObject, "PlayerKillLogs").transform.childCount;

        endKillLogIndex = 0;
        maxKillLogIndex = n;
        playerKillLogDatas = new KillLogData[n];

        for (int i = 1; i <= n; i++)
        {
            UIObjects["killLog_" + i] = Helper.FindObject(gameObject, "KillLog_" + i);
            UIObjects["killLog_" + i + "_BackImage_Red"] = Helper.FindObject(gameObject, "KillLog_" + i + "_BackImage_Red");
            UIObjects["killLog_" + i + "_BackImage_Blue"] = Helper.FindObject(gameObject, "KillLog_" + i + "_BackImage_Blue");
            UIObjects["isMe_" + i] = Helper.FindObject(gameObject, "IsMe_" + i);
            UIObjects["murder_" + i] = Helper.FindObject(gameObject, "Murder_" + i);
            murderNames.Add(UIObjects["murder_" + i].GetComponent<Text>());
            UIObjects["victim_" + i] = Helper.FindObject(gameObject, "Victim_" + i);
            victimNames.Add(UIObjects["victim_" + i].GetComponent<Text>());
        }
    }

    void ConnectMyTeamStatus()
    {
        int n = Helper.FindObject(gameObject, "MyTeamPlayerStatus").transform.childCount;

        for(int i = 1; i <= n; i++)
        {
            UIObjects["player_" + i] = Helper.FindObject(gameObject, "Player_" + i);
            UIObjects["playerName_" + i] = Helper.FindObject(gameObject, "PlayerName_" + i);
            UIObjects["player_" + i + "_Image"] = Helper.FindObject(gameObject, "Player_" + i + "_Image");
            playerNames.Add(UIObjects["playerName_" + i].GetComponent<Text>());
            UIObjects["playerDead_" + i] = Helper.FindObject(gameObject, "PlayerDead_" + i);
        }

    }

    void ConnectOutcome()
    {
        UIObjects["roundWin"] = Helper.FindObject(gameObject, "RoundWin");
        roundWinText = UIObjects["roundWin"].GetComponentInChildren<Text>();
        UIObjects["roundLoose"] = Helper.FindObject(gameObject, "RoundLoose");
        roundLooseText = UIObjects["roundLoose"].GetComponentInChildren<Text>();
        UIObjects["victory"] = Helper.FindObject(gameObject, "Victory");
        UIObjects["defeat"] = Helper.FindObject(gameObject, "Defeat");
    }

    void ConnectUIProperties()
    {
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
        redAngelTimerImage = UIObjects["redAngelTimer"].GetComponent<Image>();
        blueAngelTimerImage = UIObjects["blueAngelTimer"].GetComponent<Image>();

        damageImage_left = UIObjects["damage_Left"].GetComponent<Image>();
        damageImage_right = UIObjects["damage_Right"].GetComponent<Image>();
    }

    void FixedUpdate()
    {
        redPayloadImage.fillAmount = duringRoundData.occupyingA.rate * 0.01f;
        bluePayloadImage.fillAmount = duringRoundData.occupyingB.rate * 0.01f;
        redPercentageText.text = string.Format((int)duringRoundData.occupyingA.rate + "%");
        bluePercentageText.text = string.Format((int)duringRoundData.occupyingB.rate + "%");
        extraTimerText.text = string.Format("{0:F2}", duringRoundData.roundEndTimer);

        DisableKillLog();

        if (duringRoundData.occupyingTeam.name == "A")
            redFillCircleImage.fillAmount = duringRoundData.occupyingTeam.rate * 0.01f;
        else if (duringRoundData.occupyingTeam.name == "B")
            blueFillCircleImage.fillAmount = duringRoundData.occupyingTeam.rate * 0.01f;
        else
        {
            redFillCircleImage.fillAmount = 0;
            blueFillCircleImage.fillAmount = 0;
        }

        redCTFImage.fillAmount = duringRoundData.roundEndTimer / duringRoundStandardData.roundEndTime;
        blueCTFImage.fillAmount = duringRoundData.roundEndTimer / duringRoundStandardData.roundEndTime;
        
    }

    [PunRPC]
    public void DisActiveCrosshair()
    {
        if(UIObjects["crossHair"] !=null)
        {
            UIObjects["crossHair"].SetActive(false);
        }
    }

    [PunRPC]
    public void ActiveInGameUIObj(string uiName, bool isActive)
    {
        if (!UIObjects.ContainsKey(uiName) && UIObjects[uiName].activeSelf == isActive) return;
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
    public void ShowRoundPoint(string team, int num)
    {
        if(team == "A")
        {
            ActiveInGameUIObj("redPoint_" + num, true);
        }

        else
        {
            ActiveInGameUIObj("bluePoint_" + num, true);
        }
    }

    [PunRPC]
    public void ShowDamageUI(string damagePart)
    {
        if(UIObjects["damage"])
        {
            UIObjects["damage"].SetActive(true);

            if(damagePart == "head")
            {
                damageImage_left.color = Color.red;
                damageImage_right.color = Color.red;

                soundManager.PlayAudio("AttackSound_Big", 1.0f, false, false, "EffectSound");
            }

            else
            {
                damageImage_left.color = Color.white;
                damageImage_right.color = Color.white;

                soundManager.PlayAudio("AttackSound_Small", 1.0f, false, false, "EffectSound");
            }

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
            //killText.text = string.Format("처치 테스트");
            // 킬 사운드
            soundManager.PlayAudioOverlap("KillSound", 1.0f, false, false, "EffectSound");
            StartCoroutine(DisableUI("killText", killActiveTime));
        }
   
    }

    [PunRPC]
    public void ShowAssistUI(string victim)
    {
        if (UIObjects["killText"])
        {
            UIObjects["killText"].SetActive(true);
            killText.text = string.Format("<color=red>" + victim + "</color>" + " 처치 기여");
            //killText.text = string.Format("어시스트 테스트");
            // 어시스트 사운드
            soundManager.PlayAudioOverlap("AssistSound", 1.0f, false, false, "EffectSound");
            StartCoroutine(DisableUI("killText", killActiveTime));
        }
    }

    [PunRPC]
    public void ShowTeamState(string playerName)
    {
        UIObjects["player_" + teamCount].SetActive(true);
        playerNames[teamCount - 1].text = playerName;
        teamCount++;
    }

    [PunRPC]
    public void InitTeamLifeDead()
    {
        for (int i = 0; i < playerNames.Count; i++)
        {
            UIObjects["playerDead_" + (i + 1)].SetActive(false);
        }
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


    public void ShowAngelTimerUI(float angelStatueCoolTime, string team ,bool isActive)
    {
        if(UIObjects.ContainsKey("angelTimer"))
        {
            UIObjects["angelTimer"].SetActive(isActive);

            if(team == "TeamA")
            {
                if (UIObjects.ContainsKey("redAngelTimer"))
                {
                    UIObjects["redAngelTimer"].SetActive(isActive);

                    if(isActive)
                    {
                        redAngelTimerImage.fillAmount = 1.0f + 
                            (globalTimer - (float)PhotonNetwork.LocalPlayer.CustomProperties["AngelStatueCoolTime"]) / angelStatueCoolTime;

                        if(redAngelTimerImage.fillAmount >=0.99f)
                        {
                            UIObjects["useAngel"].SetActive(true);
                        }

                        else
                        {
                            UIObjects["useAngel"].SetActive(false);
                        }
                    }
                }
            }

            else if(team == "TeamB")
            {
                if (UIObjects.ContainsKey("blueAngelTimer"))
                {
                    UIObjects["blueAngelTimer"].SetActive(isActive);

                    if (isActive)
                    {
                        blueAngelTimerImage.fillAmount = 1.0f + 
                            (globalTimer - (float)PhotonNetwork.LocalPlayer.CustomProperties["AngelStatueCoolTime"]) / angelStatueCoolTime;

                        if (blueAngelTimerImage.fillAmount >= 0.99f)
                        {
                            UIObjects["useAngel"].SetActive(true);
                        }

                        else
                        {
                            UIObjects["useAngel"].SetActive(false);
                        }
                    }
                }
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

        playerKillLogDatas[0].killLogTimer = globalTimer + killLogActiveTime;

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

        if(globalTimer >= playerKillLogDatas[endKillLogIndex-1].killLogTimer)
        {
            UIObjects["killLog_" + endKillLogIndex].SetActive(false);
            //Debug.Log("<color=yellow>" + "DisableKillLog index : " + endKillLogIndex + "</color>");
            endKillLogIndex--;
        }
    }

    [PunRPC]
    public void DisableAllKillLog()
    {
        for (int i = 0; i < endKillLogIndex; i++)
        {
            playerKillLogDatas[i].killLogTimer = 0.0f;
            UIObjects["killLog_" + endKillLogIndex].SetActive(false);
        }

        endKillLogIndex = 0;
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
            stream.SendNext(globalTimer);
            stream.SendNext(duringRoundData.teamAOccupying);
            stream.SendNext(duringRoundData.teamBOccupying);
            stream.SendNext(duringRoundData.occupyingReturnTimer);
            stream.SendNext(duringRoundData.roundEndTimer);
            stream.SendNext(duringRoundData.currentOccupationTeam);
            stream.SendNext(duringRoundData.occupyingA.rate);
            stream.SendNext(duringRoundData.occupyingB.rate);
            stream.SendNext(duringRoundData.occupyingTeam.name);
            stream.SendNext(duringRoundData.occupyingTeam.rate);
        }
        else
        {
            globalTimer = (float)stream.ReceiveNext();
            duringRoundData.teamAOccupying = (int)stream.ReceiveNext();
            duringRoundData.teamBOccupying = (int)stream.ReceiveNext();
            duringRoundData.occupyingReturnTimer = (float)stream.ReceiveNext();
            duringRoundData.roundEndTimer = (float)stream.ReceiveNext();
            duringRoundData.currentOccupationTeam = (string)stream.ReceiveNext();
            duringRoundData.occupyingA.rate = (float)stream.ReceiveNext();
            duringRoundData.occupyingB.rate = (float)stream.ReceiveNext();
            duringRoundData.occupyingTeam.name = (string)stream.ReceiveNext();
            duringRoundData.occupyingTeam.rate = (float)stream.ReceiveNext();
        }
    }
}