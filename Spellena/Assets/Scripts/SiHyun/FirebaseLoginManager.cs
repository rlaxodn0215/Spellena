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
        public User(string _userName, string _email)
        {
            this.userName = _userName;
            this.email = _email;
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
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            }
        });
    }

    public void SignOut()
    {
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
        User _user = new User(_userName, _email);
        string _json = JsonUtility.ToJson(_user);
        reference.Child(_uID).SetRawJsonValueAsync(_json);
    }

    public async Task<string> ReadUserInfo(string _uID)
    {
        DatabaseReference _userReference = reference.Child(_uID);
        DataSnapshot _snapShot = await _userReference.GetValueAsync();

        if(_snapShot != null)
        {
            return _snapShot.Child("userName").Value.ToString();
        }
        return null;
    }

}
