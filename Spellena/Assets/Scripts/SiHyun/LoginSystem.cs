using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginSystem : MonoBehaviour
{
    public InputField email;
    public InputField passward;

    // Start is called before the first frame update
    void Start()
    {
        FirebaseLoginManager.Instance.Init();
    }

    // Update is called once per frame

    public void Register()
    {
        string e = email.text;
        string p = passward.text;

        FirebaseLoginManager.Instance.Register(e, p);
    }

    public void SignIn()
    {
        FirebaseLoginManager.Instance.SignIn(email.text, passward.text);
    }

    public void SignOut()
    {
        FirebaseLoginManager.Instance.SignOut();
    }
}
