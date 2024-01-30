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

    public void SetRoomInfo(string _roomName, int _playerCount, int _maxPlayer, string _masterClientName)
    {
        roomName = _roomName;
        roomNameText.text = _roomName.Length > maxChar ? _roomName.Substring(0, maxChar) + "..." : _roomName;
        playerCountText.text = $"{_playerCount} / {_maxPlayer}";
        masterPlayerName.text = _masterClientName;
    }

    public void OnClickItem()
    {
        Debug.Log(roomName + " : πÊ ¿‘¿Â");
        manager.JoinRoom(roomName);
    }

}
