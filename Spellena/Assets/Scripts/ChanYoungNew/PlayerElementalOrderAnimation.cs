using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerElementalOrderAnimation : PlayerCommonAnimation, IPunObservable
{
    PlayerElementalOrder playerElementalOrder;
    private float forceCrossFadingTime = 0f;
    protected override void Start()
    {
        base.Start();
        playerElementalOrder = transform.root.GetComponent<PlayerElementalOrder>();
    }

    protected override void CorrectUniqueAnimation()
    {
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
        {
            if (forceCrossFadingTime <= 0f)
                animator.SetFloat("Idle", 0);
        }
    }

    protected override void FixedUpdate()
    {
        if (forceCrossFadingTime > 0f)
            forceCrossFadingTime -= Time.fixedDeltaTime;
    }

    protected override void CrossFadeAnimation(string stateName, float targetSpeed)
    {
        forceCrossFadingTime = crossFadeTime;
        animator.SetFloat("Idle", targetSpeed);
        animator.SetFloat(stateName, targetSpeed);
        animator.CrossFade(stateName, crossFadeTime, 1);
    }

    protected override void OnAnimatorIK()
    {
        base.OnAnimatorIK();

        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Skill1Casting"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.5f, Time.deltaTime * 8);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0.5f, Time.deltaTime * 8);
        }
        else
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0f, Time.deltaTime * 8);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0f, Time.deltaTime * 8);
        }


        animator.SetIKPosition(AvatarIKGoal.RightHand, handSightMain.position);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, handSightMain.position);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, rightHandWeight);
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView (stream, info);
    }

}
