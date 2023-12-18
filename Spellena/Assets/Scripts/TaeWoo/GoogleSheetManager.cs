using UnityEngine;
using UnityEngine.Networking;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class GoogleSheetManager : EditorWindow
{
    private GoogleSheetData googleSheetData;

    private GameCenterTestData gameCenterTestData;
    private AeternaData aeternaData;
    private ElementalOrderData elementalOrderData;
    private CultistData cultistData;

    // URL
    private string URL;

    // 문자열로 나눈 2차원 배열
    private string[,] dividData;

    [MenuItem("Custom/GoogleSpreadSheetManager")]
    public static void ShowWindow()
    {
        EditorWindow wnd = GetWindow<GoogleSheetManager>();
        wnd.titleContent = new GUIContent("GoogleSheetManager");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Sheet Data", EditorStyles.boldLabel);
        googleSheetData = (GoogleSheetData)EditorGUILayout.ObjectField("SheetData", googleSheetData, typeof(GoogleSheetData), true);
        EditorGUILayout.LabelField("");

        gameCenterTestData = (GameCenterTestData)EditorGUILayout.ObjectField("GameCenterTestData", gameCenterTestData, typeof(GameCenterTestData), true);
        aeternaData = (AeternaData)EditorGUILayout.ObjectField("AeternaData", aeternaData, typeof(AeternaData), true);
        elementalOrderData = (ElementalOrderData)EditorGUILayout.ObjectField("ElementalOrderData", elementalOrderData, typeof(ElementalOrderData), true);
        cultistData = (CultistData)EditorGUILayout.ObjectField("CultistData", cultistData, typeof(CultistData), true);
        if (GUILayout.Button("데이터 불러오고 저장하기"))
        {
            InitData();

            Debug.Log("데이터 불러오고 저장 성공!");
        }

    }

    void InitData()
    {
        for (int i = 0; i < googleSheetData.gooleSheets.Length; i++)
        {
            URL = googleSheetData.gooleSheets[i].address + googleSheetData.exportFormattsv + googleSheetData.andRange
                + googleSheetData.gooleSheets[i].range_1 + ":" + googleSheetData.gooleSheets[i].range_2;

            UnityWebRequest www = UnityWebRequest.Get(URL);
            www.SendWebRequest();
            while (!www.isDone)
            {
                //Debug.Log("데이터 가져오는 중...");                                
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("데이터 가져오기 실패: " + www.error);
                return;
            }

            string Data = www.downloadHandler.text;
            Debug.Log(googleSheetData.name + " 데이터 가져오기 성공");

            //SaveToText(Data, i);
            DividText(Data);
            GiveData(i);
        }
    }

    void SaveToText(string data, int index)
    {
        switch (index)
        {
            case 0:
                string fullpth = "Assets/GameDatas/AeternaData.txt";
                StreamWriter sw;

                if (!File.Exists(fullpth))
                {
                    sw = new StreamWriter(fullpth + ".txt");
                    sw.WriteLine(data);

                    sw.Flush();
                    sw.Close();
                }

                else
                {
                    File.WriteAllText(fullpth, data);
                }

                break;
            case 1:
                string fullpth1 = "Assets/GameDatas/ElementalOrderData.txt";

                StreamWriter sw1;

                if (!File.Exists(fullpth1))
                {
                    sw1 = new StreamWriter(fullpth1 + ".txt");
                    sw1.WriteLine(data);

                    sw1.Flush();
                    sw1.Close();
                }

                else
                {
                    File.WriteAllText(fullpth1, data);
                }

                break;
            default:
                break;
        }
    }

    void DividText(string tsv)
    {
        //if (index == 0)
        //{
        //    tsv = ReadTxt("Assets/GameDatas/AeternaData.txt");
        //}

        //else if(index==1)
        //{
        //    tsv = ReadTxt("Assets/GameDatas/ElementalOrderData.txt");
        //}

        //else
        //{
        //    tsv = " ";
        //}

        //가로 기준으로 나눈다.
        string[] row = tsv.Split('\n');
        // 행의 양
        int rowSize = row.Length;
        // 열의 양
        int columSize = row[0].Split('\t').Length;

        dividData = new string[columSize, rowSize];

        // 행의 길이로 가로로 나눈다.
        for (int i = 0; i < rowSize; i++)
        {
            string[] column = row[i].Split('\t');

            // 열의 길이로 세로로 나눈다.
            for (int j = 0; j < columSize; j++)
            {
                dividData[j, i] = column[j];
                //Debug.Log(column[j]);
            }
        }
    }

    string ReadTxt(string filePath)
    {
        string value = "";

        if (File.Exists(filePath))
        {
            value = File.ReadAllText(filePath);
        }

        return value;
    }

    void GiveData(int index)
    {
        switch (index)
        {
            case 0:
                GiveGameCenterTestData();
                break;
            case 1:
                GiveAeternaData();
                break;
            case 2:
                GiveElementalOrderData();
                break;
            case 3:
                GiveCultistData();
                break;
            default:
                break;
        }
    }



    void GiveGameCenterTestData()
    {
        gameCenterTestData.loadingTime = 1;
        gameCenterTestData.characterSelectTime = 15;
        gameCenterTestData.readyTime = 1;
        gameCenterTestData.playerRespawnTime = float.Parse(dividData[1, 7]);
        gameCenterTestData.assistTime = 10;

        gameCenterTestData.angelStatueCoolTime = float.Parse(dividData[18, 0]);
        gameCenterTestData.angelStatueHpPerTime = int.Parse(dividData[16, 0]);
        gameCenterTestData.angelStatueContinueTime = int.Parse(dividData[17, 0]);

        gameCenterTestData.occupyingGaugeRate = 100.0f / (float.Parse(dividData[0, 0]) / float.Parse(dividData[2, 0]));
        gameCenterTestData.occupyingReturnTime = 3;
        gameCenterTestData.occupyingRate = 100.0f / (float.Parse(dividData[5, 0]) / float.Parse(dividData[7, 0]));
        gameCenterTestData.occupyingComplete = 99;
        gameCenterTestData.roundEndTime = int.Parse(dividData[8, 0]);
        gameCenterTestData.roundEndResultTime = 4;

        EditorUtility.SetDirty(gameCenterTestData);
    }

    void GiveAeternaData()
    {
        aeternaData.hp = int.Parse(dividData[28, 0]);
        aeternaData.moveSpeed = float.Parse(dividData[29, 0]);
        aeternaData.backSpeed = float.Parse(dividData[30, 0]);
        aeternaData.sideSpeed = float.Parse(dividData[31, 0]);
        aeternaData.runSpeedRatio = float.Parse(dividData[32, 0]);
        aeternaData.sitSpeed = float.Parse(dividData[33, 0]);
        aeternaData.sitBackSpeed = float.Parse(dividData[34, 0]);
        aeternaData.sitSideSpeed = float.Parse(dividData[35, 0]);
        aeternaData.jumpHeight = float.Parse(dividData[36, 0]);
        aeternaData.headShotRatio = float.Parse(dividData[39, 0]);

        //[Tooltip("기본 공격 쿨 타임")]
        aeternaData.basicAttackTime = float.Parse(dividData[9, 1]);
        //[Tooltip("스킬1 포탈 쿨 타임")]
        aeternaData.skill1DoorCoolTime = float.Parse(dividData[9, 2]);
        //[Tooltip("스킬2 쿨 타임")]
        aeternaData.skill2CoolTime = float.Parse(dividData[9, 3]);
        //[Tooltip("스킬3 쿨 타임")]
        aeternaData.skill3CoolTime = float.Parse(dividData[9, 5]);

        //[Tooltip("기본 공격 수명")]
        aeternaData.DimenstionSlash_0_lifeTime = float.Parse(dividData[47, 12]) / float.Parse(dividData[46, 12]);
        //[Tooltip("기본 공격 데미지")]
        aeternaData.DimenstionSlash_0_Damage = int.Parse(dividData[8, 6]);
        //[Tooltip("기본 공격 스피드")]
        aeternaData.DimenstionSlash_0_Speed = (int)(float.Parse(dividData[46, 12]));
        //[Tooltip("기본 공격 힐량")]
        aeternaData.DimenstionSlash_0_Healing = -int.Parse(dividData[8, 9]);

        //[Header("에테르나 스킬1 데이터")]
        //[Tooltip("스킬1 포탈 수명")]
        aeternaData.skill1Time = (int)(float.Parse(dividData[49, 13]));
        //[Tooltip("스킬1 포탈 소환 최대 사거리")]
        aeternaData.skill1DoorSpawnMaxRange = float.Parse(dividData[47, 13]);
        //[Tooltip("스킬1 포탈 적용 범위")]
        aeternaData.skill1DoorRange = (int)(float.Parse(dividData[48, 13]));
        //[Tooltip("스킬1 포탈 중심 힘")]
        aeternaData.skill1InnerForce = float.Parse(dividData[43, 13]);

        //[Header("에테르나 스킬2 데이터")]
        //[Tooltip("스킬2 지속 시간")]
        aeternaData.skill2DurationTime = float.Parse(dividData[16, 3]);
        //[Tooltip("스킬2 투사체 가지고 있는 시간")]
        aeternaData.skill2HoldTime = 10;

        //[Header("에테르나 스킬3 데이터")]
        //[Tooltip("스킬3 지속 시간")]
        aeternaData.skill3DurationTime = float.Parse(dividData[16, 5]);

        //[Header("에테르나 스킬4 데이터")]
        //[Tooltip("스킬4 궁극기 코스트")]
        aeternaData.skill4Cost = int.Parse(dividData[12, 6]);
        //[Tooltip("스킬4 지속 시간")]
        aeternaData.skill4DurationTime = 12;

        //[Header("스킬4 1단계 데이터")]
        //[Tooltip("스킬4 1단계 도달 시간")]
        aeternaData.skill4Phase1Time = float.Parse(dividData[11, 6]);
        //[Tooltip("1단계 공격 수명")]
        aeternaData.DimenstionSlash_1_lifeTime = float.Parse(dividData[47, 14]) / int.Parse(dividData[46, 14]);
        //[Tooltip("1단계 공격 데미지")]
        aeternaData.DimenstionSlash_1_Damage = int.Parse(dividData[8, 6]);
        //[Tooltip("1단계 공격 스피드")]
        aeternaData.DimenstionSlash_1_Speed = (int)(float.Parse(dividData[46, 14]));
        //[Tooltip("1단계 공격 힐량")]
        aeternaData.DimenstionSlash_1_Healing = -int.Parse(dividData[8, 9]);

        //[Header("스킬4 2단계 데이터")]
        //[Tooltip("스킬4 2단계 도달 시간")]
        aeternaData.skill4Phase2Time = float.Parse(dividData[11, 7]);
        //[Tooltip("2단계 공격 수명")]
        aeternaData.DimenstionSlash_2_lifeTime = float.Parse(dividData[47, 15]) / int.Parse(dividData[46, 15]);
        //[Tooltip("2단계 공격 데미지")]
        aeternaData.DimenstionSlash_2_Damage = int.Parse(dividData[8, 7]);
        //[Tooltip("2단계 공격 스피드")]
        aeternaData.DimenstionSlash_2_Speed = (int)(float.Parse(dividData[46, 15]));
        //[Tooltip("2단계 공격 힐량")]
        aeternaData.DimenstionSlash_2_Healing = -int.Parse(dividData[8, 10]);

        //[Header("스킬4 3단계 데이터")]
        //[Tooltip("스킬4 3단계 도달 시간")]
        aeternaData.skill4Phase3Time = float.Parse(dividData[11, 8]);
        //[Tooltip("3단계 공격 수명")]
        aeternaData.DimenstionSlash_3_lifeTime = float.Parse(dividData[47, 16]) / int.Parse(dividData[46, 16]);
        //[Tooltip("3단계 공격 데미지")]
        aeternaData.DimenstionSlash_3_Damage = int.Parse(dividData[8, 8]);
        //[Tooltip("3단계 공격 스피드")]
        aeternaData.DimenstionSlash_3_Speed = (int)(float.Parse(dividData[46, 16]));
        //[Tooltip("3단계 공격 힐량")]
        aeternaData.DimenstionSlash_3_Healing = -int.Parse(dividData[8, 11]);

        EditorUtility.SetDirty(aeternaData);
    }

    void GiveElementalOrderData()
    {
        elementalOrderData.hp = int.Parse(dividData[28, 0]);
        elementalOrderData.moveSpeed = float.Parse(dividData[29, 0]);
        elementalOrderData.backSpeed = float.Parse(dividData[30, 0]);
        elementalOrderData.sideSpeed = float.Parse(dividData[31, 0]);
        elementalOrderData.runSpeedRatio = float.Parse(dividData[32, 0]);
        elementalOrderData.sitSpeed = float.Parse(dividData[33, 0]);
        elementalOrderData.sitBackSpeed = float.Parse(dividData[34, 0]);
        elementalOrderData.sitSideSpeed = float.Parse(dividData[35, 0]);
        elementalOrderData.jumpHeight = float.Parse(dividData[36, 0]);
        elementalOrderData.headShotRatio = float.Parse(dividData[38, 0]);
        //스킬 1
        elementalOrderData.ragnaEdgeFloorDamage = float.Parse(dividData[8, 1]);
        elementalOrderData.ragnaEdgeCylinderDamage = float.Parse(dividData[8, 2]);
        elementalOrderData.rangaEdgeCoolDownTime = float.Parse(dividData[9, 2]);
        elementalOrderData.ragnaEdgeCastingTime = float.Parse(dividData[10, 1]);
        elementalOrderData.ragnaEdgeFloorLifeTime = float.Parse(dividData[16, 1]);
        elementalOrderData.ragnaEdgeCylinderLifeTime = float.Parse(dividData[16, 2]);
        elementalOrderData.rangaEdgeCylinderDebuff = dividData[19, 2];
        //스킬 2
        elementalOrderData.burstFlareBullet = int.Parse(dividData[4, 9]);
        elementalOrderData.burstFlareDamage = float.Parse(dividData[8, 9]);
        elementalOrderData.burstFlareCoolDownTime = float.Parse(dividData[9, 9]);
        elementalOrderData.burstFlareCastingTime = float.Parse(dividData[10, 9]);
        elementalOrderData.burstFlareLifeTime = float.Parse(dividData[16, 9]);
        //스킬 3
        elementalOrderData.gaiaTiedDamage = float.Parse(dividData[8, 3]);
        elementalOrderData.gaiaTiedCoolDownTime = float.Parse(dividData[9, 8]);
        elementalOrderData.gaiaTiedCastingTime = float.Parse(dividData[10, 3]);
        elementalOrderData.gaiaTiedMaxDistace = float.Parse(dividData[12, 3]);
        elementalOrderData.gaiaTiedLifeTime = new float[6];
        for (int i = 0; i < 6; i++)
        {
            elementalOrderData.gaiaTiedLifeTime[i] = float.Parse(dividData[16, 3 + i]);
        }
        //스킬 4
        elementalOrderData.meteorStrikeDamage = float.Parse(dividData[8, 10]);
        elementalOrderData.meteorStrikeCoolDownTime = float.Parse(dividData[9, 10]);
        elementalOrderData.meteorStrikeCastingTime = float.Parse(dividData[10, 10]);
        elementalOrderData.meteorStrikeMaxDistance = float.Parse(dividData[12, 10]);
        elementalOrderData.meteorStrikeLifeTime = float.Parse(dividData[16, 10]);
        //스킬 5
        elementalOrderData.terraBreakDamageFirst = float.Parse(dividData[8, 11]);
        elementalOrderData.terraBreakDamage = float.Parse(dividData[8, 12]);
        elementalOrderData.terraBreakCoolDownTime = float.Parse(dividData[9, 12]);
        elementalOrderData.terraBreakCastingTime = float.Parse(dividData[10, 12]);
        elementalOrderData.terraBreakMaxDistance = float.Parse(dividData[12, 11]);
        elementalOrderData.terraBreakLifeTimeFirst = float.Parse(dividData[16, 11]);
        elementalOrderData.terraBreakLifeTime = float.Parse(dividData[16, 12]);
        //스킬 6
        elementalOrderData.eterialStormDamage = float.Parse(dividData[8, 13]);
        elementalOrderData.eterialStormCoolDownTime = float.Parse(dividData[9, 13]);
        elementalOrderData.eterialStormCastingTime = float.Parse(dividData[10, 13]);
        elementalOrderData.eterialStormLifeTime = float.Parse(dividData[16, 13]);

        EditorUtility.SetDirty(elementalOrderData);
    }

    void GiveCultistData()
    {
        cultistData.hp = int.Parse(dividData[20, 0]);
        cultistData.moveSpeed = float.Parse(dividData[21, 0]);
        cultistData.backSpeed = float.Parse(dividData[22, 0]);
        cultistData.sideSpeed = float.Parse(dividData[23, 0]);
        cultistData.runSpeedRatio = float.Parse(dividData[24, 0]);
        cultistData.sitSpeed = float.Parse(dividData[25, 0]);
        cultistData.sitBackSpeed = float.Parse(dividData[26, 0]);
        cultistData.sitSideSpeed = float.Parse(dividData[27, 0]);
        cultistData.jumpHeight = float.Parse(dividData[28, 0]);
        cultistData.headShotRatio = float.Parse(dividData[31, 0]);
        //평타 관련
        cultistData.invocationCastingTime = float.Parse(dividData[0, 1]);
        cultistData.lungeHoldingTime = float.Parse(dividData[2, 2]);
        cultistData.lungeAttackDamage = float.Parse(dividData[5, 2]);
        cultistData.throwDamage = int.Parse(dividData[5, 5]);
        //스킬 1
        cultistData.skill1CastingTime = float.Parse(dividData[0, 6]);
        cultistData.skill1CoolDownTime = float.Parse(dividData[1, 6]);
        cultistData.skill1Damage = float.Parse(dividData[5, 6]);
        //스킬 2
        cultistData.skill2CastingTime = float.Parse(dividData[0, 12]);
        cultistData.skill2CoolDownTime = float.Parse(dividData[1, 12]);
        cultistData.skill2ChannelingTime = float.Parse(dividData[4, 12]);
        //스킬 3
        cultistData.skill3CastingTime = float.Parse(dividData[0, 17]);
        cultistData.skill3CoolDownTime = float.Parse(dividData[1, 17]);
        cultistData.skill3ChannelingTime = float.Parse(dividData[4, 17]);
        //스킬 4
        cultistData.skill4CoolDownTime = 0f;
        cultistData.skill4CastingTime = float.Parse(dividData[0, 27]);


        EditorUtility.SetDirty(cultistData);
    }

}

#endif