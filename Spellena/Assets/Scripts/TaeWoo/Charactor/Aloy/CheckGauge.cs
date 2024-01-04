using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGauge
{
    private float dataCoolTime = -1;
    private float curCoolTime = -1;

    private int dataUltimateGauage = -1;
    private int curUltimateGauage = -1;

    public CheckGauge(){}

    public CheckGauge(float _dataCoolTime)
    {
        dataCoolTime = _dataCoolTime;
    }

    public CheckGauge(int _dataUltimateGauage)
    {
        dataUltimateGauage = _dataUltimateGauage;
    }

    public void UpdateCurCoolTime(float _curCoolTime)
    {
        curCoolTime = _curCoolTime;
    }

    public void UpdateCurUltimateGauage(int _curUltimateGauage)
    {
        curUltimateGauage = _curUltimateGauage;
    }

    public bool CheckCoolTime()
    {
        return curCoolTime >= dataCoolTime;
    }

    public bool CheckUltimateGauge()
    {
        return curUltimateGauage >= dataUltimateGauage;
    }
}
