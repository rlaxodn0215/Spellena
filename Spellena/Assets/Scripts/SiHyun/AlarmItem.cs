using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlarmItem : MonoBehaviour
{
    public Text alarmType;
    public Text senderName;
    private string senderId;
    public Button acceptButton;

    public void SetAlarmInfo(string _senderId ,string _senderName, string _alarmType)
    {
        senderId = _senderId;
        senderName.text = _senderName;
        alarmType.text = _alarmType;
    }

    public void SetUpButtons(GameObject _quickMatchUI, GameObject _mainUI)
    {
        Button[] _buttons = GetComponentsInChildren<Button>();
        acceptButton.onClick.AddListener(() => OnButtonClick(_quickMatchUI, _mainUI, alarmType.text));
        Debug.Log("버튼 클릭 리스너 추가");
    }

    private void OnButtonClick(GameObject _quickMatchUI, GameObject _mainUI, string _alarmType)
    {
        if(_alarmType == "파티 요청")
        _quickMatchUI.SetActive(true);
        _mainUI.SetActive(false);
        Debug.Log("수락 버튼 눌림");
    }

    public void OnClickAcceptButton()
    {
        if (alarmType.text == "친구 요청")
        {
            FirebaseLoginManager.Instance.SetFriendRequestStatus(senderId, FirebaseLoginManager.Instance.GetUser().UserId, "accept");
        }
        else if (alarmType.text == "파티 요청")
        {
            FirebaseLoginManager.Instance.SetPartyRequestStatus(senderId, FirebaseLoginManager.Instance.GetUser().UserId, "accept");
        }
    }

    public void OnClickRefuseButton()
    {
        if (alarmType.text == "친구 요청")
        {
            FirebaseLoginManager.Instance.SetFriendRequestStatus(senderId, FirebaseLoginManager.Instance.GetUser().UserId, "refuse");
        }
        else if (alarmType.text == "파티 요청")
        {
            FirebaseLoginManager.Instance.SetPartyRequestStatus(senderId, FirebaseLoginManager.Instance.GetUser().UserId, "refuse");
        }
    }

    public void DestoryAlarm()
    {
        Destroy(this.gameObject);
    }
}
