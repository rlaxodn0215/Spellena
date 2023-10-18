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
                Debug.Log("로그인");
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

}
