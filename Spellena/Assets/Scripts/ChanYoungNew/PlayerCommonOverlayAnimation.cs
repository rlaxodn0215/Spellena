using UnityEngine;
using Photon.Pun;
using GlobalEnum;
using System.Collections.Generic;

public class PlayerCommonOverlayAnimation : MonoBehaviour
{
    PlayerCommon playerCommon;
    protected Animator overlayAnimator;
    protected PlayerData playerData;
    private PhotonView rootPhotonView;
    protected GameObject cameraOverlay;
    protected Transform sightOverlay;

    protected float rightHandWeight = 0f;
    protected float leftHandWeight = 0f;
    protected float rightHandRotWeight = 0f;
    protected float leftHandRotWeight = 0f;

    public class AnimationRoute
    {
        public List<AnimationType> route = new List<AnimationType>();
        public List<float> routeTime = new List<float>();
        public int routeIndex = 0;
    }

    protected List<AnimationRoute> skillRoutes = new List<AnimationRoute>();
    protected List<AnimationRoute> plainRoutes = new List<AnimationRoute>();
    protected CallType currentAnimationType = CallType.None;
    protected int currentRoute = -1;
    protected int currentAnimationIndex = -1;
    protected string parameterName = "";

    public enum AnimationType
    {
        None, Casting, Channeling
    }

    private int animationListener;

    virtual protected void Start()
    {
        InitCommonComponents();
        InitUniqueComponents();
    }

    virtual protected void Update()
    {
        if (rootPhotonView.IsMine)
        {
            if (ListenAnimatorState())
                PlayAnimationChangeEvent();
        }
    }

    protected void InitCommonComponents()
    {
        playerCommon = transform.root.GetComponent<PlayerCommon>();

        playerData = playerCommon.playerData;
        overlayAnimator = GetComponent<Animator>();
        animationListener = animationListener = overlayAnimator.GetCurrentAnimatorStateInfo(1).fullPathHash;
        rootPhotonView = transform.root.GetComponent<PhotonView>();
        cameraOverlay = playerCommon.cameraOverlay.gameObject;
        sightOverlay = cameraOverlay.transform.GetChild(0);

        playerCommon.PlayAnimation += PlayAnimation;
        AddSkillRoute(playerData.skillCastingTime.Count);
        AddPlainRoute(playerData.plainCastingTime.Count);
    }

    virtual protected void InitUniqueComponents()
    {

    }

    protected void AddSkillRoute(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AnimationRoute _route = new AnimationRoute();
            skillRoutes.Add(_route);
        }
    }

    protected void AddPlainRoute(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AnimationRoute _route = new AnimationRoute();
            plainRoutes.Add(_route);
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
        ChangeAnimationRoot(_info);
    }


    virtual protected void ChangeAnimationRoot(AnimatorStateInfo info)
    {
        if (currentAnimationType == CallType.Skill)
        {
            skillRoutes[currentAnimationIndex].routeIndex++;
            CallAnimationEvent();
            if (skillRoutes[currentAnimationIndex].routeIndex >= skillRoutes[currentAnimationIndex].route.Count)
            {
                skillRoutes[currentAnimationIndex].routeIndex = 0;

                currentAnimationType = CallType.None;
                currentAnimationIndex = -1;
            }
            else
                SetAnimationSpeed(info);
        }
        else if (currentAnimationType == CallType.Plain)
        {
            plainRoutes[currentAnimationIndex].routeIndex++;
            CallAnimationEvent();
            if (plainRoutes[currentAnimationIndex].routeIndex >= plainRoutes[currentAnimationIndex].route.Count)
            {
                plainRoutes[currentAnimationIndex].routeIndex = 0;

                currentAnimationType = CallType.None;
                currentAnimationIndex = -1;
            }
            else
                SetAnimationSpeed(info);
        }
    }
    virtual protected void CallAnimationEvent()
    {

    }

    virtual protected void SetAnimationSpeed(AnimatorStateInfo info)
    {
        float _length = info.length;
        float _time = -1f;
        string _parameter = "";


        if (currentAnimationType == CallType.Skill)
        {
            _parameter = "Skill" + (currentAnimationIndex + 1);
            _time = skillRoutes[currentAnimationIndex].routeTime[skillRoutes[currentAnimationIndex].routeIndex];

            AnimationType _tempType = skillRoutes[currentAnimationIndex].route[skillRoutes[currentAnimationIndex].routeIndex];

            if (_length > _time - 0.01f && _length < _time + 0.01f)
                return;


            AddAnimationType(ref _parameter, _tempType);
        }
        else if (currentAnimationType == CallType.Plain)
        {
            _parameter = "Plain" + (currentAnimationIndex + 1);
            _time = plainRoutes[currentAnimationIndex].routeTime[plainRoutes[currentAnimationIndex].routeIndex];

            AnimationType _tempType = plainRoutes[currentAnimationIndex].route[plainRoutes[currentAnimationIndex].routeIndex];

            if (_length > _time - 0.01f && _length < _time + 0.01f)
                return;

            AddAnimationType(ref _parameter, _tempType);
        }

        if (_time > 0f)
        {
            float _speed = _length / _time;
            overlayAnimator.SetFloat(_parameter, _speed);
        }
    }

    protected void AddAnimationType(ref string parameter, AnimationType targetType)
    {
        if (targetType == AnimationType.Casting)
            parameter += "Casting";
        else if (targetType == AnimationType.Channeling)
            parameter += "Channeling";
    }

    virtual protected void PlayAnimation(AnimationChangeType changeType, CallType callType, int index)
    {
        if (index < 0)
            return;
        string _parameter = "";
        CallType _callType = CallType.None;

        if (callType == CallType.Skill)
        {
            _callType = CallType.Skill;
            _parameter += "Skill";
        }
        else if (callType == CallType.Plain)
        {
            _callType = CallType.Plain;
            _parameter += "Plain";
        }

        if (changeType == AnimationChangeType.Invoke)
        {
            _parameter += (index + 1);
            overlayAnimator.SetBool(_parameter, true);
            currentAnimationType = _callType;
            currentAnimationIndex = index;
        }
        else
        {
            ChangeAnimation();
        }
    }

    virtual protected void ChangeAnimation()
    {

    }
}
