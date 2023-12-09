using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitCrosshairAni : MonoBehaviour
{
    public List<Image> crosshair;
    private float disappearSpeed = 1.5f;
    Coroutine coroutine;
    private void OnEnable()
    {
        if(coroutine !=null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(CrossHairAni());
    }

    IEnumerator CrossHairAni()
    {
        while (crosshair[0].fillAmount > 0.0f)
        {
            for (int i = 0; i < crosshair.Count; i++)
            {
                crosshair[i].fillAmount -= disappearSpeed * Time.deltaTime;
            }

            yield return null;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < crosshair.Count; i++)
        {
            crosshair[i].fillAmount = 1.0f;
        }
    }

}
