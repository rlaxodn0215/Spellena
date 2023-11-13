using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchResultsItem : MonoBehaviour
{
    public Text userName;
    private string userId;

    private void Start()
    {
        userName = this.GetComponent<Text>();
    }

    public void SetItemInfo(string _userId, string _nickName)
    {
        userId = _userId;
        userName.text = _nickName;
    }

    public void OnClickInviteBtn()
    {

    }

    public void OnClickRequestFriendBtn()
    {
        var _sendUser = FirebaseLoginManager.Instance.GetUser();
        string _sendUserId = _sendUser.UserId;

        Debug.Log("Send User : " + _sendUserId);
        Debug.Log("Recevier User : " + userId);
        FirebaseLoginManager.Instance.SendFriendRequest(_sendUserId, userId);
    }

}
