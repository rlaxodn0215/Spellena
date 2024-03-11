using ObserverData;
using InputData;
using UnityEngine;
using AnimationDataRelated;

public class UpperObserver : MonoBehaviour
{
    private ObserverFrame<int> skillSetObserver = new ObserverFrame<int>();
    private ObserverFrame<InputSide> mouseClickObserver = new ObserverFrame<InputSide>();
    private ObserverFrame<Vector2> mouseMoveObserver = new ObserverFrame<Vector2>();
    private ObserverFrame<AnimationData> animationObserver = new ObserverFrame<AnimationData>();

    public void RaiseFunction(FunctionFrame<int> frame)
    {
        skillSetObserver.RaiseObserver(frame);
    }

    public void RaiseFunction(FunctionFrame<InputSide> frame)
    {
        mouseClickObserver.RaiseObserver(frame);
    }

    public void RaiseFunction(FunctionFrame<Vector2> frame)
    {
        mouseMoveObserver.RaiseObserver(frame);
    }

    public void RaiseFunction(FunctionFrame<AnimationData> frame)
    {
        animationObserver.RaiseObserver(frame);
    }

    public void LowerFunction(FunctionFrame<int> frame)
    {
        skillSetObserver.LowerObserver(frame);
    }

    public void LowerFunction(FunctionFrame<InputSide> frame)
    {
        mouseClickObserver.LowerObserver(frame);
    }

    public void LowerFunction(FunctionFrame<Vector2> frame)
    {
        mouseMoveObserver.LowerObserver(frame);
    }

    public void LowerFunction(FunctionFrame<AnimationData> frame)
    {
        animationObserver.LowerObserver(frame);
    }    
}
