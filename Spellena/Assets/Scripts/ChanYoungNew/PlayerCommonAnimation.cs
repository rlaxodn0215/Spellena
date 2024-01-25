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

    int animationListener;

    public enum AnimationType
    {
        Casting, Channeling
    }

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
        float _time = 0f;
        string _parameter = "";
        string _skill = "";

        int _check = 0;

        for(int i = 0; i < playerData.skillCastingTime.Count; i++)
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

        animator.SetBool(_skill, false);

        float _length = info.length;
        if (_length <= 1.01 && _length >= 0.99)
            return;

        if (_check == 1)
        {
            float _speed = _length / _time;
            animator.SetFloat(_parameter, _speed);
        }
    }

    virtual protected void OnAnimatorIK()
    {
        animator.SetLookAtPosition(sightMain.position);
        animator.SetLookAtWeight(1f);
    }

    protected void SetAnimationParameter(string type, int index)
    {
        if (index == -1)
            return;
        if(type == "Skill")
        {
            for(int i = 0; i < playerData.skillCastingTime.Count; i++)
            {
                string _type = "Skill" + (i + 1);
                animator.SetBool(_type, false);
            }


            string _stateName = "Skill" + (index + 1);
            animator.SetBool(_stateName, true);
        }
        else if(type == "Plain")
        {

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
