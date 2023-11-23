using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Firebase.Database;

public class FriendItem : MonoBehaviour
{
    string userId;
    public Text userNickname;
    public GameObject offlineLayer;
    public GameObject functionButtons;
    public Button inviteButton;

    private string senderId;

    private void SetOfflineLayer(DataSnapshot _snapshot)
    {
        Debug.Log("함수 호출");
        string _status = _snapshot.Value.ToString();
        if (_status == "온라인")
        {
            offlineLayer.SetActive(false);
            functionButtons.SetActive(true);
            Debug.LogWarning("Unexpected status: " + _status);

        }
        else if (_status == "오프라인")
        {
            offlineLayer.SetActive(true);
            functionButtons.SetActive(false);
            Debug.LogWarning("Unexpected status: " + _status);
        }
    }

    public void SetFriendInfo(string _id, string _nickname, string _status)
    {
        userId = _id;
        userNickname.text = _nickname;
        var _sendUser = FirebaseLoginManager.Instance.GetUser();
        senderId = _sendUser.UserId;
        if (_status == "온라인")
        {
            functionButtons.SetActive(true);
            offlineLayer.SetActive(false);
            Debug.LogWarning("Unexpected status: " + _status);
        }
        else if (_status == "오프라인")
        {
            functionButtons.SetActive(false);
            offlineLayer.SetActive(true);
            Debug.LogWarning("Unexpected status: " + _status);
        }

        DatabaseReference statusRef = FirebaseLoginManager.Instance.GetReference().Child("users").Child(_id).Child("status");
        statusRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            try
            {
                SetOfflineLayer(args.Snapshot);
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception in ValueChanged event handler: " + ex.Message);
            }
        };
    }

    public void SetUpButtons(GameObject _quickMatchUI, GameObject _mainUI, GameObject _friendUI, Action<string> action)
    {
        inviteButton.onClick.AddListener(() => OnButtonClick(_quickMatchUI, _mainUI, _friendUI, action));
        Debug.Log("버튼 클릭 리스너 추가");
    }

    private void OnButtonClick(GameObject _quickMatchUI, GameObject _mainUI, GameObject _friendUI, Action<string> action)
    {
        FirebaseLoginManager.Instance.SendPartyRequest(senderId, userId);
        FirebaseLoginManager.Instance.SetLobbyMaster(senderId, true);
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

}
