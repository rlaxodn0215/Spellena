using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using temp;

public class SelectingCharacterEffect : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    private GameObject cursorEffect;
    private GameObject selectEffect;

    void Start()
    {
        cursorEffect = Helper.FindObject(gameObject, "CursorEffect");
        selectEffect = Helper.FindObject(gameObject, "SelectEffect");  
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
