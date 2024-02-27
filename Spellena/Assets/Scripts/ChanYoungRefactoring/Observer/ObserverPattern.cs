using System;
using System.Collections.Generic;

namespace DataObserver
{
    public class DataListObserver<T>
    {
        private List<T> data = new List<T>();
        public Action<DataListObserver<T>> NotifyDataChanged;

        public void Notify()
        {
            NotifyDataChanged?.Invoke(this);
        }
    }

    public class DataObserver<T>
    {
        public T data;

        public Action<DataObserver<T>> NotifyDataChanged;//옵저버에서 실행됨
        public Action<T> NotifyOtherChanged;//다른 객체가 변경된 것을 확인

        public void Notify()
        {
            NotifyDataChanged?.Invoke(this);
        }

        public void NotifyToOther()
        {
            NotifyOtherChanged?.Invoke(data);
        }
    }
}
