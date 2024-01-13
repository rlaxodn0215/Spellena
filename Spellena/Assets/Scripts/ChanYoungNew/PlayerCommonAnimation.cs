using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Animations;
using UnityEngine;

public class PlayerCommonAnimation : MonoBehaviourPunCallbacks, IPunObservable
{

    public Camera cameraMain;
    protected Transform sightMain;
    protected Transform handSightMain;

    protected Animator animator;
    protected AnimatorController animatorController;
    protected List<AnimatorState> states = new List<AnimatorState>();
    protected List<AnimationClip> clips = new List<AnimationClip>();

    protected PlayerCommon playerCommon;
    protected PlayerData playerData;

    protected float moveDirectionVertical = 0;
    protected float moveDirectionHorizontal = 0;
    protected float targetMoveDirectionVertical = 0;
    protected float targetMoveDirectionHorizontal = 0;

    protected float crossFadeTime = 0.3f;

    protected float rightHandWeight = 0f;
    protected float leftHandWeight = 0f;

    private Quaternion networkRotation;

    virtual protected void Start()
    {
        InitCommonComponents();
    }

    virtual protected void Update()
    {
        LerpLowerAnimation();
        CorrectUniqueAnimation();

        if (!photonView.IsMine)
            cameraMain.transform.localRotation = Quaternion.Lerp(cameraMain.transform.localRotation, networkRotation, Time.deltaTime * 8);
    }

    virtual protected void FixedUpdate()
    {

    }

    virtual protected void CorrectUniqueAnimation()
    {

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

        animatorController = animator.runtimeAnimatorController as AnimatorController;
        ChildAnimatorState[] _states = animatorController.layers[1].stateMachine.states;

        for (int i = 0; i < _states.Length; i++)
        {
            states.Add(_states[i].state);
            clips.Add(_states[i].state.motion as AnimationClip);
        }

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

    virtual protected void OnAnimatorIK()
    {
        animator.SetLookAtPosition(sightMain.position);
        animator.SetLookAtWeight(1f);
    }


    virtual protected void SetAnimation(int index, int stateIndex, string stateName)
    {
        float skillTime = playerData.skillCastingTime[index];
        float _length = clips[stateIndex].length;
        float _targetSpeed = _length / skillTime;

        CrossFadeAnimation(stateName, _targetSpeed);
    }

    virtual protected void CrossFadeAnimation(string stateName, float targetSpeed)
    {
        animator.SetFloat(stateName, targetSpeed);
        animator.CrossFade(stateName, crossFadeTime, 1);
    }

    protected void SetAnimationParameter(int index)
    {
        if (index == -1)
            return;
        else
        {
            string _stateName = "Skill" + (index + 1) + "Casting";
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].name == _stateName)
                {
                    SetAnimation(index, i, _stateName);
                    break;
                }
            }
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
