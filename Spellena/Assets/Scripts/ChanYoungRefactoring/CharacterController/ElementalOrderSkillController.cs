using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalOrderSkillController : SkillController, IPunObservable
{

    private List<int> commands = new List<int>();

    /*
    ��� : �������̵� -> ��ų Ű �Է��� ����
    ���� ->
    command : ��ų Ű�� ��ȣ�� ����
    */
    protected override void GetSkillKey(int command)
    {
        AddCommand(command);
    }

    /*
    ��� : ������Ż ���� ��ų Ŀ�ǵ� �߰�
    command : ��ų Ű�� ��ȣ�� ����
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
    ��� : �������̵� -> 2���� ����� Ŀ�ǵ��� ��ȯ ���� ���� ��ų Index�� ��ȯ
    ���� : ��ų Index ��ȯ
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
    ��� : Ŀ�ǵ带 ��ų Index�� ��ȯ�ϱ� ���� �� ����
    ���� -> input : ����� Ŀ�ǵ带 ����
    ���� : 1�� -> 1, 2�� -> 3, 3�� -> 7�� ��ȯ
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
