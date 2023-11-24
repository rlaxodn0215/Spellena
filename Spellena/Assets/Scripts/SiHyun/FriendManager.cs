using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Firebase.Database;

public class FriendManager : MonoBehaviour
{
    // 검색 결과
    List<SearchResultsItem> resultsNickNameList = new List<SearchResultsItem>();
    public SearchResultsItem searchResultsItem;
    public Transform contentObjects;
    public InputField searchInputField;
    public Text nullResultText;

    // 친구 초대 알림
    List<FriendRequestItem> friendRequestAlarmItemList = new List<FriendRequestItem>();
    public FriendRequestItem friendRequestAlarmItem;

    //파티 초대 알림
    List<PartyRequestItem> partyRequestAlarmItemList = new List<PartyRequestItem>();
    public PartyRequestItem partyRequestAlarmItem;

    public Transform alarmSpace;

    // 친구 목록
    List<FriendItem> friendItemsList = new List<FriendItem>();
    public FriendItem friendItem;
    public Transform friendList;

    // 파티원 목록
    List<MemberItem> memberList = new List<MemberItem>();
    public MemberItem memberItem;
    public Transform memberSpace;

    public GameObject matchUI;
    public GameObject mainUI;
    public GameObject friendUI;

    string userId;

    DatabaseReference reference;

    private void Start()
    {
        userId = FirebaseLoginManager.Instance.GetUser().UserId;
        reference = FirebaseLoginManager.Instance.GetReference();

        DatabaseReference friendRef = FirebaseLoginManager.Instance.GetReference().Child("friendRequests");
        friendRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            try
            {
                if (args.Snapshot != null && args.Snapshot.HasChildren)
                {
                    FriendRequestAlarm(args.Snapshot);
                }
            }
            catch(Exception ex)
            {
                Debug.LogError("Exception in ValueChanged event handler: " + ex.Message);
            }
        };

        DatabaseReference partyRef = FirebaseLoginManager.Instance.GetReference().Child("partyRequests");
        partyRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            try
            {
                if (args.Snapshot != null && args.Snapshot.HasChildren)
                {
                    PartyRequestAlarm(args.Snapshot);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception in ValueChanged event handler: " + ex.Message);
            }
        };

        DatabaseReference friendsDataRef = FirebaseLoginManager.Instance.GetReference().Child("users").Child(userId).Child("friendList");
        friendsDataRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            try
            {
                if (args.Snapshot != null && args.Snapshot.HasChildren)
                {
                    UpdateFriendList(userId);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception in ValueChanged event handler: " + ex.Message);
            }
        };

        DatabaseReference partyDataRef = FirebaseLoginManager.Instance.GetReference().Child("users").Child(userId).Child("partyMemberList");
        partyDataRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            try
            {
                if (args.Snapshot != null && args.Snapshot.HasChildren)
                {
                    UpdatePartyMemberList(userId);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception in ValueChanged event handler: " + ex.Message);
            }
        };

