using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetManager : MonoBehaviour
{
    public GoogleSheetData googleSheetData;
    [Header("")]
    public AeternaData aeternaData;

    private string URL;

    // 문자열로 나눈 2차원 배열
    private string[,] dividData;

    IEnumerator Start()
    {
        for(int i = 0; i < googleSheetData.gooleSheets.Length; i++)
        {
            URL = googleSheetData.gooleSheets[i].address + googleSheetData.exportFormattsv + googleSheetData.andRange
                + googleSheetData.gooleSheets[i].range_1 + ":" + googleSheetData.gooleSheets[i].range_2;

            UnityWebRequest www = UnityWebRequest.Get(URL);
            yield return www.SendWebRequest();   // URL과 통신

            string Data = www.downloadHandler.text; // string 데이터로 받아옴
            Debug.Log("End Receving");

            DividText(Data);
            GiveData(i);
        }
        
    }


    void DividText(string tsv)
    {
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

    void GiveData(int index)
    {
        switch(index)
        {
            case 0:
                GiveAeternaData();
                break;
            default:
                break;
        }
    }

    void GiveAeternaData()
    {
        Debug.Log(dividData[28, 0]);
        //aeternaData.Hp = (int)dividData[28,0];
        //aeternaData.sitSpeed;
        //aeternaData.walkSpeed;
        //[Tooltip("달리는 속도")]
        //aeternaData.runSpeed;
        //[Tooltip("점프 높이")]
        //aeternaData.jumpHeight;

        //[Header("에테르나 기본 공격 데이터")]
        //[Tooltip("기본 공격 쿨 타임")]
        //aeternaData.basicAttackTime;
        //[Tooltip("기본 공격 수명")]
        //aeternaData.DimenstionSlash_0_lifeTime;
        //[Tooltip("기본 공격 데미지")]
        //aeternaData.DimenstionSlash_0_Damage;
        //[Tooltip("기본 공격 스피드")]
        //aeternaData.DimenstionSlash_0_Speed;
        //[Tooltip("기본 공격 힐량")]
        //aeternaData.DimenstionSlash_0_Healing;

        //[Header("에테르나 스킬1 데이터")]
        //[Tooltip("스킬1 포탈 수명")]
        //aeternaData.skill1Time;
        //[Tooltip("스킬1 포탈 쿨 타임")]
        //aeternaData.skill1DoorCoolTime;
        //[Tooltip("스킬1 포탈 소환 최대 고리")]
        //aeternaData.skill1DoorSpawnMaxRange;
        //[Tooltip("스킬1 포탈 적용 범위")]
        //aeternaData.skill1DoorRange;
        //[Tooltip("스킬1 포탈 디버프 비율")]
        //aeternaData.skill1DeBuffRatio;

        //[Header("에테르나 스킬2 데이터")]
        //[Tooltip("스킬2 지속 시간")]
        //aeternaData.skill2DurationTime;
        //[Tooltip("스킬2 투사체 가지고 있는 시간")]
        //aeternaData.skill2HoldTime;
        //[Tooltip("스킬2 쿨 타임")]
        //aeternaData.skill2CoolTime;

        //[Header("에테르나 스킬3 데이터")]
        //[Tooltip("스킬3 지속 시간")]
        //aeternaData.skill3DurationTime;
        //[Tooltip("스킬3 쿨 타임")]
        //aeternaData.skill3CoolTime;

        //[Header("에테르나 스킬4 데이터")]
        //[Tooltip("스킬4 궁극기 코스트")]
        //aeternaData.skill4Cost;
        //[Tooltip("스킬4 지속 시간")]
        //aeternaData.skill4DurationTime;

        //[Header("스킬4 1단계 데이터")]
        //[Tooltip("스킬4 1단계 도달 시간")]
        //aeternaData.skill4Phase1Time;
        //[Tooltip("1단계 공격 수명")]
        //aeternaData.DimenstionSlash_1_lifeTime;
        //[Tooltip("1단계 공격 데미지")]
        //aeternaData.DimenstionSlash_1_Damage;
        //[Tooltip("1단계 공격 스피드")]
        //aeternaData.DimenstionSlash_1_Speed;
        //[Tooltip("1단계 공격 힐량")]
        //aeternaData.DimenstionSlash_1_Healing;

        //[Header("스킬4 2단계 데이터")]
        //[Tooltip("스킬4 2단계 도달 시간")]
        //aeternaData.skill4Phase2Time;
        //[Tooltip("2단계 공격 수명")]
        //aeternaData.DimenstionSlash_2_lifeTime;
        //[Tooltip("2단계 공격 데미지")]
        //aeternaData.DimenstionSlash_2_Damage;
        //[Tooltip("2단계 공격 스피드")]
        //aeternaData.DimenstionSlash_2_Speed;
        //[Tooltip("2단계 공격 힐량")]
        //aeternaData.DimenstionSlash_2_Healing;

        //[Header("스킬4 3단계 데이터")]
        //[Tooltip("스킬4 3단계 도달 시간")]
        //aeternaData.skill4Phase3Time;
        //[Tooltip("3단계 공격 수명")]
        //aeternaData.DimenstionSlash_3_lifeTime;
        //[Tooltip("3단계 공격 데미지")]
        //aeternaData.DimenstionSlash_3_Damage;
        //[Tooltip("3단계 공격 스피드")]
        //aeternaData.DimenstionSlash_3_Speed;
        //[Tooltip("3단계 공격 힐량")]
        //aeternaData.DimenstionSlash_3_Healing;
    }
}