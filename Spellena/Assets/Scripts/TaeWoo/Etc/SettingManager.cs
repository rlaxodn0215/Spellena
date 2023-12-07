using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Player;

public class SettingManager : MonoBehaviour
{
    private static SettingManager instance;
    public static SettingManager Instance
    {
        get
        {
            return instance;
        }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }

    int previousWidth;   // 이전 화면 너비
    int previousHeight;  // 이전 화면 높이
    int previousNum;     // 이전 드롭박스 인덱스 번호
    int resolutionNum;

    public GameObject SettingPanel;
    public GameObject resolutionCheckPanel;
    public GameObject escButton;
    public GameObject backImage;

    public Dropdown resolutionDropdown;
    public Dropdown fullScreenDropdown;
    public Dropdown mouseOppositeDropdown;

    public Slider soundSlider;
    public Slider bgmSlider;
    public Slider effectSlider;
    public Slider voiceSlider;
    public Slider sensitivitySlider;

    public InputField soundInput;
    public InputField bgmInput;
    public InputField effectInput;
    public InputField voiceInput;
    public InputField sensitivityInput;

    FullScreenMode screenMode;
    List<Resolution> resolutions = new List<Resolution>();

    [HideInInspector]
    public float soundVal;
    [HideInInspector]
    public float bgmVal;
    [HideInInspector]
    public float effectVal;
    [HideInInspector]
    public float voiceVal;
    [HideInInspector]
    public float sensitivityVal;
    [HideInInspector]
    public bool isOpposite = false;

    private void Start()
    {
        InitUI();

        previousWidth = Screen.width;
        previousHeight = Screen.height;
        previousNum = resolutionDropdown.value;
        resolutionCheckPanel.SetActive(false);

        SettingPanel.SetActive(false);
        UpdateSliderValueToText();

        Screen.SetResolution(1336, 768, FullScreenMode.Windowed);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name != "SiHyun MainLobby Test" &&
                SceneManager.GetActiveScene().name != "SiHyun RoomLobby Test")
            {
                if (SettingPanel.activeSelf)
                {
                    SettingPanel.SetActive(false);
                    ActivatePlayerInput(true);
                }

                else
                {
                    SettingPanel.SetActive(true);
                    ActivatePlayerInput(false);
                    UpdateSliderValueToText();
                    backImage.SetActive(false);
                    escButton.SetActive(false);
                }
            }
          
        }

        if (SettingPanel.activeSelf)
            UpdateSliderValueToText();
    }

    void InitUI()
    {
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

    void ActivatePlayerInput(bool isActive)
    {
        Character[] characters = GameObject.FindObjectsOfType<Character>();

        foreach(Character character in characters)
        {
            character.GetComponent<PlayerInput>().enabled = isActive;
        }
    }

    public void FullScreenBtn()
    {
        int _x = fullScreenDropdown.value;
        if (_x == 0)
        {
            screenMode = FullScreenMode.FullScreenWindow;
        }
        else if (_x == 1)
        {
            screenMode = FullScreenMode.Windowed;
        }
        else if (_x == 2)
        {
            screenMode = FullScreenMode.ExclusiveFullScreen;
        }
        Screen.SetResolution(resolutions[resolutionNum].width,
                             resolutions[resolutionNum].height,
                             screenMode);
    }

    public void DropboxOptionChange(int x)
    {
        resolutionNum = x;

        Screen.SetResolution(resolutions[resolutionNum].width,
                             resolutions[resolutionNum].height,
                             screenMode);

        resolutionCheckPanel.SetActive(true);
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

    public void UpdateSliderValueToText()
    {
        soundVal = soundSlider.value;
        bgmVal = bgmSlider.value;
        effectVal = effectSlider.value;
        voiceVal = voiceSlider.value;
        sensitivityVal = sensitivitySlider.value;

        soundInput.text = string.Format("{0}", soundVal * 100);
        bgmInput.text = string.Format("{0}", bgmVal * 100);
        effectInput.text = string.Format("{0}", effectVal * 100);
        voiceInput.text = string.Format("{0}", voiceVal * 100);

        sensitivityInput.text = string.Format("{0}", sensitivityVal * 20);
    }

    public void CheckMouseOpposite()
    {
        int x = mouseOppositeDropdown.value;

        if (x == 0)
        {
            isOpposite = false;
        }
        else if (x == 1)
        {
            isOpposite = true;
        }
    }


}
