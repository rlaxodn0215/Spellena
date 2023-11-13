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

    private void Start()
    {
        nullResultText.gameObject.SetActive(false);
        if (searchInputField != null)
            searchInputField.onEndEdit.AddListener(SearchUser);
    }

    async void SearchUser(string text)
    {
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
