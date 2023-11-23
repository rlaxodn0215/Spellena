using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyRequestItem : MonoBehaviour
{
    public Text alarmType;
    public Text senderName;
    private string senderId;
    public Button acceptButton;

    public void SetAlarmInfo(string _senderId, string _senderName, string _alarmType)
    {
        senderId = _senderId;
        senderName.text = _senderName;
        alarmType.text = _alarmType;
    }

    public void SetUpButtons(GameObject _quickMatchUI, GameObject _mainUI, GameObject _friendUI)
    {
        acceptButton.onClick.AddListener(() => OnButtonClick(_quickMatchUI, _mainUI, _friendUI));
        Debug.Log("버튼 클릭 리스너 추가");
    }

    private void OnButtonClick(GameObject _quickMatchUI, GameObject _mainUI, GameObject _friendUI)
    {
        _quickMatchUI.SetActive(true);
        _mainUI.SetActive(false);
        _friendUI.SetActive(false);
        Debug.Log("수락 버튼 눌림");
    }

    public void OnClickAcceptButton()
    {
        FirebaseLoginManager.Instance.SetPartyRequestStatus(senderId, FirebaseLoginManager.Instance.GetUser().UserId, "accept");
    }

    public void OnClickRefuseButton()
    {
        FirebaseLoginManager.Instance.SetPartyRequestStatus(senderId, FirebaseLoginManager.Instance.GetUser().UserId, "refuse");
    }

    public void DestoryAlarm()
    {
        Destroy(this.gameObject);
    }
}

