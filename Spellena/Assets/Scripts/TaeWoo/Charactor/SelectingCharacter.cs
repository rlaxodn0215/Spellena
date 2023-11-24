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

    public GameObject noChoose;
    public GameObject confirm;
    public GameObject reSelect;

    private string selectName;

    [HideInInspector]
    public float timer = -1.0f;

    private void Start()
    {
        ConnectUI();
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
        switch (name)
        {
            case "Aeterna":
                selectName = name;
                avatar[0].SetActive(true);
                avatar[1].SetActive(false);
                noChoose.SetActive(false);
                break;
            case "ElementalOrder":
                selectName = name;
                avatar[0].SetActive(false);
                avatar[1].SetActive(true);
                noChoose.SetActive(false);
                break;
            default:
                break;
        }
   }

    public void ConfirmCharacter()
    {
        if (!avatar[0].activeSelf && !avatar[1].activeSelf) return;

        confirm.SetActive(false);
        reSelect.SetActive(true);
        GameCenterTest.ChangePlayerCustomProperties(PhotonNetwork.LocalPlayer, "Character", selectName);
    }

    public void ReSelectCharacter()
    {
        confirm.SetActive(true);
        reSelect.SetActive(false);
    }

}
