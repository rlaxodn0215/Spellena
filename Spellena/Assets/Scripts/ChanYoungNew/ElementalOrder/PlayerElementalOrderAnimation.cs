using UnityEngine;
using Photon.Pun;

public class PlayerElementalOrderAnimation : PlayerCommonAnimation, IPunObservable
{
    PlayerElementalOrder playerElementalOrder;
    protected override void Start()
    {
        base.Start();
        playerElementalOrder = transform.root.GetComponent<PlayerElementalOrder>();
    }

    protected override void OnAnimatorIK()
    {
        base.OnAnimatorIK();

        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Skill1Channeling"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.5f, Time.deltaTime * 20);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0.5f, Time.deltaTime * 20);
        }
        else if (animator.GetCurrentAnimatorStateInfo(1).IsName("Skill2Channeling"))
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, 0.5f, Time.deltaTime);
            leftHandWeight = Mathf.Lerp(leftHandWeight, 0f, Time.deltaTime * 20);
        }
        else if (animator.GetCurrentAnimatorStateInfo(1).IsName("Skill5Channeling"))
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
