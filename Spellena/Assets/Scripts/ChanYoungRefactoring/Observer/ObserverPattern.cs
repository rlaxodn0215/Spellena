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

        public Action<DataObserver<T>> NotifyDataChanged;//���������� �����
        public Action<T> NotifyOtherChanged;//�ٸ� ��ü�� ����� ���� Ȯ��

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
