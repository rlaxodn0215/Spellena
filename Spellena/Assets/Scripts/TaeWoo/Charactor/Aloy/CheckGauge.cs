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
        curCoolTime = 0.0f;
    }

    public CheckGauge(int _dataUltimateGauage)
    {
        dataUltimateGauage = _dataUltimateGauage;
        curUltimateGauage = 0;
    }

    public void UpdateCurCoolTime()
    {
        if(dataCoolTime >= 0)
            curCoolTime += Time.deltaTime;
    }

    public void UpdateCurCoolTime(float _curCoolTime)
    {
        if (dataCoolTime >= 0)
            curCoolTime = _curCoolTime;
    }

    public void UpdateCurUltimateGauage()
    {
        if (dataUltimateGauage >= 0)
            curUltimateGauage++;
    }

    public void UpdateCurUltimateGauage(int _curUltimateGauage)
    {
        if (dataUltimateGauage >= 0)
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
