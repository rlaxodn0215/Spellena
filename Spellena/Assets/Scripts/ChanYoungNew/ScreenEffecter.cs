using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffecter : MonoBehaviour
{
    Image horrorEffect;

    private CharacterStateController characterStateController;

    private List<string> screenEffect = new List<string>();
    private List<int> screenEffectFrame = new List<int>();

    private void Start()
    {
        characterStateController = transform.root.GetComponent<CharacterStateController>();
        characterStateController.PlayScreenEffect += AddScreenEffectFrame;
    }

    private void FixedUpdate()
    {
        for(int i = 0; i < screenEffect.Count; i++)
        {
            screenEffectFrame[i]--;
        }

        for(int i = 0; i < screenEffect.Count; i++)
        {
            if (screenEffectFrame[i] <= 0)
            {
                SetColor(screenEffect[i], 0);

                screenEffectFrame.RemoveAt(i);
                screenEffect.RemoveAt(i);

                i = -1;
            }
        }

        for(int i = 0; i < screenEffect.Count; i++)
        {
            SetColor(screenEffect[i], screenEffectFrame[i]);
        }
    }

    private void SetColor(string effectName, int frame)
    {
        Color _color;
        if(effectName == "Horror")
        {
            _color = horrorEffect.color;
            _color.a = 0.2f * frame;
            horrorEffect.color = _color;
        }
    }

    private void AddScreenEffectFrame(string effectName, float time)
    {
        int _frame = (int)(time / 0.2f);
        int _check = 0;

        for(int i = 0; i < screenEffect.Count; i++)
        {
            if (screenEffect[i] == effectName)
            {
                if (screenEffectFrame[i] < _frame)
                    screenEffectFrame[i] = _frame;
                _check = 1;
                break;
            }
        }

        if(_check == 0)
        {
            screenEffect.Add(effectName);
            screenEffectFrame.Add(_frame);
        }
    }

}
