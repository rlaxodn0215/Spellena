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
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
        {
            SignOut();
        }
        auth.StateChanged += OnChanged;
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

    public void SignIn(string email, string passward)
    {
        auth.SignInWithEmailAndPasswordAsync(email, passward).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: "
                    + task.Exception);
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

        return null;
    }

    public void SetUserStatus(string _userId, string _status)
    {
        reference.Child("users").Child(_userId).Child("status").SetValueAsync(_status);
    }

    /*public void SearchUserByName(string _nickName)
    {
        List<string> _resultList = new List<string>();

        reference.Child("users").OrderByChild("nickName").EqualTo(_nickName).GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot _snapShot = task.Result;
            if (_snapShot.HasChildren)
            {
                foreach (var _childSnapshot in _snapShot.Children)
                {
                    string _userId = _childSnapshot.Key;
                    _resultList.Add(_userId);
                }
            }
        });
    }

    public void AddFriend(string _userId, string _friendId)
    {
        reference.Child("users").Child(_userId).Child("friends").Child("friendId").SetValueAsync(_friendId);
    }

    public void FriendList(string _userId)
    {
        reference.Child("users").Child(_userId).Child("friends").GetValueAsync();
    }*/


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
