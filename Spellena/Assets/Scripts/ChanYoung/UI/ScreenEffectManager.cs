using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEffectManager : MonoBehaviour
{
    class ScreenEffect
    {
        public bool isUp = true;
        public string type;
        public int frame = 0;
    }
    List<ScreenEffect> screenEffects = new List<ScreenEffect>();

    public GameObject screenEffect;

    Image hitEffect;
    Image healEffect;

    private void Start()
    {
        hitEffect = screenEffect.transform.Find("HitEffect").GetComponent<Image>();
        healEffect = screenEffect.transform.Find("HealEffect").GetComponent<Image>();
    }

    private void FixedUpdate()
    {
        int _hitFrame = 0;
        int _healFrame = 0;
        for (int i = 0; i < screenEffects.Count; i++)
        {
            if (screenEffects[i].type == "Hit")
            {
                if (screenEffects[i].isUp)
                {
                    screenEffects[i].frame++;
                    _hitFrame += screenEffects[i].frame;
                    if (screenEffects[i].frame >= 5)
                        screenEffects[i].isUp = false;
                }
                else
                {
                    screenEffects[i].frame--;
                    _hitFrame += screenEffects[i].frame;
                }
            }
            else if (screenEffects[i].type == "Heal")
            {
                if (screenEffects[i].isUp)
                {
                    screenEffects[i].frame++;
                    _healFrame += screenEffects[i].frame;
                    if (screenEffects[i].frame >= 5)
                        screenEffects[i].isUp = false;
                }
                else
                {
                    screenEffects[i].frame--;
                    _healFrame += screenEffects[i].frame;
                }
            }
        }

        for (int i = 0; i < screenEffects.Count; i++)
        {
            if (screenEffects[i].frame <= 0)
            {
                screenEffects.RemoveAt(i);
                i = -1;
            }
        }

        Color _hitColor = hitEffect.color;
        _hitColor.a = 0.2f * _hitFrame;
        Color _healColor = healEffect.color;
        _healColor.a = 0.2f * _healFrame;

        Debug.Log(_hitFrame);

        hitEffect.color = _hitColor;
        healEffect.color = _healColor;

    }

    public void PlayDamageEffect(int damage)
    {
        ScreenEffect _tempEffect = new ScreenEffect();
        if (damage == 0)
            return;
        else if (damage > 0)
            _tempEffect.type = "Hit";
        else if (damage < 0)
            _tempEffect.type = "Heal";
        screenEffects.Add(_tempEffect);
    }

}
