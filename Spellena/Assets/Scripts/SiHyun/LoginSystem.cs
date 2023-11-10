using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginSystem : MonoBehaviour
{
    // 로그인
    public InputField email;
    public InputField passward;

    // 회원 가입
    private bool readyToRegister = false;
    public InputField registerEmail;
    public InputField registerPassward;
    public InputField registerPasswardCheck;
    public Text passwardOkText;
    public InputField userName;
    public InputField birthDate;
    public InputField phoneNumber;
    public InputField nickNameField;

    // 아이디 찾기
    public InputField userNameInput;
    public InputField phoneNumberInput;
    public Text resultEmailText;

    // Start is called before the first frame update
    void Start()
    {
        FirebaseLoginManager.Instance.Init();
    }

    // Update is called once per frame
    private void Update()
    {
        if (string.IsNullOrEmpty(registerPasswardCheck.text))
        {
            passwardOkText.text = "";
            readyToRegister = false;
        }
        else if (registerPassward.text == registerPasswardCheck.text)
        {
            passwardOkText.text = "확인되었습니다.";
            readyToRegister = true;
        }
        else
        {
            passwardOkText.text = "비밀번호가 일치하지 않습니다.";
            readyToRegister = false;
        }
    }

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

    public void OkBtnClick()
    {
        if (readyToRegister)
        {
            FirebaseLoginManager.Instance.SetNickname(nickNameField.text);
            FirebaseLoginManager.Instance.SetUserName(userName.text);
            FirebaseLoginManager.Instance.SetBirthDate(birthDate.text);
            FirebaseLoginManager.Instance.SetPhoneNumber(phoneNumber.text);
            Register();
        }
    }

    /*public void OnClickFindID()
    {
        resultEmailText.text = await FirebaseLoginManager.Instance.FindUserEmail(userNameInput.text, phoneNumberInput.text);
    }*/
}
