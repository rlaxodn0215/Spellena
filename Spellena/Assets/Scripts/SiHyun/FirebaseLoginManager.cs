using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System;

public class FirebaseLoginManager
{
    private static FirebaseLoginManager instance = null;

    private string nickname;

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
    private FirebaseAuth auth;
    private FirebaseUser user;

    public void Init()
    {
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
            user = auth.CurrentUser;
            if(signed)
            {
                Firebase.Auth.FirebaseUser currentUser = auth.CurrentUser;
                Debug.Log("로그인");
                if(currentUser.DisplayName != null)
                {
                    SceneManager.LoadScene("SiHyun MainLobby Test");
                }
                else
                {
                    if (currentUser != null)
                    {
                        Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
                        {
                            DisplayName = nickname,
                            PhotoUrl = new System.Uri("https://example.com/jane-q-user/profile.jpg"),
                        };
                        currentUser.UpdateUserProfileAsync(profile).ContinueWith(task => {
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
                    }
                }
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
        return user;
    }

    public void SetNickname(string s)
    {
        nickname = s;
    }
}
