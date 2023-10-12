using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    PlayerInput playerInput;
    public enum PlayerActionState
    {
        Move, Jump, Walk, Sit, Interaction, Skill1, Skill2, Skill3, Skill4
    }

    struct PlayerActionData
    {
        public PlayerActionState playerActionState;
        public InputAction inputAction;
        public bool IsExecuting;
    }

     List<PlayerActionData> playerActionDatas = new List<PlayerActionData>();

    Vector2 moveVec;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        SetPlayerKeys(PlayerActionState.Move, "Move");
        SetPlayerKeys(PlayerActionState.Jump, "Jump");
        SetPlayerKeys(PlayerActionState.Walk, "Walk");
        SetPlayerKeys(PlayerActionState.Sit, "Sit");
        SetPlayerKeys(PlayerActionState.Interaction, "Interaction");
        SetPlayerKeys(PlayerActionState.Skill1, "Skill1");
        SetPlayerKeys(PlayerActionState.Skill2, "Skill2");
        SetPlayerKeys(PlayerActionState.Skill3, "Skill3");
        SetPlayerKeys(PlayerActionState.Skill4, "Skill4");
    }

    void SetPlayerKeys(PlayerActionState playerActionState, string action)
    {
        PlayerActionData _data = new PlayerActionData();
        _data.playerActionState = playerActionState;
        _data.inputAction = playerInput.actions.FindAction(action);
        _data.IsExecuting = false;
        playerActionDatas.Add(_data);
    }


    void OnMove(InputValue value)
    {

    }

    void OnJump(InputValue value)
    {
        
    }

    void OnWalk(InputValue value)
    {

    }

    void OnSit(InputValue value)
    {

    }

    void OnInteraction(InputValue value)
    {

    }

    void OnSkill1(InputValue value)
    {

    }

    void OnSkill2(InputValue value)
    {

    }

    void OnSkill3(InputValue value)
    {

    }

    void OnSkill4(InputValue value)
    {

    }

}
