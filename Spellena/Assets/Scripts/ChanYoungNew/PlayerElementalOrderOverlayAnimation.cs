using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerElementalOrderOverlayAnimation : PlayerCommonOverlayAnimation
{

    private Vector3 rightHandPos;
    private Vector3 leftHandPos;

    private Vector3 originCameraVec;

    protected override void Start()
    {
        base.Start();
        originCameraVec = cameraOverlay.transform.localPosition;
    }

    protected override void Update()
    {
        base.Update();
        rightHandPos = sightOverlay.position + transform.right * 0.3f;
        leftHandPos = sightOverlay.position - transform.right * 0.3f;
    }

    private void OnAnimatorIK()
    {
        if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.3f, Time.deltaTime * 20);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0.3f, Time.deltaTime * 20);
            SetIK(rightHandPos, leftHandPos);
            SetCameraPos(0f);
        }
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Skill1Channeling"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.5f, Time.deltaTime * 20);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0.5f, Time.deltaTime * 20);

            Vector3 _targetVec = sightOverlay.position + Vector3.down * 0.3f;
            SetIK(_targetVec, _targetVec);
            SetCameraPos(0.2f);
        }
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Skill2Channeling"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.3f, Time.deltaTime);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0f, Time.deltaTime);
            SetIK(rightHandPos, leftHandPos);
            SetCameraPos(0f);
        }
        else
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0f, Time.deltaTime * 20);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0f, Time.deltaTime * 20);
            SetIK(sightOverlay.position, sightOverlay.position);
            SetCameraPos(0f);
        }

    }

    private void SetCameraPos(float targetVec)
    {
        cameraOverlay.transform.localPosition = Vector3.Lerp(cameraOverlay.transform.localPosition,
            originCameraVec + new Vector3(0, targetVec, 0), Time.deltaTime * 20f);
    }

    private void SetIK(Vector3 rightHand, Vector3 leftHand)
    {
        overlayAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHand);
        overlayAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
        overlayAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHand);
        overlayAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
    }
}
