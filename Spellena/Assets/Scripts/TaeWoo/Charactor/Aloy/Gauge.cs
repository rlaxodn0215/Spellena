using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gauge
{
    private float dataCoolTime = -1;
    private float curCoolTime = -1;

    private int dataUltimateGauage = -1;
    private int curUltimateGauage = -1;

    public Gauge(){}

    public Gauge(float _dataCoolTime)
    {
        dataCoolTime = _dataCoolTime;
        curCoolTime = 0.0f;
    }

    public Gauge(int _dataUltimateGauage)
    {
        dataUltimateGauage = _dataUltimateGauage;
        curUltimateGauage = 0;
    }

    public void UpdateCurCoolTime(float deltaTime)
    {
        if(dataCoolTime >= 0)
            curCoolTime += deltaTime;
    }

    public void ChangeCurCoolTime(float _curCoolTime)
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

    public bool IsCoolTimeFinish()
    {
        return curCoolTime >= dataCoolTime;
    }

    public bool CanUseUltimate()
    {
        return curUltimateGauage >= dataUltimateGauage;
    }
}
