using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerElementalOrderOverlayAnimation : MonoBehaviour
{
    PlayerElementalOrder playerElementalOrder;
    private Animator overlayAnimator;
    private PlayerData playerData;
    private void Start()
    {
        InitCommonComponents();
    }

    private void InitCommonComponents()
    {
        playerElementalOrder = transform.root.GetComponent<PlayerElementalOrder>();
        playerElementalOrder.PlayAnimation += SetAnimationParameter;
        overlayAnimator = GetComponent<Animator>();
        playerData = playerElementalOrder.playerData;
    }


    private void SetAnimationParameter(int index)
    {
    }

    private void OnAnimatorIK()
    {
    }

}
