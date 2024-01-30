using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class PlayerCommonOverlayAnimation : MonoBehaviour
{
    PlayerCommon playerCommon;
    protected Animator overlayAnimator;
    private PlayerData playerData;
    private PhotonView rootPhotonView;
    protected GameObject cameraOverlay;
    protected Transform sightOverlay;

    protected float rightHandWeight = 0f;
    protected float leftHandWeight = 0f;
    protected float rightHandRotWeight = 0f;
    protected float leftHandRotWeight = 0f;

    public enum AnimationType
    {
        None, SkillCasting, SkillChanneling, PlainCasting, PlainChanneling
    }

    protected AnimationType currentAnimationType = AnimationType.None;
    protected int currentAnimationIndex = -1;

    private int animationListener;

    virtual protected void Start()
    {
        playerCommon = transform.root.GetComponent<PlayerCommon>();

        playerCommon.PlayAnimation += SetAnimationParameter;


        playerData = playerCommon.playerData;
        overlayAnimator = GetComponent<Animator>();
        animationListener = animationListener = overlayAnimator.GetCurrentAnimatorStateInfo(1).fullPathHash;
        rootPhotonView = transform.root.GetComponent<PhotonView>();
        cameraOverlay = playerCommon.cameraOverlay.gameObject;
        sightOverlay = cameraOverlay.transform.GetChild(0);

    }

    virtual protected void Update()
    {
        if (rootPhotonView.IsMine)
        {
            if (ListenAnimatorState())
                PlayAnimationChangeEvent();
        }
    }

    virtual protected bool ListenAnimatorState()
    {
        int _hash = overlayAnimator.GetNextAnimatorStateInfo(1).fullPathHash;
        if (_hash == 0)
            return false;
        if (_hash != animationListener)
        {
            animationListener = _hash;
            return true;
        }
        return false;
    }

    virtual protected void PlayAnimationChangeEvent()
    {
        AnimatorStateInfo _info = overlayAnimator.GetNextAnimatorStateInfo(1);
        SetAnimationTime(_info);
    }

    virtual protected void SetAnimationTime(AnimatorStateInfo info)
    {
        string _temp;
        string _parameter = "";
        float _time = -1f;
        if (currentAnimationType == AnimationType.None)
        {
            for(int i = 0; i < playerData.skillCastingTime.Count; i++)
            {
                _temp = "Skill" + (i + 1);
                bool _isSkill = overlayAnimator.GetBool(_temp);
                if (_isSkill)
                {
                    if (playerData.skillCastingTime[i] <= 0)
                    {
                        _time = playerData.skillChannelingTime[i];
                        _parameter = _temp + "Channeling";
                        currentAnimationType = AnimationType.SkillChanneling;
                        Debug.Log(_time);
                    }
                    else
                    {
                        _time = playerData.skillCastingTime[i];
                        _parameter = _temp + "Casting";
                        currentAnimationType = AnimationType.SkillCasting;
                    }
                    currentAnimationIndex = i;
                    overlayAnimator.SetBool(_temp, false);
                    break;
                }
            }

            for(int i = 0; i < playerData.plainCastingTime.Count; i++)
            {
                _temp = "Plain" + (i + 1);
                bool _isPlain = overlayAnimator.GetBool(_temp);
                if(_isPlain)
                {
                    if (playerData.plainCastingTime[i] <= 0)
                    {
                        _time = playerData.plainChannelingTime[i];
                        _parameter = _temp + "Channeling";
                        currentAnimationType = AnimationType.PlainChanneling;
                    }
                    else
                    {
                        _time = playerData.plainCastingTime[i];
                        _parameter = _temp + "Casting";
                        currentAnimationType = AnimationType.PlainCasting;
                    }
                    currentAnimationIndex = i;
                    break;
                }
                overlayAnimator.SetBool(_temp, false);
            }    
        }
        else if(currentAnimationType == AnimationType.SkillCasting)
        {
            if (playerData.skillChannelingTime[currentAnimationIndex] <= 0)
            {
                currentAnimationType = AnimationType.None;
                currentAnimationIndex = -1;
            }
            else
            {
                _time = playerData.skillChannelingTime[currentAnimationIndex];
                _parameter = "Skill" + (currentAnimationIndex + 1) + "Channeling";
                currentAnimationType = AnimationType.SkillChanneling;
            }
        }
        else if(currentAnimationType == AnimationType.PlainCasting)
        {
            if (playerData.plainChannelingTime[currentAnimationIndex] <= 0)
            {
                currentAnimationType = AnimationType.None;
                currentAnimationIndex = -1;
            }
            else
            {
                _time = playerData.plainChannelingTime[currentAnimationIndex];
                _parameter = "Plain" + (currentAnimationIndex + 1) + "Channeling";
                currentAnimationType = AnimationType.PlainChanneling;
            }
        }
        else if(currentAnimationType == AnimationType.SkillChanneling ||
            currentAnimationType == AnimationType.PlainChanneling)
        {
            currentAnimationType = AnimationType.None;
            currentAnimationIndex = -1;
        }

        if (_time > 0f)
        {
            float _length = info.length;
            if (_length <= 1.01 && _length >= 0.99)
                return;

            float _speed = _length / _time;
            overlayAnimator.SetFloat(_parameter, _speed);
        }
    }

    virtual protected void SetAnimationParameter(string type, int index)
    {
        if(!rootPhotonView.IsMine || index == -1)
            return;

        string _type = ""; string _stateName = "";

        if(type == "Skill")
        {
            for (int i = 0; i < playerData.skillCastingTime.Count; i++)
            {
                _type = "Skill" + (i + 1);
                overlayAnimator.SetBool(_type, false);
            }

            _stateName = "Skill" + (index + 1);
            overlayAnimator.SetBool(_stateName, true);
        }
        else if(type == "Plain")
        {
            for(int i = 0; i < playerData.plainCastingTime.Count; i++)
            {
                _type = "Plain" + (i + 1);
                overlayAnimator.SetBool(_type, false);
            }

            _stateName = "Plain" + (index + 1);
            overlayAnimator.SetBool(_stateName, true);
        }
    }
}
