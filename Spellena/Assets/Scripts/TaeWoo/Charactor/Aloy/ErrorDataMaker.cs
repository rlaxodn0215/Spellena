using System.IO;
public class ErrorDataMaker
{
    public static bool isErrorOccur = false;
    private static string filePath = "Assets/GameDatas/Error.txt";
    private static StreamWriter sw;
    public static void SaveErrorData(int errorCode)
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

    private static string MakeErrorSentence(int errorCode)
    {
        string sentence = "ERROR CODE " + errorCode + " : ";
        switch(errorCode)
        {
            case 0: return (sentence + "AbilityMaker_Tree_NULL");
            case 1: return (sentence + "AbilityMaker_data_NULL");
            case 2: return (sentence + "AloyBT_animator_NULL");
            case 3: return (sentence + "AloyBT_abilityMaker_NULL");
            case 4: return (sentence + "AloyBT_aimingTrasform_NULL");
            case 5: return (sentence + "EnemyDetector_condition_NULL");
            case 6: return (sentence + "EnemyDetector_playerTransform_NULL");
            case 7: return (sentence + "EnemyDetector_bowAnimator_NULL");
            case 8: return (sentence + "EnemyDetector_animator_NULL");
            case 9: return (sentence + "NormalArrowAttack_playerTransform_NULL");
            case 10: return (sentence + "NormalArrowAttack_attackTransform_NULL");
            case 11: return (sentence + "NormalArrowAttack_bowAnimator_NULL");
            case 12: return (sentence + "NormalArrowAttack_arrowAniObj_NULL");
            case 13: return (sentence + "NormalArrowAttack_agent_NULL");
            case 14: return (sentence + "NormalArrowAttack_animator_NULL");
            case 15: return (sentence + "NormalArrowAttack_avoidTimer_NULL");
            case 16: return (sentence + "GotoOccupationArea_arrowAniObj_NULL");
            case 17: return (sentence + "GotoOccupationArea_agent_NULL");
            case 18: return (sentence + "GotoOccupationArea_animator_NULL");
        }
        return sentence;
    }

}
