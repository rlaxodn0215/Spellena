using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemberItem : MonoBehaviour
{
    string userId;
    public Text userNickname;

    public void SetMemberInfo(string _id, string _nickname)
    {
        userId = _id;
        userNickname.text = _nickname;
    }
}
