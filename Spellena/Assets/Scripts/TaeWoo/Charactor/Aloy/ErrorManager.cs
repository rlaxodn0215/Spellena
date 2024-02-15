using System.IO;
using DefineDatas;
public class ErrorManager
{
    public static bool isErrorOccur = false;
    private static string filePath = "Assets/GameDatas/Error.txt";
    private static StreamWriter sw;
    public static void SaveErrorData(ErrorCode errorCode)
    {
        isErrorOccur = true;
        if (!File.Exists(filePath))
        {
            sw = new StreamWriter(filePath);
            sw.WriteLine(MakeErrorSentence(errorCode));
            sw.Flush();
            sw.Close();
        }

        else
        {
            File.AppendAllText(filePath, MakeErrorSentence(errorCode) + "\n");
        }
    }

    public static void SaveErrorData(string data)
    {
        isErrorOccur = true;
        if (!File.Exists(filePath))
        {
            sw = new StreamWriter(filePath);
            sw.WriteLine(data);
            sw.Flush();
            sw.Close();
        }

        else
        {
            File.AppendAllText(filePath, data + "\n");
        }
    }

    private static string MakeErrorSentence(ErrorCode errorCode)
    {
        return "ERROR CODE " + (int)errorCode + " : " + errorCode;
    }

}
