using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchResultsItem : MonoBehaviour
{
    public Text userName;
    private string userId;

    private MatchMaking matchMaking;

    private void Start()
    {
        userName = this.GetComponent<Text>();
        matchMaking = GetComponent<MatchMaking>();
    }

    public void SetItemInfo(string _userId, string _nickName)
    {
        userId = _userId;
        userName.text = _nickName;
    }

    public void OnClickInviteBtn()
    {
        var _sendUser = FirebaseLoginManager.Instance.GetUser();
        string _sendUserId = _sendUser.UserId;

        if (_sendUserId != userId)
        {
            FirebaseLoginManager.Instance.SendPartyRequest(_sendUserId, userId);
            FirebaseLoginManager.Instance.SetLobbyMaster(_sendUserId, true);
        }
    }

    public void OnClickRequestFriendBtn()
    {
        var _sendUser = FirebaseLoginManager.Instance.GetUser();
        string _sendUserId = _sendUser.UserId;

        if (_sendUserId != userId)
        {
            FirebaseLoginManager.Instance.SendFriendRequest(_sendUserId, userId);
        }
    }

}
