using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SelectingCharacter : MonoBehaviour
{
    public List<GameObject> avatar;
    public GameObject confirm;
    public GameObject reSelect;

    private string selectName;

   public void SelectCharacter(string name)
   {
        switch (name)
        {
            case "Aeterna":
                selectName = name;
                avatar[0].SetActive(true);
                avatar[1].SetActive(false);
                break;
            case "ElementalOrder":
                selectName = name;
                avatar[0].SetActive(false);
                avatar[1].SetActive(true);
                break;
            default:
                break;
        }
   }

    public void ConfirmCharacter()
    {
        if (!avatar[0].activeSelf && !avatar[1].activeSelf) return;

        confirm.SetActive(false);
        reSelect.SetActive(true);
        GameCenterTest.ChangePlayerCustomProperties(PhotonNetwork.LocalPlayer, "Character", selectName);
        // 시간 초과시 마지막에 선택한 캐릭터가 된다
        // 아무 선택 안하면 랜덤으로 소환
    }

    public void ReSelectCharacter()
    {
        confirm.SetActive(true);
        reSelect.SetActive(false);
    }



}
