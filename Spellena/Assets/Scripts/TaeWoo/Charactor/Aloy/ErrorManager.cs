using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using DefineDatas;
public class ErrorManager
{
    public static bool isErrorOccur = false;
    private static string reportMail = "rlaxodn0215@naver.com";
    private static string filePath = "Assets/GameDatas/Error.txt";
    private static string deviceInfo = "-------------\n" +
                                       "Device Model : " + SystemInfo.deviceModel + "\n" +
                                       "Device OS : " + SystemInfo.operatingSystem + "\n" +
                                       "-------------\n";
    private static StreamWriter sw;

    private static void SetErrorOccur()
    {
        if (isErrorOccur) return;
        isErrorOccur = true;

        if (!File.Exists(filePath))
        {
            sw = new StreamWriter(filePath);
            sw.WriteLine(deviceInfo);
            sw.Flush(); sw.Close();
        }

        else
        {
            File.WriteAllText(filePath, deviceInfo);
        }

    }
    public static void SaveErrorData(ErrorCode errorCode)
    {
        SetErrorOccur();
        if (!File.Exists(filePath))
        {
            sw = new StreamWriter(filePath);
            sw.WriteLine(MakeErrorSentence(errorCode));
            sw.Flush(); sw.Close();
        }

        else
        {
            File.AppendAllText(filePath, MakeErrorSentence(errorCode) + "\n");
        }
    }

    public static void SaveErrorData(string data)
    {
        SetErrorOccur();
        if (!File.Exists(filePath))
        {
            sw = new StreamWriter(filePath);
            sw.WriteLine(data);
            sw.Flush(); sw.Close();
        }

        else
        {
            File.AppendAllText(filePath, data + "\n");
        }
    }

    public static void SaveErrorData(ErrorCode errorCode, string addData)
    {
        SetErrorOccur();
        if (!File.Exists(filePath))
        {
            sw = new StreamWriter(filePath);
            sw.WriteLine(MakeErrorSentence(errorCode));
            sw.Flush(); sw.Close();
        }

        else
        {
            File.AppendAllText(filePath, MakeErrorSentence(errorCode) + addData + "\n");
        }
    }

    private static string MakeErrorSentence(ErrorCode errorCode)
    {
        return "ERROR CODE " + (int)errorCode + " : " + errorCode.ToString();
    }

    public static void SendErrorReport()
    {
        string title = EscapeURL("Error_Report");
        string body = EscapeURL(ReadErrorText(filePath));
        Application.OpenURL("mailto:" + reportMail + "?subject=" + title + "&body=" + body);
    }

    static string ReadErrorText(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        string error = "";

        if (fileInfo.Exists)
        {
            StreamReader reader = new StreamReader(filePath);
            error = reader.ReadToEnd();
            reader.Close();
        }

        else
            error = "해당 Text파일이 없습니다.";

        return error;
    }

    private static string EscapeURL(string url)
    {
        return UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }

    // PlayerSettings의 ScriptCompilation에서 ENABLE_DEBUG가 추가 되어야 함수 호출
    [System.Diagnostics.Conditional("ENABLE_DEBUG")]
    public static void Log(object message)
    {
        Debug.Log(message);
    }
}
