public class CoolTimer
{
    private float dataCoolTime;
    private float curCoolTime;
    public CoolTimer(float _dataCoolTime)
    {
        dataCoolTime = _dataCoolTime;
    }

    public void UpdateCoolTime(float _deltaTime)
    {
         curCoolTime += _deltaTime;
    }

    public void ChangeCoolTime(float _curCoolTime)
    {
         curCoolTime = _curCoolTime;
    }

    public bool IsCoolTimeFinish()
    {
        return curCoolTime >= dataCoolTime;
    }
}
