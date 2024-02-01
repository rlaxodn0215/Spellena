using GlobalEnum;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerElementalOrderOverlayAnimation : PlayerCommonOverlayAnimation
{

    private Vector3 rightHandPos;
    private Vector3 leftHandPos;

    private Vector3 originCameraVec;

    /*
    [Range(-1f, 1f)]
    public float rightWeight = 0;
    [Range(-1f, 1f)]
    public float leftWeight = 0;

    [Range(-180f, 180f)]
    public float rightAngle = 0;
    [Range(-180f, 180f)]
    public float leftAngle = 0;
    */

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

    protected override void InitUniqueComponents()
    {
        for (int i = 0; i < skillRoutes.Count; i++)
        {
            skillRoutes[i].route.Add(AnimationType.None);
            skillRoutes[i].routeTime.Add(0);

            skillRoutes[i].route.Add(AnimationType.Casting);
            skillRoutes[i].routeTime.Add(playerData.skillCastingTime[i]);
        }
    }

    protected override void CallAnimationEvent()
    {
        if (currentAnimationType == CallType.Skill)
        {
            if (skillRoutes[currentAnimationIndex].routeIndex == 1)//캐스팅 중일 때
            {
                string _parameter = "Skill" + (currentAnimationIndex + 1);
                overlayAnimator.SetBool(_parameter, false);
            }
        }
    }

    private void OnAnimatorIK()
    {
        if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.3f, Time.deltaTime * 20);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0.3f, Time.deltaTime * 20);

            //rightHandRotWeight = Mathf.Lerp(rightHandRotWeight, rightWeight, Time.deltaTime * 20);
            //leftHandRotWeight = Mathf.Lerp(leftHandRotWeight, leftWeight, Time.deltaTime * 20);

            SetIK(rightHandPos, leftHandPos);
            //SetIKRot(Quaternion.Euler(0, rightAngle, 0) * transform.rotation, Quaternion.Euler(0, leftAngle, 0) * transform.rotation);

            SetCameraPos(0f);
        }
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Skill1Casting"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.5f, Time.deltaTime * 20);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0.5f, Time.deltaTime * 20);


            rightHandRotWeight = Mathf.Lerp(rightHandRotWeight, 0f, Time.deltaTime * 20);
            leftHandRotWeight = Mathf.Lerp(leftHandRotWeight, 0f, Time.deltaTime * 20);

            Vector3 _targetVec = sightOverlay.position + Vector3.down * 0.3f;
            SetIK(_targetVec, _targetVec);
            SetIKRot(Quaternion.identity, Quaternion.identity);
            SetCameraPos(0.2f);
        }
        else if (overlayAnimator.GetCurrentAnimatorStateInfo(1).IsName("Skill2Casting"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.3f, Time.deltaTime);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0f, Time.deltaTime);


            rightHandRotWeight = Mathf.Lerp(rightHandRotWeight, 0f, Time.deltaTime * 20);
            leftHandRotWeight = Mathf.Lerp(leftHandRotWeight, 0f, Time.deltaTime * 20);

            SetIK(rightHandPos, leftHandPos);
            SetIKRot(Quaternion.identity, Quaternion.identity);
            SetCameraPos(0f);
        }
        else
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0f, Time.deltaTime * 20);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0f, Time.deltaTime * 20);

            rightHandRotWeight = Mathf.Lerp(rightHandRotWeight, 0f, Time.deltaTime * 20);
            leftHandRotWeight = Mathf.Lerp(leftHandRotWeight, 0f, Time.deltaTime * 20);

            SetIK(sightOverlay.position, sightOverlay.position);
            SetIKRot(Quaternion.identity, Quaternion.identity);
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

    private void SetIKRot(Quaternion rightHand, Quaternion leftHand)
    {
        overlayAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHand);
        overlayAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandRotWeight);
        overlayAnimator.SetIKRotation(AvatarIKGoal.LeftHand, leftHand);
        overlayAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandRotWeight);
    }
}
