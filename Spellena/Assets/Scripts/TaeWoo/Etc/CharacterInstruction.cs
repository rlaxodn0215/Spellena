using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using temp;

public class CharacterInstruction : MonoBehaviour
{
    Dictionary<string, GameObject> UIObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        UIObjects["Aeterna"] = Helper.FindObject(gameObject, "Aeterna");
        UIObjects["ElementalOrder"] = Helper.FindObject(gameObject, "ElementalOrder");
        UIObjects["Dracoson"] = Helper.FindObject(gameObject, "Dracoson");
        UIObjects["Cultist"] = Helper.FindObject(gameObject, "Cultist");
        UIObjects["KeySettings"] = Helper.FindObject(gameObject, "KeySettings");
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

        if (Input.GetKeyDown(KeyCode.F2))
        {
            ActiveInstruction("KeySettings", true);
        }

        if (Input.GetKeyUp(KeyCode.F2))
        {
            ActiveInstruction("KeySettings", false);
        }
    }

    void ActiveInstruction(string name, bool isActive)
    {
        if(UIObjects.ContainsKey(name))
        UIObjects[name].SetActive(isActive);      
    }
}
