using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SearchResultsItem : MonoBehaviour
{
    public Text userName;
    private string userId;
    private string senderId;
    public Button inviteButton;
    

    private void Start()
    {
        userName = this.GetComponent<Text>();
    }

    public void SetItemInfo(string _userId, string _nickName)
    {
        userId = _userId;
        userName.text = _nickName;

        var _sendUser = FirebaseLoginManager.Instance.GetUser();
        senderId = _sendUser.UserId;
    }

    public void SetUpButtons(GameObject _quickMatchUI, GameObject _mainUI, GameObject _friendUI, Action<string> action)
    {
        inviteButton.onClick.AddListener(() => OnButtonClick(_quickMatchUI, _mainUI, _friendUI, action));
        Debug.Log("버튼 클릭 리스너 추가");
    }

    private void OnButtonClick(GameObject _quickMatchUI, GameObject _mainUI, GameObject _friendUI, Action<string> action)
    {
        _quickMatchUI.SetActive(true);
        _mainUI.SetActive(false);
        _friendUI.SetActive(false);
        action(senderId);
        Debug.Log("초대 버튼 눌림");
    }

    public void ExecuteFunction(Action<string> action, string _userId)
    {
        action(_userId);
    }

    public void OnClickInviteBtn()
    {
        if (senderId != userId)
        {
            FirebaseLoginManager.Instance.SendPartyRequest(senderId, userId);
            FirebaseLoginManager.Instance.SetLobbyMaster(senderId, true);
        }
    }

    public void OnClickRequestFriendBtn()
    {
        if (senderId != userId)
        {
            FirebaseLoginManager.Instance.SendFriendRequest(senderId, userId);
        }
    }

}
