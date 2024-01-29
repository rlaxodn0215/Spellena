using Photon.Pun;
using UnityEngine;

public class PlayerCommonAnimation : MonoBehaviourPunCallbacks, IPunObservable
{

    public Camera cameraMain;
    protected Transform sightMain;
    protected Transform handSightMain;

    protected Animator animator;
    protected PlayerCommon playerCommon;
    protected PlayerData playerData;

    protected float moveDirectionVertical = 0;
    protected float moveDirectionHorizontal = 0;
    protected float targetMoveDirectionVertical = 0;
    protected float targetMoveDirectionHorizontal = 0;

    protected float rightHandWeight = 0f;
    protected float leftHandWeight = 0f;


    protected string nextAnimationState = string.Empty;
    private Quaternion networkRotation;
    public enum AnimationType
    {
        None, SkillCasting, SkillChanneling, PlainCasting, PlainChanneling
    }

    protected AnimationType currentAnimationType = AnimationType.None;
    protected int currentAnimationIndex = -1;

    int animationListener;

    virtual protected void Start()
    {
        InitCommonComponents();
    }

    virtual protected void Update()
    {
        LerpLowerAnimation();

        if (!photonView.IsMine)
            cameraMain.transform.localRotation = Quaternion.Lerp(cameraMain.transform.localRotation, networkRotation, Time.deltaTime * 8);

        if(ListenAnimatorState())
            PlayAnimationChangeEvent();
    }

    private void LerpLowerAnimation()
    {
        moveDirectionVertical = Mathf.Lerp(moveDirectionVertical, targetMoveDirectionVertical, Time.deltaTime * 20f);
        moveDirectionHorizontal = Mathf.Lerp(moveDirectionHorizontal, targetMoveDirectionHorizontal, Time.deltaTime * 20f);

        animator.SetFloat("MoveDirectionVertical", moveDirectionVertical);
        animator.SetFloat("MoveDirectionHorizontal", moveDirectionHorizontal);
    }

    protected void InitCommonComponents()
    {
        networkRotation = cameraMain.transform.localRotation;

        playerCommon = transform.root.GetComponent<PlayerCommon>();
        playerCommon.UpdateLowerAnimation += UpdateLowerAnimation;
        sightMain = cameraMain.transform.GetChild(0);
        handSightMain = cameraMain.transform.GetChild(1);
        animator = GetComponent<Animator>();
        playerData = playerCommon.playerData;
        playerCommon.PlayAnimation += SetAnimationParameter;

        animationListener = animator.GetCurrentAnimatorStateInfo(1).fullPathHash;
    }

    virtual protected void UpdateLowerAnimation(Vector2 moveDirection, bool isRunning)
    {
        //y가 앞뒤, x가 좌우 x가 0보다 크면 오른쪽
        if(isRunning)
            moveDirection *= 2;

        targetMoveDirectionVertical = moveDirection.y;
        targetMoveDirectionHorizontal = moveDirection.x;

        animator.SetFloat("TargetMoveDirectionVertical", moveDirection.y);
        animator.SetFloat("TargetMoveDirectionHorizontal", moveDirection.x);
    }

    virtual protected bool ListenAnimatorState()
    {
        int _hash = animator.GetNextAnimatorStateInfo(1).fullPathHash;
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
        AnimatorStateInfo _info = animator.GetNextAnimatorStateInfo(1);
        SetAnimationTime(_info);
    }
    virtual protected void SetAnimationTime(AnimatorStateInfo info)
    {
        string _temp;
        string _parameter = "";
        float _time = -1f;
        if (currentAnimationType == AnimationType.None)
        {
            for (int i = 0; i < playerData.skillCastingTime.Count; i++)
            {
                _temp = "Skill" + (i + 1);
                bool _isSkill = animator.GetBool(_temp);
                if (_isSkill)
                {
                    if (playerData.skillCastingTime[i] <= 0)
                    {
                        _time = playerData.skillChannelingTime[i];
                        _parameter = _temp + "Channeling";
                        currentAnimationType = AnimationType.SkillChanneling;
                    }
                    else
                    {
                        _time = playerData.skillCastingTime[i];
                        _parameter = _temp + "Casting";
                        currentAnimationType = AnimationType.SkillCasting;
                    }
                    currentAnimationIndex = i;
                    animator.SetBool(_temp, false);
                    break;
                }
            }

            for (int i = 0; i < playerData.plainCastingTime.Count; i++)
            {
                _temp = "Plain" + (i + 1);
                bool _isPlain = animator.GetBool(_temp);
                if (_isPlain)
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
                animator.SetBool(_temp, false);
            }
        }
        else if (currentAnimationType == AnimationType.SkillCasting)
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
        else if (currentAnimationType == AnimationType.PlainCasting)
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
        else if (currentAnimationType == AnimationType.SkillChanneling ||
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
            animator.SetFloat(_parameter, _speed);
        }
    }

    virtual protected void OnAnimatorIK()
    {
        animator.SetLookAtPosition(sightMain.position);
        animator.SetLookAtWeight(1f);
    }

    virtual protected void SetAnimationParameter(string type, int index)
    {
        if (index == -1)
            return;

        string _type = ""; string _stateName = "";

        if (type == "Skill")
        {
            for (int i = 0; i < playerData.skillCastingTime.Count; i++)
            {
                _type = "Skill" + (i + 1);
                animator.SetBool(_type, false);
            }

            _stateName = "Skill" + (index + 1);
            animator.SetBool(_stateName, true);
        }
        else if (type == "Plain")
        {
            for (int i = 0; i < playerData.plainCastingTime.Count; i++)
            {
                _type = "Plain" + (i + 1);
                animator.SetBool(_type, false);
            }

            _stateName = "Plain" + (index + 1);
            animator.SetBool(_stateName, true);
        }
    }


    virtual public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(cameraMain.transform.localRotation);
        }
        else
        {
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }

}
