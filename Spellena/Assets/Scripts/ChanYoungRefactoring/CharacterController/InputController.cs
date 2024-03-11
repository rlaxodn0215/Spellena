using InputData;
using ObserverData;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private FunctionFrame<Vector2> moveLowerDirSender;
    private FunctionFrame<MoveType> moveLowerTypeSender;

    private FunctionFrame<int> skillKeySender;
    private FunctionFrame<InputSide> mouseClickSender;
    private FunctionFrame<Vector2> mouseMoveSender;

    private bool isRunning = false;

    private void Start()
    {
        InitFunctionFrame();
    }

    private void InitFunctionFrame()
    {
        moveLowerDirSender = new FunctionFrame<Vector2>(ObserveType.Send);
        moveLowerDirSender.Data = new Vector2(0f, 0f);
        moveLowerTypeSender = new FunctionFrame<MoveType>(ObserveType.Send);
        moveLowerTypeSender.Data = MoveType.None;


        skillKeySender = new FunctionFrame<int>(ObserveType.Send);
        skillKeySender.Data = -1;

        mouseClickSender = new FunctionFrame<InputSide>(ObserveType.Send);
        mouseClickSender.Data = InputSide.None;
        mouseMoveSender = new FunctionFrame<Vector2>(ObserveType.Send);
        mouseMoveSender.Data = Vector2.zero;
    }

    private void InitObserver()
    {
        GetComponent<LowerObserver>().RaiseFunction(moveLowerDirSender);
        GetComponent<LowerObserver>().RaiseFunction(moveLowerTypeSender);
        GetComponent<UpperObserver>().RaiseFunction(mouseClickSender);
        GetComponent<UpperObserver>().RaiseFunction(mouseMoveSender);
    }

    private void OnMove(InputValue inputValue)
    {
        Vector2 _direction = new Vector2(inputValue.Get<Vector2>().x, inputValue.Get<Vector2>().y);
        moveLowerDirSender.Data = _direction;
    }

    private void OnJump()
    {
        moveLowerTypeSender.Data = MoveType.Jump;
    }

    private void OnMouseButton()
    {
        mouseClickSender.Data = InputSide.Left;
    }

    private void OnMouseButton2()
    {
        mouseClickSender.Data = InputSide.Right;
    }

    private void OnMouseMove(InputValue inputValue)
    {
        mouseMoveSender.Data = inputValue.Get<Vector2>();
    }

    private void OnRun()
    {
        isRunning = !isRunning;
        if (isRunning)
            moveLowerTypeSender.Data = MoveType.Run;
        else
            moveLowerTypeSender.Data = MoveType.None;
    }

    private void OnSkill1()
    {
        skillKeySender.Data = 1;
    }

    private void OnSkill2()
    {
        skillKeySender.Data = 2;
    }

    private void OnSkill3()
    {
        skillKeySender.Data = 3;
    }

    private void OnSkill4()
    {
        skillKeySender.Data = 4;
    }

    private void OnButtonCancel()
    {
        skillKeySender.Data = -1;
    }
}
