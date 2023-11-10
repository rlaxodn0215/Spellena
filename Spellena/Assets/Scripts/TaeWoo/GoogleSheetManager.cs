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
    private object[,] dividData;

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

        dividData = new object[rowSize, columSize];

        // 행의 길이로 가로로 나눈다.
        for (int i = 0; i < rowSize; i++)
        {
            string[] column = row[i].Split('\t');

            // 열의 길이로 세로로 나눈다.
            for (int j = 0; j < columSize; j++)
            {
                dividData[i, j] = column[j];
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

    }
}