using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FriendItem : MonoBehaviour
{
    string userId;
    public Text userNickname;
   
    public void SetFriendInfo(string _id, string _nickname)
    {
        userId = _id;
        userNickname.text = _nickname;
    }
}
