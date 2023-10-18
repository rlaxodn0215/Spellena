using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ChangeInputField : MonoBehaviour
{
    EventSystem system;
    public Selectable firstInput;
    public Button submitButton;

    // Start is called before the first frame update
    void Start()
    {
        system = EventSystem.current;
        firstInput.Select();
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
            Debug.Log(submitButton.name);
            submitButton.onClick.Invoke();
        }
    }

    public void LoadRegisterScene()
    {
        SceneManager.LoadScene("SiHyun Register Test");
    }
}
