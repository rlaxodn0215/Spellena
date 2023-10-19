using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ChangeInputField : MonoBehaviour
{
    EventSystem system;
    public Selectable firstInputLogin;
    public Selectable firstInputRegister;
    public Selectable firstInputNickname;
    public Button submitLoginButton;
    public Button submitRegisterButton;
    public Button submitOkButton;
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject authPanel;
    Button _submitButton = null;

    public InputField nickNameField;

    // Start is called before the first frame update
    void Start()
    {
        system = EventSystem.current;
        firstInputLogin.Select();
        _submitButton = submitLoginButton;
        registerPanel.SetActive(false);
        authPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
        {
            Selectable next =
                system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
            if (next != null)
            {
                next.Select();
            }
        }
        else if(Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next =
                system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null)
            {
                next.Select();
            }
        }
        else if(Input.GetKeyDown(KeyCode.Return))
        {
            _submitButton.onClick.Invoke();
        }
    }
    
    public void SigninPanelActvie()
    {
        loginPanel.SetActive(true);
        authPanel.SetActive(false);
        _submitButton = submitLoginButton;
        firstInputLogin.Select();

    }

    public void RegisterPanelActive()
    {
        registerPanel.SetActive(true);
        loginPanel.SetActive(false);
        _submitButton = submitRegisterButton;
        firstInputRegister.Select();
    }
    
    public void AuthPanelActive()
    {
        authPanel.SetActive(true);
        registerPanel.SetActive(false);
        _submitButton = submitOkButton;
        firstInputNickname.Select();
    }


}
