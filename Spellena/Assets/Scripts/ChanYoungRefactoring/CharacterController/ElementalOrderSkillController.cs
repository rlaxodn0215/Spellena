using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalOrderSkillController : SkillController, IPunObservable
{

    private List<int> commands = new List<int>();

    /*
    기능 : 오버라이드 -> 스킬 키 입력을 받음
    인자 ->
    command : 스킬 키의 번호를 받음
    */
    protected override void GetSkillKey(int command)
    {
        AddCommand(command);
    }

    /*
    기능 : 엘리멘탈 오더 스킬 커맨드 추가
    command : 스킬 키의 번호를 받음
    */
    private void AddCommand(int command)
    {
        if (command == -1)
            commands.Clear();
        else if (command == 4)
            return;
        else if (commands.Count < 2)
            commands.Add(command);
    }

    /*
    기능 : 오버라이드 -> 2개의 저장된 커맨드의 변환 값에 따라 스킬 Index를 반환
    리턴 : 스킬 Index 반환
    */

    protected override int GetSkillIndex()
    {
        if (commands.Count >= 2)
        {
            int _sum = ExchangeSkillIndex(commands[0]) + ExchangeSkillIndex(commands[1]);
            if (_sum <= -2)
                return -1;
            else if (_sum <= 2)
                return 0;
            else if (_sum <= 4)
                return 1;
            else if (_sum <= 6)
                return 2;
            else if (_sum <= 8)
                return 3;
            else if (_sum <= 10)
                return 4;
            else if (_sum <= 14)
                return 5;
        }
        return -1;
    }

    /*
    기능 : 커맨드를 스킬 Index로 변환하기 위한 값 변경
    인자 -> input : 저장된 커맨드를 받음
    리턴 : 1번 -> 1, 2번 -> 3, 3번 -> 7을 반환
    */
    private int ExchangeSkillIndex(int input)
    {
        if (input == 1)
            return 1;
        else if (input == 2)
            return 3;
        else if (input == 3)
            return 7;
        return -1;
    }
}
