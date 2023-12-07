using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SelectingCharacter : MonoBehaviour
{
    Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();

    Text timeCountText;

    public List<GameObject> avatar;

    // 0 : champSelect, 1 : champSelected
    public List<AudioClip> clips;

    public GameObject noChoose;
    public GameObject confirm;
    public GameObject reSelect;

    private string selectName;
    private AudioSource audioSource;

    [HideInInspector]
    public float timer = -1.0f;

    private void Start()
    {
        ConnectUI();
        audioSource = GetComponent<AudioSource>();
    }
    void ConnectUI()
    {
        UIObjects["timeCount"] = GameCenterTest.FindObject(gameObject, "TimeCount");
        timeCountText = UIObjects["timeCount"].GetComponent<Text>();
    }

    private void Update()
    {
        ShowTimer();
    }

    [PunRPC]
    public void ReceiveTimerCount(float time)
    {
        timer = time;
    }

    [PunRPC]
    public void ActiveCharacterSelectObj(string uiName, bool isActive)
    {
        if (!UIObjects.ContainsKey(uiName)) return;
        UIObjects[uiName].SetActive(isActive);
    }

    void ShowTimer()
    {
        if(timer < 0)
        {
            timeCountText.text = null;
        }

        else
        {
            timeCountText.text = string.Format("{0:F0}", timer);
        }

    }

    public void SelectCharacter(string name)
    {
        selectName = name;
        noChoose.SetActive(false);
        audioSource.clip = clips[0];
        audioSource.Play();

        switch (name)
        {
            case "Aeterna":
                ShowCharacter(0);
                break;
            case "ElementalOrder":
                ShowCharacter(1);
                break;
            case "Dracoson":
                ShowCharacter(2);
                break;
            case "Cultist":
                ShowCharacter(3);
                break;
            default:
                break;
        }
    }

    void ShowCharacter(int index)
    {
        for(int i = 0; i < avatar.Count; i++)
        {
            if(i == index)
            {
                avatar[i].SetActive(true);
            }

            else
            {
                avatar[i].SetActive(false);
            }
        }
    }

    public void ConfirmCharacter()
    {
        if (!avatar[0].activeSelf && !avatar[1].activeSelf && !avatar[2].activeSelf && !avatar[3].activeSelf) return;

        confirm.SetActive(false);
        reSelect.SetActive(true);
        GameCenterTest.ChangePlayerCustomProperties(PhotonNetwork.LocalPlayer, "Character", selectName);
        audioSource.clip = clips[1];
        audioSource.Play();
    }

    public void ReSelectCharacter()
    {
        confirm.SetActive(true);
        reSelect.SetActive(false);
    }


}
