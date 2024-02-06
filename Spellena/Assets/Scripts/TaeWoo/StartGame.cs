using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using FSM;
using GameCenterDataType;

public class StartGame : MonoBehaviour
{
    public GameObject redTeam;
    public GameObject blueTeam;

    public void GameStart()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("StartGameToAll", RpcTarget.AllBufferedViaServer);
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }

    [PunRPC]
    public void StartGameToAll()
    {
        LoadSceneManager.LoadNextScene("TaeWooScene_3", MakeTeamActorNumList(redTeam), MakeTeamActorNumList(blueTeam));
    }

    List<int> MakeTeamActorNumList(GameObject teamObj)
    {
        List<int> temp = new List<int>();

        for (int i = 0; i < teamObj.transform.childCount; i++)
        {
            temp.Add(teamObj.transform.GetChild(i).GetComponent<PhotonView>().OwnerActorNr);
        }

        return temp;
    }

}
