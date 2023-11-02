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
                /*if (currentUser != null)
                {
                    Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
                    {
                        DisplayName = nickname,
                        PhotoUrl = new System.Uri("https://example.com/jane-q-user/profile.jpg"),
                    };
                    currentUser.UpdateUserProfileAsync(profile).ContinueWith(task =>
                    {
                        if (task.IsCanceled)
                        {
                            Debug.LogError("UpdateUserProfileAsync was canceled.");
                            return;
                        }
                        if (task.IsFaulted)
                        {
                            Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                            return;
                        }

                        Debug.Log("User profile updated successfully.");
                    });
                }*/

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
            return _snapShot.Child("userName").Value.ToString();
        }
        return null;
    }

    public void SetUserStatus(string _userId, string _status)
    {
        reference.Child("users").Child(_userId).Child("status").SetValueAsync(_status);
    }



    public void SearchUserByName(string _userName)
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
    }

}
