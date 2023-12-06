using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoOption : MonoBehaviour
{
    public GameObject friendsPanel;
    public GameObject playPanel;
    public GameObject gameOffPanel;
    public Button mainEscButton;
    public Button friendEscButton;
    public Button playEscButton;
    Button escButton;

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
            else 
            {
                OnClickEscButton(mainEscButton);
            }
        }


    }

    public void OnClickEscButton(Button _btn)
    {
        escButton = _btn;
        escButton.onClick.Invoke();
    }
}

