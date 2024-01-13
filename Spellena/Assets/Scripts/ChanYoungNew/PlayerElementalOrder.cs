using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using TMPro.EditorUtilities;
using System;

public class PlayerElementalOrder : PlayerCommon
{
    private List<int> commands = new List<int>();


    protected override void InitUniqueComponents()
    {
        AddSkill(2);
        for(int i = 0; i < skillDatas.Count; i++)
            skillDatas[i].isUnique = true;
    }

    protected override void OnSkill1()
    {
        AddCommand(1);
    }
    protected override void OnSkill2()
    {
        AddCommand(2);
    }
    protected override void OnSkill3()
    {
        AddCommand(3);
    }
    protected override void OnSkill4()
    {
    }

    protected override void OnMouseButton()
    {
        isClicked = !isClicked;
        if(isClicked)
        {
            int _index = GetIndexByCommands();
            //¿¤¸®¸àÅ» ¿À´õ´Â ÆòÅ¸°¡ ¾ø´Ù
            if (_index >= 0)
            {
                if (skillDatas[_index].skillState == SkillData.SkillState.None)
                    photonView.RPC("ClickMouse", RpcTarget.MasterClient, _index, false);
                else if (skillDatas[_index].skillState == SkillData.SkillState.Unique)
                    photonView.RPC("ClickMouse", RpcTarget.MasterClient, _index, true);
            }
        }
    }

    protected override void CancelSkill()
    {
        base.CancelSkill();
        commands.Clear();
    }


    private int GetIndexByCommands()
    {
        if(commands.Count >= 2)
        {
            if (commands[0] == 1 && commands[1] == 1)
                return 0;
            else if ((commands[0] == 1 && commands[1] == 2) || (commands[0] == 2 && commands[1] == 1))
                return 1;
            else if ((commands[0] == 1 && commands[1] == 3) || (commands[0] == 3 && commands[1] == 1))
                return 2;
            else if (commands[0] == 2 && commands[1] == 2)
                return 3;
            else if ((commands[0] == 2 && commands[1] == 3) || (commands[0] == 3 && commands[1] == 2))
                return 4;
            else
                return 5;
        }
        return -1;
    }

    private void AddCommand(int command)
    {
        if(commands.Count < 2)
            commands.Add(command);
    }

    [PunRPC]
    public override void SetSkillPlayer(int index, int nextSkillState)
    {
        base.SetSkillPlayer(index, nextSkillState);
        if (photonView.IsMine && (SkillData.SkillState)nextSkillState == SkillData.SkillState.Casting)
            commands.Clear();
    }

}
