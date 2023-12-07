using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoOption : MonoBehaviour
{
    public GameObject friendsPanel;
    public GameObject playPanel;
    public GameObject gameOffPanel;
    public GameObject characterPanel;
    public Button mainEscButton;
    public Button friendEscButton;
    public Button playEscButton;
    public Button characterEscButton;

    Button escButton;
    SettingManager settingManager;

    private void OnEnable()
    {
        LinkSettingManager();
    }

    void LinkSettingManager()
    {
        GameObject temp = GameObject.Find("SettingManager");

        if (temp == null)
        {
            Debug.LogError("SettingManager을 찾을 수 없습니다.");
            return;
        }

        settingManager = temp.GetComponent<SettingManager>();

        if (settingManager == null)
        {
            Debug.LogError("SettingManager의 Component을 찾을 수 없습니다.");
            return;
        }
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(friendsPanel.activeSelf)
            {
                OnClickEscButton(friendEscButton);
            }
            else if(playPanel.activeSelf)
            {
                OnClickEscButton(playEscButton);
            }
            else if(gameOffPanel.activeSelf)
            {
                gameOffPanel.SetActive(false);
            }
            else if (characterPanel.activeSelf)
            {
                OnClickEscButton(characterEscButton);
            }
            else
            {
                OnClickEscButton(mainEscButton);
            }
        }


    }

    public void OnClickEscButton(Button _btn)
    {
        escButton = _btn;

        if (escButton == mainEscButton)
        {
            if (settingManager.SettingPanel.activeSelf)
            {
                settingManager.SettingPanel.SetActive(false);
            }

            else
            {
                settingManager.SettingPanel.SetActive(true);
            }
        }

        else
        {
            escButton.onClick.Invoke();
        }
    }
}

