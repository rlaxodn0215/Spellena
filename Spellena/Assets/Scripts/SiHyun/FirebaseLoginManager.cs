using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Database;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;

public class FirebaseLoginManager
{
    private static FirebaseLoginManager instance = null;

    private string nickname;
    private string userName;
    private string birthDate;
    private string phoneNumber;

    private FirebaseAuth auth;
    private FirebaseUser user;
    DatabaseReference reference;
    private Dictionary<string, string> userIdMapping = new Dictionary<string, string>();

    public class User
    {
        public string nickName;
        public string email;
        public string passward;
        public string userName;
        public string birthDate;
        public string phoneNumber;
        public string status;

        public User(string _nickName, string _email, string _passward,
                    string _userName, string _birthDate, string _phoneNumber, string _status)
        {
            this.nickName = _nickName;
            this.email = _email;
            this.passward = _passward;
            this.userName = _userName;
            this.birthDate = _birthDate;
            this.phoneNumber = _phoneNumber;
            this.status = _status;
        }
    }

    [System.Serializable]
    public class FriendRequestData
    {
        public string senderUserId;
        public string status;

        public FriendRequestData(string _senderUserId, string _status)
        {
            this.senderUserId = _senderUserId;
            this.status = _status;
        }
    }

    [System.Serializable]
    public class PartyRequestData
    {
        public string senderUserId;
        public string status;

        public PartyRequestData(string _senderUserId, string _status)
        {
            this.senderUserId = _senderUserId;
            this.status = _status;
        }
    }

