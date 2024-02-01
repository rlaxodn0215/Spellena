using UnityEngine;
using Photon.Pun;
using GlobalEnum;

public class PlayerElementalOrderAnimation : PlayerCommonAnimation, IPunObservable
{
    PlayerElementalOrder playerElementalOrder;
    protected override void Start()
    {
        base.Start();
        playerElementalOrder = transform.root.GetComponent<PlayerElementalOrder>();
    }

    protected override void InitUniqueComponents()
    {
        for(int i = 0; i < skillRoutes.Count; i++)
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
                animator.SetBool(_parameter, false);
                Debug.Log("출동");
            }
        }
    }

    protected override void OnAnimatorIK()
    {
        base.OnAnimatorIK();

        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Skill1Casting"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.5f, Time.deltaTime * 20);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0.5f, Time.deltaTime * 20);
        }
        else if (animator.GetCurrentAnimatorStateInfo(1).IsName("Skill2Casting"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.5f, Time.deltaTime);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0f, Time.deltaTime * 20);
        }
        else if (animator.GetCurrentAnimatorStateInfo(1).IsName("Skill5Casting"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.2f, Time.deltaTime * 20);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0f, Time.deltaTime * 20);
        }
        else
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0f, Time.deltaTime * 20);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0f, Time.deltaTime * 20);
        }


        animator.SetIKPosition(AvatarIKGoal.RightHand, handSightMain.position);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, handSightMain.position);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
    }
}