        nullResultText.gameObject.SetActive(false);
        if (searchInputField != null)
        {
            searchInputField.onEndEdit.AddListener(SearchUser);
        }
    }

    public void AcceptFriend(string _userId, string _friendId)
    {
        reference.Child("users").Child(_userId).Child("friendList").Child(_friendId).SetValueAsync("friend");
        reference.Child("users").Child(_friendId).Child("friendList").Child(_userId).SetValueAsync("friend");
    }

    async void FriendRequestAlarm(DataSnapshot _data)
    {
        DataSnapshot _ref = _data.Child(userId);

        if (_ref.HasChildren && _ref != null)
        {
            foreach(var _childrenSnapshot in _ref.Children)
            {
                if(_childrenSnapshot != null)
                {
                    string _requestStatus = _childrenSnapshot.Child("status").Value.ToString();
                    if (_requestStatus.Equals("accept"))
                    {
                        object _senderUserIdValue = _childrenSnapshot.Child("senderUserId")?.Value;
                        if (_senderUserIdValue != null)
                        {
                            string _friendId = _childrenSnapshot.Child("senderUserId").Value.ToString();
                            AcceptFriend(userId, _friendId);
                        }
                    }
                    else if (_requestStatus.Equals("pending"))
                    {
                        object _senderUserIdValue = _childrenSnapshot.Child("senderUserId")?.Value;
                        if (_senderUserIdValue != null)
                        {
                            string _userId = _childrenSnapshot.Child("senderUserId").Value.ToString();
                            string _userNickName = await FirebaseLoginManager.Instance.ReadUserInfo(_userId);
                            FriendRequestItem _alarmItem = Instantiate(friendRequestAlarmItem, alarmSpace);
                            _alarmItem.SetAlarmInfo(_userId, _userNickName);
                            friendRequestAlarmItemList.Add(_alarmItem);
                            Debug.Log(_userId + " : " + _userNickName);
                        }
                    }
                }
            }
        }
    }

    async void UpdateFriendList(string _userId)
    {
        ResetResults(friendItemsList);

        List<string> _friendList = await FirebaseLoginManager.Instance.GetFriendsList(_userId);

        if(_friendList != null)
        {
            foreach(var _friendId in _friendList)
            {
                string _friendNickname = await FirebaseLoginManager.Instance.ReadUserInfo(_friendId);
                string _friendStatus = await FirebaseLoginManager.Instance.IsFriendOnline(_friendId);
                FriendItem _friendItem = Instantiate(friendItem, friendList);
                _friendItem.SetFriendInfo(_friendId, _friendNickname, _friendStatus);
                _friendItem.SetUpButtons(matchUI, mainUI, friendUI, UpdatePartyMemberList);
                friendItemsList.Add(_friendItem);
            }
        }
    }

    async void PartyRequestAlarm(DataSnapshot _data)
    {
        DataSnapshot _ref = _data.Child(userId);

        if (_ref.HasChildren && _ref != null)
        {
            foreach (var _childrenSnapshot in _ref.Children)
            {
                if (_childrenSnapshot != null)
                {
                    string _requestStatus = _childrenSnapshot.Child("status").Value.ToString();
                    if (_requestStatus.Equals("accept"))
                    {
                        object _senderUserIdValue = _childrenSnapshot.Child("senderUserId")?.Value;
                        if (_senderUserIdValue != null)
                        {
                            string _friendId = _childrenSnapshot.Child("senderUserId").Value.ToString();
                            AcceptParty(userId, _friendId);

                            await _childrenSnapshot.Reference.RemoveValueAsync();
                        }
                    }
                    else if (_requestStatus.Equals("pending"))
                    {
                        object _senderUserIdValue = _childrenSnapshot.Child("senderUserId")?.Value;
                        if (_senderUserIdValue != null)
                        {
                            string _userId = _childrenSnapshot.Child("senderUserId").Value.ToString();
                            string _userNickName = await FirebaseLoginManager.Instance.ReadUserInfo(_userId);
                            PartyRequestItem _alarmItem = Instantiate(partyRequestAlarmItem, alarmSpace);
                            _alarmItem.SetAlarmInfo(_userId, _userNickName, "일반 대전");
                            _alarmItem.SetUpButtons(matchUI, mainUI, friendUI);
                            partyRequestAlarmItemList.Add(_alarmItem);
                        }
                    }
                }
            }
        }
    }

    public void AcceptParty(string _userId, string _friendId)
    {
        reference.Child("users").Child(_userId).Child("partyMemberList").Child(_friendId).SetValueAsync("party");
        reference.Child("users").Child(_friendId).Child("partyMemberList").Child(_userId).SetValueAsync("party");
    }

    async void UpdatePartyMemberList(string _userId)
    {
        ResetResults(memberList);

        string _userName = await FirebaseLoginManager.Instance.ReadUserInfo(_userId);
        MemberItem _mineItem = Instantiate(memberItem, memberSpace);
        _mineItem.SetMemberInfo(_userId, _userName);
        memberList.Add(_mineItem);

        List<string> _memberList = await FirebaseLoginManager.Instance.GetPartyMemberList(_userId);

        if (_memberList != null)
        {
            foreach (var _memberId in _memberList)
            {
                string _memberNickName = await FirebaseLoginManager.Instance.ReadUserInfo(_memberId);
                MemberItem _memberItem = Instantiate(memberItem, memberSpace);
                _memberItem.SetMemberInfo(_memberId, _memberNickName);
                memberList.Add(_memberItem);
            }
        }
    }

    async void SearchUser(string text)
    {
        nullResultText.gameObject.SetActive(false);

        ResetResults(resultsNickNameList);

        List<string> _newResults = await FirebaseLoginManager.Instance.SearchUserByName(text);

        if (_newResults != null)
        {
            foreach (var _userId in _newResults)
            {
                string _nickName = await FirebaseLoginManager.Instance.ReadUserInfo(_userId);
                SearchResultsItem _resultsUser = Instantiate(searchResultsItem, contentObjects);
                _resultsUser.SetItemInfo(_userId, _nickName);
                _resultsUser.SetUpButtons(matchUI, mainUI, friendUI, UpdatePartyMemberList);
                resultsNickNameList.Add(_resultsUser);
            }
        }
        else
        {
            nullResultText.gameObject.SetActive(true);
        }
    }

    public void ResetResults<T>(List<T> _list) where T : MonoBehaviour
    {
        foreach(var item in _list)
        {
            Destroy(item.gameObject);
        }

        _list.Clear();
    }

    public void OnClickUpdate()
    {
        UpdatePartyMemberList(userId);
    }
}
