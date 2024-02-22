using System.Collections.Generic;
using UnityEngine;
using DataObserver;

public class PlayerObserver : MonoBehaviour
{
    private List<DataObserver<int>> dataObserversInt = new List<DataObserver<int>>();
    private List<DataObserver<Vector2>> dataObserversVector2 = new List<DataObserver<Vector2>>();
    public void RaiseDataObserver<T>(DataObserver<T> dataObserver)
    {
        if (typeof(T) == typeof(int))
            dataObserversInt.Add((DataObserver<int>)(object)dataObserver);
        else if (typeof(T) == typeof(Vector2))
            dataObserversVector2.Add((DataObserver<Vector2>)(object)dataObserver);
        else
            return;
        dataObserver.NotifyDataChanged += Notify;
    }

    private void Notify<T>(DataObserver<T> observer)
    {
    }


    public void LowerDataObserver<T>(DataObserver<T> dataObserver)
    {
        dataObserver.NotifyDataChanged -= Notify;
    }

}
