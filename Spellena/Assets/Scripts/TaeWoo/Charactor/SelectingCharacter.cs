using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SelectingCharacter : MonoBehaviour
{
    public List<GameObject> avatar;

   public void SelectCharacter(string name)
   {
        switch (name)
        {
            case "Aeterna":
                avatar[0].SetActive(true);
                avatar[1].SetActive(false);
                break;
            case "ElementalOrder":
                avatar[0].SetActive(false);
                avatar[1].SetActive(true);
                break;
            default:
                break;
        }

        GameCenterTest.ChangePlayerCustomProperties(PhotonNetwork.LocalPlayer, "Character", name);

   }
}
