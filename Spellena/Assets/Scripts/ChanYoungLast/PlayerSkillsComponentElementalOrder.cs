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

    //�Է� �� �߻��ϴ� �̺�Ʈ
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
    ��� : ��ų �غ� ���¿� �ٸ� ��ų�� ���� ���¸� Ȯ���ϰ� ��ų ����
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
    ��� : ���� ���� ��ų�� �ִ��� Ȯ��
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
    ��� : ����� ��ų�� �ε����� ������
    -> -1 : ���� 
       0 ~ 5 : ��ų �ε���
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
    ��� : �Է� Ű�� ���� ��ų Ŀ�ǵ� �߰�
    */
    private void AddCommand(int index)
    {
        if (commands.Count < 2)
            commands.Add(index);
    }
}