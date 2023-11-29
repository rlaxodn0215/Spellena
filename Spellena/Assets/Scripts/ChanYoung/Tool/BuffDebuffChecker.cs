using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffDebuffChecker : MonoBehaviourPunCallbacks
{
    List<string> buffsAndDebuffs = new List<string>();
    List<object> buffDebuffData = new List<object>();
    List<float> leftTime = new List<float>();

    private void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < buffsAndDebuffs.Count; i++)
            {
                if (leftTime[i] > 0f)
                    leftTime[i] -= Time.deltaTime;
            }

            for (int i = 0; i < buffsAndDebuffs.Count; i++)
            {
                if (leftTime[i] <= 0f)
                {
                    buffsAndDebuffs.RemoveAt(i);
                    leftTime.RemoveAt(i);
                    i = -1;
                }
            }
        }
    }

    void CallRPCTunnel(string tunnelCommand)
    {
        object[] _tempData;
        _tempData = new object[3];
        _tempData[0] = tunnelCommand;
        _tempData[1] = buffsAndDebuffs.ToArray();
        _tempData[2] = leftTime.ToArray();

        photonView.RPC("CallRPCTunnelBuffDebuff", RpcTarget.AllBuffered, _tempData);
    }

    [PunRPC]
    public void CallRPCTunnelBuffDebuff(object[] data)
    {
        if ((string)data[0] == "UpdateData")
            UpdateData(data);
    }
    void UpdateData(object[] data)
    {
        buffsAndDebuffs = ((string[])data[1]).ToList();
        leftTime = ((float[])data[2]).ToList();
    }

    public void SetNewBuffDebuff(string buffDebuff)
    {
        for (int i = 0; i < buffsAndDebuffs.Count; i++)
        {
            if (buffsAndDebuffs[i] == buffDebuff)
            {
                UpdateBuffDebuff(buffDebuff, i);
                return;
            }
        }
        AddBuffDebuff(buffDebuff);
    }

    void UpdateBuffDebuff(string buffDebuff, int i)
    {
    }

    void AddBuffDebuff(string buffDebuff)
    {
        if(buffDebuff == "TerribleTentacles")
        {

        }
    }

    public bool CheckBuffDebuff(string buffDebuff)
    {
        for(int i = 0; i < buffsAndDebuffs.Count; i++)
        {
            if (buffsAndDebuffs[i] == buffDebuff)
            {
                if (leftTime[i] > 0f)
                    return true;
            }
        }
        return false;
    }


}
