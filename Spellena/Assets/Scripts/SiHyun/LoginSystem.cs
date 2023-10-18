using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginSystem : MonoBehaviour
{
    public InputField email;
    public InputField passward;

    public InputField registerEmail;
    public InputField registerPassward;

    // Start is called before the first frame update
    void Start()
    {
        FirebaseLoginManager.Instance.Init();
    }

    // Update is called once per frame

    public void Register()
    {
        FirebaseLoginManager.Instance.Register(registerEmail.text, registerPassward.text);
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
