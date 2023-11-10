using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoogleSheetData", menuName = "ScriptableObject/GoogleSheetData")]
public class GoogleSheetData : ScriptableObject
{
    public SheetData[] gooleSheets;

    [System.Serializable]
    public struct SheetData
    {
        [Header("이름")]
        public string name;
        [Header("구글 시트 주소(edit? 포함 이후의 문자열 제거)")]
        public string address;// = "https://docs.google.com/spreadsheets/d/1nRFZ43p-eW-c1i4hLp7vaqQt6ms10xjNhDdVvyk1HIQ/";
        [Header("참조할 시트 시작 셀")]
        public string range_1;// = "A2";
        [Header("참조할 시트 끝 셀")]
        public string range_2;// = "C";
    }

    // export?format=tsv => tsv 파일 형식으로 추출
    [HideInInspector]
    public string exportFormattsv = "export?format=tsv";
    // & range = A2:C3 => 가져올 엑셀 셀 범위
    [HideInInspector]
    public string andRange = "&range=";

}

