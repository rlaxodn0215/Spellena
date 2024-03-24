using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using SkillInfo;

public class PlayerSkillsComponentElementalOrder : MonoBehaviour
{
    public List<PlayerSkill> skills;
    private List<int> commands = new List<int>();

    private void Start()
    {
    }

    //입력 시 발생하는 이벤트
    public void OnSkill1(InputAction.CallbackContext context)
    {
        if (context.performed)
            AddCommand(0);
    }

    public void OnSkill2(InputAction.CallbackContext context)
    {
        if (context.performed)
            AddCommand(1);
    }

    public void OnSkill3(InputAction.CallbackContext context)
    {
        if (context.performed)
            AddCommand(2);
    }

    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (context.performed)
            PlaySkill();
        //context.canceled
    }

    //-------------------------------------------------------

    /*
    기능 : 스킬 준비 상태와 다른 스킬의 진행 상태를 확인하고 스킬 진행
    */
    private void PlaySkill()
    {
        int _index = GetTargetIndex();

        if (_index == -1)
            return;

        if (skills[_index].IsReady() && !IsProgressing())
            skills[_index].Play();
    }

    /*
    기능 : 진행 중인 스킬이 있는지 확인
    */
    private bool IsProgressing()
    {
        for(int i = 0; i < skills.Count; i++)
        {
            if (skills[i].IsProgressing())
                return true;
        }
        return false;
    }

    /*
    기능 : 사용할 스킬의 인덱스를 가져옴
    -> -1 : 실패 
       0 ~ 5 : 스킬 인덱스
    */
    private int GetTargetIndex()
    {
        if (commands.Count < 2)
            return -1;
        else
        {
            if (commands[0] == 0 && commands[1] == 0)
                return 0;
            else if ((commands[0] == 1 && commands[1] == 0)
                || (commands[0] == 0 && commands[1] == 1))
                return 1;
            else if ((commands[0] == 2 && commands[1] == 0)
                || (commands[0] == 0 && commands[1] == 2))
                return 2;
            else if (commands[0] == 1 && commands[1] == 1)
                return 3;
            else if ((commands[0] == 1 && commands[1] == 2)
                || (commands[0] == 2 && commands[1] == 1))
                return 4;
            else if (commands[0] == 2 && commands[1] == 2)
                return 5;
        }
        return -1;
    }

    /*
    기능 : 입력 키에 따라서 스킬 커맨드 추가
    */
    private void AddCommand(int index)
    {
        if (commands.Count < 2)
            commands.Add(index);
    }
}