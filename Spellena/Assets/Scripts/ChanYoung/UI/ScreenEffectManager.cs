using Player;
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
    public BuffDebuffChecker buffDebuffChecker;

    Image hitEffect;
    Image healEffect;
    Image horrorEffect;
    Image phlegmHorrorEffect;
    Image backgroundEffect;

    Image lungeEffect;
    Image blessingCastingEffect;

    Image cameraDownEffect;
    int cameraDownFrame = 0;

    private void Start()
    {
        hitEffect = screenEffect.transform.Find("HitEffect").GetComponent<Image>();
        healEffect = screenEffect.transform.Find("HealEffect").GetComponent<Image>();
        horrorEffect = screenEffect.transform.Find("HorrorEffect").GetComponent<Image>();
        phlegmHorrorEffect = screenEffect.transform.Find("PhlegmHorrorEffect").GetComponent<Image>();
        backgroundEffect = phlegmHorrorEffect.transform.GetChild(0).GetComponent<Image>();
        if(transform.root.GetComponent<Cultist>() != null)
        {
            lungeEffect = screenEffect.transform.Find("LungeEffect").GetComponent<Image>();
            blessingCastingEffect = screenEffect.transform.Find("BlessingCastingEffect").GetComponent<Image>();
            cameraDownEffect = screenEffect.transform.Find("CameraDownEffect").GetComponent<Image>();
        }
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

        hitEffect.color = _hitColor;
        healEffect.color = _healColor;
        //Debug.Log(_hitColor);
        CheckHorror();
        CheckPhlegmHorror();
        CheckCultist();

    }

    void CheckCultist()
    {
        if (transform.root.GetComponent<Cultist>() != null)
        {
            CheckLunge();
            CheckBlessing();
        }
    }

    void CheckBlessing()
    {
        Cultist _tempCultist = transform.root.GetComponent<Cultist>();
        Color _blessingColor = blessingCastingEffect.color;
        if(buffDebuffChecker.FindBuffDebuff("BlessingCast"))
        {
            if (_blessingColor.a < 1f)
                _blessingColor.a += 0.1f;
        }
        else
        {
            if (_blessingColor.a > 0f)
                _blessingColor.a -= 0.1f;
        }

        blessingCastingEffect.color = _blessingColor;
    }

    void CheckLunge()
    {
        Cultist _tempCultist = transform.root.GetComponent<Cultist>();
        Color _lungeColor = lungeEffect.color;
        if (_tempCultist.skillState == SkillStateCultist.LungeHolding)
        {
            if (_lungeColor.a < 1f)
                _lungeColor.a += 0.1f;
        }
        else
        {
            if (_lungeColor.a > 0f)
                _lungeColor.a -= 0.1f;
        }

        lungeEffect.color = _lungeColor;
    }

    void CheckPhlegmHorror()
    {
        if(transform.root.GetComponent<Cultist>() != null)
        {
            Color _phlegmHorrorColor = phlegmHorrorEffect.color;
            Color _backgroundColor = backgroundEffect.color;
            if (transform.root.GetComponent<Cultist>().skillState == SkillStateCultist.Skill2Channeling)
            {
                if (_phlegmHorrorColor.a < 1f)
                    _phlegmHorrorColor.a += 0.1f;
                if(_backgroundColor.a < 0.04f)
                    _backgroundColor.a += 0.004f;
            }
            else
            {
                if (_phlegmHorrorColor.a > 0f)
                    _phlegmHorrorColor.a -= 0.1f;
                if (_backgroundColor.a > 0f)
                    _backgroundColor.a -= 0.004f;
            }

            phlegmHorrorEffect.color = _phlegmHorrorColor;
            backgroundEffect.color = _backgroundColor;
        }
    }

    void CheckHorror()
    {
        Color _horrorColor = horrorEffect.color;
        if(buffDebuffChecker.FindBuffDebuff("Horror"))
        {
            if (_horrorColor.a < 1f)
                _horrorColor.a += 0.2f;
        }
        else
        {
            if (_horrorColor.a > 0f)
                _horrorColor.a -= 0.2f;
        }
        horrorEffect.color = _horrorColor;
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

    public void PlayCameraDownEffect()
    {

    }

}
