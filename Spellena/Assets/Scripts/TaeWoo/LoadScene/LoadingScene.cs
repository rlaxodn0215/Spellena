using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class LoadingScene : MonoBehaviourPunCallbacks
{
    public GameObject mapName;
    public GameObject helpBackImage;
    public GameObject helpText;
    public GameObject helpWords;
    public GameObject loadingSign;

    public string name;
    public string words;

    private Text mapNameText;
    private Text helpWordsText;

    private Image helpBackImageComponent;
    private RectTransform loadingSignRectTransform;

    private float timer = 0.0f;
    private float helpBackImageOpenSpeed = 1.0f;
    private float loadingSignRotateSpeed = 3.5f;
    private float loadingSignRotateFrequency = 0.5f;

    void Start()
    {
        mapNameText = mapName.GetComponent<Text>();
        helpBackImageComponent = helpBackImage.GetComponent<Image>();
        helpWordsText = helpWords.GetComponent<Text>();
        loadingSignRectTransform = loadingSign.GetComponent<RectTransform>();

        mapNameText.text = name;
        helpWordsText.text = words;

        StartCoroutine(SlideHelpBackImage());
    }

    IEnumerator SlideHelpBackImage()
    {
        while(helpBackImageComponent.fillAmount < 1.0f)
        {
            helpBackImageComponent.fillAmount += helpBackImageOpenSpeed * Time.deltaTime;
            yield return null;
        }

        helpText.SetActive(true);
        helpWords.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        LoadingSignRotate();
    }
    

    void LoadingSignRotate()
    {
        loadingSignRectTransform.Rotate(0,loadingSignRotateSpeed * Mathf.Abs(Mathf.Sin(loadingSignRotateFrequency * timer)), 0);
    }
}