    public static FirebaseLoginManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FirebaseLoginManager();
            }
            return instance;
        }
    }

    public void Init()
    {
        auth = FirebaseAuth.DefaultInstance;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        if (auth.CurrentUser != null)
        {
            SignOut();
        }
        auth.StateChanged += OnChanged;
    }

    public DatabaseReference GetReference()
    {
        return reference;
    }

    private void OnChanged(object sender, EventArgs e)
    {
        if (auth.CurrentUser != user)
        {
            bool signed = (auth.CurrentUser != user && auth.CurrentUser != null);
            if (!signed && user != null)
            {
                Debug.Log("로그아웃");
            }
            if (signed)
            {
                Debug.Log("로그인");
                Firebase.Auth.FirebaseUser currentUser = auth.CurrentUser;
                SceneManager.LoadScene("SiHyun MainLobby Test");

            }
        }
    }

    public void Register(string email, string passward)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, passward).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: "
                    + task.Exception);
                return;
            }
            else
            {
                //Firebase user has been created.
                Firebase.Auth.AuthResult result = task.Result;
                SaveUserInfo(result.User.UserId, nickname, result.User.Email, passward, userName,
                             birthDate, phoneNumber);
                Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            }
        });

    }

    public void SignIn(string email, string passward, GameObject panel)
    {
        auth.SignInWithEmailAndPasswordAsync(email, passward).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                //Debug.LogWarning("SignInWithEmailAndPasswordAsync was canceled.");
                panel.SetActive(true);
                return;
            }
            else if (task.IsFaulted)
            {
                //Debug.LogWarning("SignInWithEmailAndPasswordAsync encountered an error: "
                   // + task.Exception);
                panel.SetActive(true);
                return;
            }
            else
            {
                Firebase.Auth.AuthResult result = task.Result;
                SetUserStatus(result.User.UserId, "온라인");
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            }
        });
    }

    public void SignOut()
    {
        user = auth.CurrentUser;
        SetUserStatus(user.UserId, "오프라인");
        SetLobbyMaster(user.UserId, false);
        PartyListReset(user.UserId);
        //RemovePartyMember(user.UserId);
        auth.SignOut();
    }

    public FirebaseUser GetUser()
    {
        user = auth.CurrentUser;
        return user;
    }
    public void SaveUserInfo(string _uID, string _nickName, string _email, string _passward,
                         string _userName, string _birthDate, string _phoneNumber)
    {
        User _user = new User(_nickName, _email, _passward, _userName, _birthDate, _phoneNumber, "온라인");
        string _json = JsonUtility.ToJson(_user);
        reference.Child("users").Child(_uID).SetRawJsonValueAsync(_json);
    }

    public async Task<string> ReadUserInfo(string _uID)
    {
        DatabaseReference _userReference = reference.Child("users").Child(_uID);
        DataSnapshot _snapShot = await _userReference.GetValueAsync();

        if (_snapShot != null)
        {
            string _nickName = _snapShot.Child("nickName").Value.ToString();
            return _nickName;
        }
        return null;
    }

    public void SetPhotonId(string _userId, string _photonId)
    {
        reference.Child("users").Child(_userId).Child("photonId").SetValueAsync(_photonId);
    }

    public async Task<string> FindUserEmail(string _userName, string _phoneNumber)
    {
        DatabaseReference _userRef = reference.Child("users");

        var _userNameQuery = _userRef.OrderByChild("userName").EqualTo(_userName);

        DataSnapshot _userNameSnapshot = await _userNameQuery.GetValueAsync();

        if(_userNameSnapshot.HasChildren)
        {
            foreach(var _childSnapshot in _userNameSnapshot.Children)
            {
                var _userPhonNumber = _childSnapshot.Child("phoneNumber").Value.ToString();
                if(_userPhonNumber == _phoneNumber)
                {
                    string _userEmail = _childSnapshot.Child("email").Value.ToString();
                    return _userEmail;
                }
            }
        }

        return "회원 정보가 없습니다.";
    }

    public async Task<string> FindUserPassward(string _email,     string _userName,
                                               string _birthDate, string _phoneNumber)
    {
        DatabaseReference _userRef = reference.Child("users");

        var _userEmailQuery = _userRef.OrderByChild("email").EqualTo(_email);

        DataSnapshot _userEmailSnapshot = await _userEmailQuery.GetValueAsync();

        if (_userEmailSnapshot.HasChildren)
        {
            foreach(var _childrenSnapshot in _userEmailSnapshot.Children)
            {
                var _userNameVal = _childrenSnapshot.Child("userName").Value.ToString();
                var _birthDateVal = _childrenSnapshot.Child("birthDate").Value.ToString();
                var _phoneNumberVal = _childrenSnapshot.Child("phoneNumber").Value.ToString();

                if(_userNameVal == _userName && _birthDateVal == _birthDate && _phoneNumberVal == _phoneNumber)
                {
                    string _passWard = _childrenSnapshot.Child("passward").Value.ToString();
                    return _passWard;
                }
            }
        }

        return "회원 정보가 없습니다.";
    }


    public async Task<List<string>> SearchUserByName(string _nickName)
    {
        List<string> _resultList = new List<string>();

        DatabaseReference _userRef = reference.Child("users");
        
        var _userNickNameQuery = _userRef.OrderByChild("nickName").EqualTo(_nickName);

        DataSnapshot _userNickNameSnapshot = await _userNickNameQuery.GetValueAsync();

        if(_userNickNameSnapshot.HasChildren)
        {
            foreach(var _childrenSnapshot in _userNickNameSnapshot.Children)
            {
                string userId = _childrenSnapshot.Key;
                _resultList.Add(userId);
            }
            return _resultList;
        }
        return null;
    }

    public void SendFriendRequest(string _senderUserId, string _recevierUserId)
    {
        FriendRequestData _user = new FriendRequestData(_senderUserId, "pending"); 
        string _json = JsonUtility.ToJson(_user);
        reference.Child("friendRequests").Child(_recevierUserId).Child("requestsUser:" + _senderUserId).SetRawJsonValueAsync(_json);
    }

    public async Task<List<string>> GetFriendsList(string _userId)
    {
        List<string> _requestsList = new List<string>();

        DatabaseReference _requestsRef = reference.Child("users").Child(_userId).Child("friendList");

        var _requestsAlarm = _requestsRef.OrderByValue().EqualTo("friend");

        DataSnapshot _beforeProcessingSnapshot= await _requestsAlarm.GetValueAsync();

        if(_beforeProcessingSnapshot.HasChildren)
        {
            foreach(var _childrenSnapshot in _beforeProcessingSnapshot.Children)
            {
                string _friendId = _childrenSnapshot.Key;
                _requestsList.Add(_friendId);
            }
            return _requestsList;
        }

        return null;
    }

    public async Task<string> IsFriendOnline(string _friendId)
    {
        DatabaseReference _friendRef = reference.Child("users").Child(_friendId);
        DataSnapshot _friendSnapshot = await _friendRef.GetValueAsync();

        if(_friendSnapshot != null)
        {
            string _status = _friendSnapshot.Child("status").Value.ToString();
            return _status;
        }

        return null;
    }

    public void SendPartyRequest(string _senderUserId, string _recevierUserId)
    {
        PartyRequestData _user = new PartyRequestData(_senderUserId, "pending");
        string _json = JsonUtility.ToJson(_user);
        reference.Child("partyRequests").Child(_recevierUserId).Child("requestsUser:" + _senderUserId).SetRawJsonValueAsync(_json);
    }

    public async Task<List<string>> GetPartyMemberList(string _userId)
    {
        List<string> _requestsList = new List<string>();

        DatabaseReference _requestsRef = reference.Child("users").Child(_userId).Child("partyMemberList");

        var _requestsAlarm = _requestsRef.OrderByValue().EqualTo("party");

        DataSnapshot _beforeProcessingSnapshot = await _requestsAlarm.GetValueAsync();

        if (_beforeProcessingSnapshot.HasChildren)
        {
            foreach (var _childrenSnapshot in _beforeProcessingSnapshot.Children)
            {
                string _friendId = _childrenSnapshot.Key;
                _requestsList.Add(_friendId);
            }
            return _requestsList;
        }

        return null;
    }

    public void PartyListReset(string _userId)
    {
        Dictionary<string, string> emptyPartyMemberList = new Dictionary<string, string>();
        reference.Child("users").Child(_userId).Child("partyMemberList").SetValueAsync(emptyPartyMemberList);
    }

    public void RemovePartyMember(string _userId)
    {
        try
        {
            DatabaseReference _removeMemberRef = reference.Child("users");

            var ad = _removeMemberRef.GetValueAsync();

            DataSnapshot _userSnapshot = ad.Result;

            foreach (var _userChild in _userSnapshot.Children)
            {
                DataSnapshot _partyMemberSnapshot = _userChild.Child("partyMemberList");

                if (_partyMemberSnapshot.Exists)
                {
                    Dictionary<string, string> partyMemberList =
                        _partyMemberSnapshot.Value as Dictionary<string, string>;

                    if (partyMemberList != null && partyMemberList.ContainsKey(_userId))
                    {
                        // Remove the member from the partyMemberList
                        partyMemberList.Remove(_userId);

                        // Check if partyMemberList is empty and update it
                        if (partyMemberList.Count == 0)
                        {
                            _userChild.Reference.Child("partyMemberList").RemoveValueAsync();
                        }
                        else
                        {
                            _userChild.Reference.Child("partyMemberList").SetValueAsync(partyMemberList);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    public void SetLobbyMaster(string _userId, bool _type)
    {
        reference.Child("users").Child(_userId).Child("isLobbyMaster?").SetValueAsync(_type);
    }

    public async Task<bool> IsLobbyMasterAsync(string userId)
    {
        var dataSnapshot = await reference.Child("users").Child(userId).Child("isLobbyMaster").GetValueAsync().ConfigureAwait(false);

        return dataSnapshot.Exists && dataSnapshot.Value is bool isLobbyMaster && isLobbyMaster;
    }

    public void SetFriendRequestStatus(string _senderUserId, string _recevierUserId, string _status)
    {
        reference.Child("friendRequests").Child(_recevierUserId).Child("requestsUser:" + _senderUserId).Child("status").SetValueAsync(_status);
    }

    public void SetPartyRequestStatus(string _senderUserId, string _recevierUserId, string _status)
    {
        reference.Child("partyRequests").Child(_recevierUserId).Child("requestsUser:" + _senderUserId).Child("status").SetValueAsync(_status);
    }

    public void SetUserStatus(string _userId, string _status)
    {
        reference.Child("users").Child(_userId).Child("status").SetValueAsync(_status);
    }

    public void SetNickname(string _nickName)
    {
        nickname = _nickName;
    }

    public void SetUserName(string _userName)
    {
        userName = _userName;
    }

    public void SetBirthDate(string _birthDate)
    {
        birthDate = _birthDate;
    }
    public void SetPhoneNumber(string _phoneNumber)
    {
        phoneNumber = _phoneNumber;
    }
}
