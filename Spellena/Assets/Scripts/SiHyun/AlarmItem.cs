using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlarmItem : MonoBehaviour
{
    public Text senderName;
    private string senderId;

    public void SetAlarmInfo(string _senderId ,string _senderName)
    {
        senderId = _senderId;
        senderName.text = _senderName;
    }
}
