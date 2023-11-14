using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    
    private void Start()
    {
        nullResultText.gameObject.SetActive(false);
        if (searchInputField != null)
        {
            searchInputField.onEndEdit.AddListener(SearchUser);
        }

        GetRequestsAlarm();

    }

    private void Update()
    {
    }

    async void GetRequestsAlarm()
    {
        foreach (var _item in alarmItemList)
        {
            Destroy(_item.gameObject);
        }
        alarmItemList.Clear();

        List<string> _alarmList = await FirebaseLoginManager.Instance.GetFriendRequests(FirebaseLoginManager.Instance.GetUser().UserId);
        
        foreach (string _alarm in _alarmList)
        {
            string _senderName = await FirebaseLoginManager.Instance.ReadUserInfo(_alarm);
            AlarmItem _alarmItem = Instantiate(alarmItem, alarmSpace);
            _alarmItem.SetAlarmInfo(_alarm, _senderName);
            alarmItemList.Add(_alarmItem);
            Debug.Log(_alarm + " : " + _senderName);
        }
    }

    async void SearchUser(string text)
    {
        nullResultText.gameObject.SetActive(false);

        ResetResults();

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

    public void ResetResults()
    {
        foreach(var item in resultsNickNameList)
        {
            Destroy(item.gameObject);
        }

        resultsNickNameList.Clear();
    }


}
