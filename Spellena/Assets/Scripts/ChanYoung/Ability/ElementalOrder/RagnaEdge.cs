using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class RagnaEdge
{
    bool isReady = false;

    public void Initialize()
    {
        isReady = true;
    }

    public bool CheckReady()
    {
        return isReady;
    }

    public void EndSkill()
    {
        isReady = false;
    }
}
