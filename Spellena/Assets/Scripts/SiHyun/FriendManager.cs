using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Firebase.Database;

public class FriendManager : MonoBehaviour
{
    List<SearchResultsItem> resultsNickNameList = new List<SearchResultsItem>();
    public SearchResultsItem searchResultsItem;
    public Transform contentObjects;
    public InputField searchInputField;
    public Text nullResultText;
    List<AlarmItem> alarmItemList = new List<AlarmItem>();
    public AlarmItem alarmItem;
    public Transform alarmSpace;
    List<FriendItem> friendItemsList = new List<FriendItem>();
    public FriendItem friendItem;
    public Transform friendList;

    string userId;

    DatabaseReference reference;
    private void Start()
    {
        userId = FirebaseLoginManager.Instance.GetUser().UserId;
        //FirebaseLoginManager.Instance.AddFriend(userId);

        DatabaseReference dataRef = FirebaseLoginManager.Instance.GetReference().Child("friendRequests");
        reference = FirebaseLoginManager.Instance.GetReference();

        dataRef.ValueChanged += (sender, args) =>
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
                    AddItems(args.Snapshot);
                }
            }
            catch(Exception ex)
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
        reference.Child("users").Child(_userId).Child("friendList").Child(_friendId).Child("status").SetValueAsync(true);
        reference.Child("users").Child(_friendId).Child("friendList").Child(_userId).Child("status").SetValueAsync(true);
    }

    async void UpdateFriendList()
    {
        if(friendItemsList != null)
        {
            foreach(var item in friendItemsList)
            {
                Destroy(item.gameObject);
            }
            friendItemsList.Clear();
        }
        
        DatabaseReference _friendListRef = reference.Child("users").Child(userId).Child("friendList");

        var _friends = _friendListRef.OrderByChild("status").EqualTo(true);

        DataSnapshot _friendsSnapshot = await _friends.GetValueAsync();

        if(_friendsSnapshot.HasChildren && _friendsSnapshot != null)
        {
            foreach(var _friend in _friendsSnapshot.Children)
            {
                string _friendId = _friend.ToString();
                string _friendNickName = await FirebaseLoginManager.Instance.ReadUserInfo(_friendId);
                FriendItem _friendItem = Instantiate(friendItem, friendList);
                _friendItem.SetFriendInfo(_friendId, _friendNickName);
                friendItemsList.Add(_friendItem);
            }
        }

    }

    async void AddItems(DataSnapshot _data)
    {
        DataSnapshot _ref = _data.Child(userId);

        if(_ref.HasChildren && _ref != null)
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
                            string _friendNickName = await FirebaseLoginManager.Instance.ReadUserInfo(_friendId);
                            AcceptFriend(userId, _friendId);
                            UpdateFriendList();
                        }
                    }
                    else if (_requestStatus.Equals("pending"))
                    {
                        object _senderUserIdValue = _childrenSnapshot.Child("senderUserId")?.Value;
                        if (_senderUserIdValue != null)
                        {
                            string _userId = _childrenSnapshot.Child("senderUserId").Value.ToString();
                            string _userNickName = await FirebaseLoginManager.Instance.ReadUserInfo(_userId);
                            AlarmItem _alarmItem = Instantiate(alarmItem, alarmSpace);
                            _alarmItem.SetAlarmInfo(_userId, _userNickName, "模备 夸没");
                            alarmItemList.Add(_alarmItem);
                            Debug.Log(_userId + " : " + _userNickName);
                        }
                    }
                }
            }
        }
    }

    async void AddAlarm(DataSnapshot _data)
    {
        DataSnapshot _ref = _data.Child(userId);

        if (_ref.HasChildren && _ref != null)
        {
            foreach (var _childrenSnapshot in _ref.Children)
            {
                if (_childrenSnapshot != null)
                {
                    string _requestStatus = _childrenSnapshot.Child("status").Value.ToString();
                    if (_requestStatus.Equals("pending"))
                    {
                        object _senderUserIdValue = _childrenSnapshot.Child("senderUserId")?.Value;
                        if (_senderUserIdValue != null)
                        {
                            string _userId = _childrenSnapshot.Child("senderUserId").Value.ToString();
                            string _userNicknName = await FirebaseLoginManager.Instance.ReadUserInfo(_userId);
                            AlarmItem _alarmItem = Instantiate(alarmItem, alarmSpace);
                            _alarmItem.SetAlarmInfo(_userId, _userNicknName, "模备 夸没");
                            alarmItemList.Add(_alarmItem);
                            Debug.Log(_userId + " : " + _userNicknName);
                        }
                    }
                }
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
                resultsNickNameList.Add(_resultsUser);
            }
        }
        else
        {
            nullResultText.gameObject.SetActive(true);
        }
    }

    public void ResetResults(List<SearchResultsItem> _list)
    {
        foreach(var item in _list)
        {
            Destroy(item.gameObject);
        }

        _list.Clear();
    }

    public void ResetResults(List<AlarmItem> _list)
    {
        foreach (var item in _list)
        {
            Destroy(item.gameObject);
        }

        _list.Clear();
    }

}
