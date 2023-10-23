using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerItem : MonoBehaviour
{
    public Text playerName;

    Image backgroundImage;
    public Color highlightColor;
    public GameObject leftArrowButton;
    public GameObject rightArrowButton;
    LobbyManager lobbyManagerScript;

    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    // Start is called before the first frame update
    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
        lobbyManagerScript = FindAnyObjectByType<LobbyManager>();
    }
    public void SetPlayerInfo(Photon.Realtime.Player _player)
    {
        playerName.text = _player.UserId;
    }

    public void ApplyLocalChanges()
    {
        backgroundImage.color = highlightColor;
        leftArrowButton.SetActive(false);
        rightArrowButton.SetActive(true);
    }

    public void OnClickLeftArrow()
    {
        lobbyManagerScript.GetListA().Remove(this);
        lobbyManagerScript.GetListB().Add(this);
        this.transform.SetParent(GameObject.Find("TeamAList").transform);
        leftArrowButton.SetActive(false);
        rightArrowButton.SetActive(true);
    }

    public void OnClickRightArrow()
    {
        lobbyManagerScript.GetListB().Remove(this);
        lobbyManagerScript.GetListA().Add(this);
        this.transform.SetParent(GameObject.Find("TeamBList").transform);
        leftArrowButton.SetActive(true);
        rightArrowButton.SetActive(false);
    }

    public PlayerItem GetPlayerItem()
    {
        return this;
    }
}
