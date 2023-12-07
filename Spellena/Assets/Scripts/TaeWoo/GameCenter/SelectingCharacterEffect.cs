using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectingCharacterEffect : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    private GameObject cursorEffect;
    private GameObject selectEffect;

    void Start()
    {
        cursorEffect = GameCenterTest.FindObject(gameObject, "CursorEffect");
        selectEffect = GameCenterTest.FindObject(gameObject, "SelectEffect");  
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        cursorEffect.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cursorEffect.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selectEffect.SetActive(true);
    }
}
