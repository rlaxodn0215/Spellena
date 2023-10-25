using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FriendItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button friendItem;
    public Button inviteButton;
    public Button deleteFriendButton;
    
    // Start is called before the first frame update
    void Start()
    {
        inviteButton.gameObject.SetActive(false);
        deleteFriendButton.gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        inviteButton.gameObject.SetActive(true);
        deleteFriendButton.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inviteButton.gameObject.SetActive(false);
        deleteFriendButton.gameObject.SetActive(false);
    }
}
