using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterInstruction : MonoBehaviour
{
    Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        UIObjects["Aeterna"] = GameCenterTest.FindObject(gameObject, "Aeterna");
        UIObjects["ElementalOrder"] = GameCenterTest.FindObject(gameObject, "ElementalOrder");
        UIObjects["Dracoson"] = GameCenterTest.FindObject(gameObject, "Dracoson");
        UIObjects["Cultist"] = GameCenterTest.FindObject(gameObject, "Cultist");
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            ActiveInstruction((string)PhotonNetwork.LocalPlayer.CustomProperties["Character"], true);
        }

        if(Input.GetKeyUp(KeyCode.F1))
        {
            ActiveInstruction((string)PhotonNetwork.LocalPlayer.CustomProperties["Character"], false);
        }
    }

    void ActiveInstruction(string name, bool isActive)
    {
        if(UIObjects.ContainsKey(name))
        UIObjects[name].SetActive(isActive);      
    }
}
