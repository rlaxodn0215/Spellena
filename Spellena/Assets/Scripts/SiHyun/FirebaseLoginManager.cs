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
    private FirebaseAuth auth;
    private FirebaseUser user;
    DatabaseReference reference;
    private Dictionary<string, string> userIdMapping = new Dictionary<string, string>();


    public class User
    {
        public string userName;
        public string email;
        public string status;
        public User(string _userName, string _email, string _status)
        {
            this.userName = _userName;
            this.email = _email;
            this.status = _status;
        }
    }

    public static FirebaseLoginManager Instance
    {
        get
        {
            if(instance == null)
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
        if(auth.CurrentUser != null)
        {
            SignOut();
        }
        auth.StateChanged += OnChanged;
    }

    private void OnChanged(object sender, EventArgs e)
    {
        if(auth.CurrentUser != user)
        {
            bool signed = (auth.CurrentUser != user && auth.CurrentUser != null);
            if(!signed && user != null)
            {
                Debug.Log("로그아웃");
            }
            if(signed)
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
                SaveUserInfo(result.User.UserId, nickname, result.User.Email);
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

    public void SetNickname(string s)
    {
        nickname = s;
    }

    public void SaveUserInfo(string _uID, string _userName, string _email)
    {
        User _user = new User(_userName, _email, "온라인");
        string _json = JsonUtility.ToJson(_user);
        reference.Child("users").Child(_uID).SetRawJsonValueAsync(_json);
    }

    public async Task<string> ReadUserInfo(string _uID)
    {
        DatabaseReference _userReference = reference.Child("users").Child(_uID);
        DataSnapshot _snapShot = await _userReference.GetValueAsync();

        if(_snapShot != null)
        {
            string _userName = _snapShot.Child("userName").Value.ToString();
            return _userName;
        }
        return null;
    }

    public void SetUserStatus(string _userId, string _status)
    {
        reference.Child("users").Child(_userId).Child("status").SetValueAsync(_status);
    }

    /*public void SearchUserByName(string _userName)
    {
        List<string> _resultList = new List<string>();

        reference.Child("users").OrderByChild("userName").EqualTo(_userName).GetValueAsync().ContinueWith(task =>
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

    public async Task<string> GetUserMapping(string _firebaseUserId)
    {
        if(userIdMapping.ContainsKey(_firebaseUserId))
        {
            return userIdMapping[_firebaseUserId];
        }
        else
        {
            //Firebase 사용자 아이디를 Photon Realtime Player의 아이디로 매핑
            string _photonUserId = await MapFirebaseUserIdToPhotonUserId(_firebaseUserId);
            if (!string.IsNullOrEmpty(_photonUserId))
            {
                userIdMapping[_firebaseUserId] = _photonUserId;
                return _photonUserId;
            }
        }
        return null;
    }
    private async Task<string> MapFirebaseUserIdToPhotonUserId(string _firebaseUserId)
    {
        // 이 함수는 Firebase 사용자 UID를 Photon UserId로 매핑하는 로직을 구현해야 합니다.
        // 아래는 예제일 뿐이며, 실제 매핑 방법은 프로젝트의 구조와 요구 사항에 따라 다를 수 있습니다.

        string photonUserId = null;

        // Firebase 사용자 UID를 기반으로 Photon UserId를 가져오는 방법 예제
        // 이 예제는 간단하게 Firebase UID를 Photon UserId로 사용하는 것입니다.
        photonUserId = _firebaseUserId;

        return photonUserId;
    }

}
