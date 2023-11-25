using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    public Text roomNameText;
    public Text masterPlayerName;
    public Text entryStatusText;
    public Text playerCountText;
    public Image entryIcon;
    public GameObject blackOut;
    public Sprite admissionPossible;
    public Sprite entryNotPossible;
    public int maxChar = 10;
    LobbyManager manager;

    private string roomName;

    private void Start()
    {
        manager = FindObjectOfType<LobbyManager>();
    }

    public void SetRoomInfo(string _roomName, int _playerCount, int _maxPlayer, string _masterClientName,
        string _gameState)
    {
        roomName = _roomName;
        if(_roomName.Length > maxChar)
        {
            roomNameText.text = _roomName.Substring(0, maxChar) + "...";
        }
        playerCountText.text = $"{_playerCount} / {_maxPlayer}";
        masterPlayerName.text = _masterClientName;

        if(_gameState == "InGame")
        {
            entryStatusText.text = "입장 불가";
            blackOut.SetActive(true);
            entryIcon.sprite = entryNotPossible;
        }
        else if(_gameState == "Waiting")
        {
            entryStatusText.text = "입장 가능";
            blackOut.SetActive(false);
            entryIcon.sprite = admissionPossible;
        }
    }

    public void OnClickItem()
    {
        Debug.Log(roomName + " : 방 입장");
        manager.JoinRoom(roomName);
    }

}
