using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private readonly string Version = "1.0";
    private string userID = "Inha";
    private string logText;
    private void Awake()
    {
        Screen.SetResolution(800, 480, false);
        logText = "Connecting to Master";
        Debug.Log(logText);
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected Master");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Failed to join random room");
        CreateRoom();
    }

    void CreateRoom()
    {
        logText = "Creating Room";

        Debug.Log(logText);
        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 3
        };

        PhotonNetwork.CreateRoom("TT_Test Room", roomOptions);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        logText = "Failed to Create room... try again";
        Debug.Log(logText);
        CreateRoom();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        logText = "OnJoined Room";
        Debug.Log(logText);
        //PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
    }

    private void OnGUI()
    {
        GUI.TextArea(new Rect(10, 10, 300, 20), logText);
    }
}
