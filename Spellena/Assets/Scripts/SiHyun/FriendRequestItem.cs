using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestItem : MonoBehaviour
{
    public Text senderName;
    private string senderId;

    public void SetAlarmInfo(string _senderId, string _senderName)
    {
        senderId = _senderId;

        string _name = "<color=black><b><i><size=17>" + _senderName + "</size></i></b></color>";
        string _text = "<color=white><size=14>" + " 님이\n친구 요청을 보냈습니다." + "</size></color>";
        string _styledText = _name + _text;

        senderName.text = _styledText;
    }

    public void OnClickAcceptButton()
    {
        FirebaseLoginManager.Instance.SetFriendRequestStatus(senderId, FirebaseLoginManager.Instance.GetUser().UserId, "accept");
    }

    public void OnClickRefuseButton()
    {
        FirebaseLoginManager.Instance.SetFriendRequestStatus(senderId, FirebaseLoginManager.Instance.GetUser().UserId, "refuse");
    }

    public void DestoryAlarm()
    {
        Destroy(this.gameObject);
    }
}
