using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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
        Debug.Log("Å©¾Æ¾Ç");
        float _time = 0f;
        string _parameter = "";
        string _skill = "";

        int _check = 0;

        for (int i = 0; i < playerData.skillCastingTime.Count; i++)
        {
            string _temp = "Skill" + (i + 1) + "Casting";
            if (info.IsName(_temp))
            {
                _time = playerData.skillCastingTime[i];
                _parameter = _temp;
                _skill = "Skill" + (i + 1);
                _check = 1;
                break;
            }
        }

        if (_check == 0)
        {
            for (int i = 0; i < playerData.skillChannelingTime.Count; i++)
            {
                string _temp = "Skill" + (i + 1) + "Channeling";
                if (info.IsName(_temp))
                {
                    _time = playerData.skillChannelingTime[i];
                    _parameter = _temp;
                    _skill = "Skill" + (i + 1);
                    _check = 1;
                    break;
                }
            }
        }

        overlayAnimator.SetBool(_skill, false);

        float _length = info.length;
        if (_length <= 1.01 && _length >= 0.99)
            return;

        if(_check == 1)
        {
            float _speed = _length / _time;
            overlayAnimator.SetFloat(_parameter, _speed);
        }
    }

    virtual protected void SetAnimationParameter(int index)
    {
        if(!rootPhotonView.IsMine || index == -1)
            return;
        else
        {
            for (int i = 0; i < playerData.skillCastingTime.Count; i++)
            {
                string _type = "Skill" + (i + 1);
                overlayAnimator.SetBool(_type, false);
            }


            string _stateName = "Skill" + (index + 1);
            overlayAnimator.SetBool(_stateName, true);
        }
    }



}
