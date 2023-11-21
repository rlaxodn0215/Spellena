using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerStats : MonoBehaviour
{
    public GameObject statAssemble;
    public GameObject[] stats;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Tab))
        {
            statAssemble.SetActive(true);
            ShowStats();
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            statAssemble.SetActive(false);
        }
    }

    void ShowStats()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            stats[i].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].CustomProperties["Name"].ToString();
            stats[i].transform.GetChild(1).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].CustomProperties["Team"].ToString();
            stats[i].transform.GetChild(2).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].CustomProperties["TotalDamage"].ToString();
            stats[i].transform.GetChild(3).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].CustomProperties["TotalHeal"].ToString();
            stats[i].transform.GetChild(4).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].CustomProperties["KillCount"].ToString();
            stats[i].transform.GetChild(5).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].CustomProperties["DeadCount"].ToString();
            stats[i].transform.GetChild(6).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].CustomProperties["IsAlive"].ToString();
        }
    }
}
