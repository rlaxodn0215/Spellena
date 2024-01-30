using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCultist : PlayerCommon
{
    private CultistPlainState cultistPlainState = CultistPlainState.None;
    public enum CultistPlainState
    {
        None, Ready, Rush, Throw
    }

    protected override void InitUniqueComponents()
    {
    }

    protected override void NotifyPlainIsOver(int index)
    {
        if (index == 1 || index == 2)
            plainIndex = -1;
    }

    protected override int ChangePlainIndex(int start, int type)
    {
        if (start == -1) // -1 -> 0
        {
            if (type == 0)
                return 0;
        }
        else if (start == 0)
        {
            if (type == 0)
                return 1;
            else
                return 2;
        }
        return -1;
    }

    /*
    protected override void PlayPlainLogic(bool IsLeft)
    {
        if(cultistPlainState == CultistPlainState.None)
        {

        }
        else if(cultistPlainState == CultistPlainState.Ready)
        {
            if(IsLeft)
                cultistPlainState = CultistPlainState.Rush;
            else
                cultistPlainState = CultistPlainState.Throw;
        }
    }
    */


    protected override void PlaySkillLogic(int index, SkillTiming timing)
    {
    }
}
