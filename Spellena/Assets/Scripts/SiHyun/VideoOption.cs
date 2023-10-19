using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoOption : MonoBehaviour
{
    int previousWidth;   // 이전 화면 너비
    int previousHeight;  // 이전 화면 높이
    int previousNum;     // 이전 드롭박스 인덱스 번호
    public GameObject resolutionCheckPanel;
    public GameObject settingPanel;
    FullScreenMode screenMode;
    public Dropdown resolutionDropdown;
    public Dropdown fullScreenDropdown;
    List<Resolution> resolutions = new List<Resolution>();
    int resolutionNum;
    // Start is called before the first frame update
    void Start()
    {
        InitUI();
        previousWidth = Screen.width;
        previousHeight = Screen.height;
        previousNum = resolutionDropdown.value;
        resolutionCheckPanel.SetActive(false);
        settingPanel.SetActive(false);

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingPanel.activeSelf)
            {
                settingPanel.SetActive(false);
            }
            else
            {
                settingPanel.SetActive(true);
            }
        }
    }
    void InitUI()
    {
        // 60프레인 기준 해상도만 리스트에 추가 
        /*for(int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (Screen.resolutions[i].refreshRate == 60)
                resolutions.Add(Screen.resolutions[i]);
        }*/
        // 설정 가능한 모든 해상도 추가
        resolutions.AddRange(Screen.resolutions);
        resolutionDropdown.options.Clear();

        int optionNum = 0;
        foreach (Resolution item in resolutions)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = item.width + " x " + item.height + " (" + item.refreshRate + "hz)";
            resolutionDropdown.options.Add(option);

            if (item.width == Screen.width && item.height == Screen.height)
                resolutionDropdown.value = optionNum;
            optionNum++;
        }
        resolutionDropdown.RefreshShownValue();
    }
    public void DropboxOptionChange(int x)
    {
        resolutionNum = x;

        Screen.SetResolution(resolutions[resolutionNum].width,
                             resolutions[resolutionNum].height,
                             screenMode);

        resolutionCheckPanel.SetActive(true);
    }

    public void FullScreenBtn(int x)
    {
        if(x == 0)
        {
            screenMode = FullScreenMode.FullScreenWindow;
        }
        else if(x == 1)
        {
            screenMode = FullScreenMode.Windowed;
        }
        else if(x == 2)
        {
            screenMode = FullScreenMode.ExclusiveFullScreen;
        }
        Screen.SetResolution(resolutions[resolutionNum].width,
                             resolutions[resolutionNum].height,
                             screenMode);
    }

    public void onClickOkBtn()
    {
        previousWidth = resolutions[resolutionNum].width;
        previousHeight = resolutions[resolutionNum].height;
        previousNum = resolutionNum;

        resolutionCheckPanel.SetActive(false);
    }

    public void onClickCancelBtn()
    {
        Screen.SetResolution(previousWidth,
                             previousHeight,
                             screenMode);

        resolutionDropdown.value = previousNum;

        resolutionCheckPanel.SetActive(false);
    }
}
