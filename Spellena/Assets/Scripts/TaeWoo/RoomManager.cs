using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public GameObject player;

    public Transform spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(message: "Connecting...");

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log(message: "Connected to Server");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        Debug.Log(message: "We're in the Lobby");

        RoomOptions roomOption = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 10
        };

        PhotonNetwork.JoinOrCreateRoom(roomName: "test", roomOptions: roomOption, typedLobby: null);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        //player = PhotonNetwork.Instantiate("ChanYoung/Prefabs/ElementalOrder", spawnPoint.position, Quaternion.identity);
        player = PhotonNetwork.Instantiate("ChanYoungNew/PrefabsNew/Cultist", spawnPoint.position, Quaternion.identity);
        if(player.GetComponent<PhotonView>().IsMine)
        {
            player.GetComponent<PlayerCommon>().SetLocalPlayer();
        }
    }
}

