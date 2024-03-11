using InputData;
using ObserverData;
using UnityEngine;

public class LowerObserver : MonoBehaviour
{
    private ObserverFrame<Vector2> lowerDirObserver;
    private ObserverFrame<MoveType> lowerTypeObserver;

    public void RaiseFunction(FunctionFrame<Vector2> frame)
    {
        lowerDirObserver.RaiseObserver(frame);
    }

    public void RaiseFunction(FunctionFrame<MoveType> frame)
    {
        lowerTypeObserver.RaiseObserver(frame);
    }

    public void LowerFunction(FunctionFrame<Vector2> frame)
    {
        lowerDirObserver.LowerObserver(frame);
    }

    public void LowerFunction(FunctionFrame<MoveType> frame)
    {
        lowerTypeObserver.LowerObserver(frame);
    }
}
