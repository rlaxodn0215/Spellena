using System;
using System.Collections.Generic;

namespace ObserverData
{
    public enum ObserveType
    {
        Send, Receive
    }

    public class FunctionFrame<T>
    {
        //data���� ����Ǹ� Notify�� �����
        private event Action<T> Notify;
        private ObserveType type;
        private T data;

        public FunctionFrame(ObserveType newType)
        {
            type = newType;
        }

        public ObserveType Type
        {
            get
            {
                return type;
            }
        }

        public T Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                if (type == ObserveType.Send)
                    Notify?.Invoke(data);
            }
        }

        /*
        ��� : Receiver�� ����� �Լ� ����
        ���� ->
        receiverData : Sender�� ���� ����
        */
        public void CallNotify(T receiveData)
        {
            Notify?.Invoke(receiveData);
        }

        /*
        ��� : Receiver�� Observer�� ���� ���� �Լ� ���
        ���� ->
        Notifier : Receiver�� �Լ�
        */
        public void SetNotify(Action<T> Notifier)
        {
            Notify = Notifier;
        }

        /*
        ��� : Notify �ʱ�ȭ
        */
        public void ResetNotify()
        {
            Notify = null;
        }
    }

    public class ObserverFrame<T>
    {
        private List<FunctionFrame<T>> senders = new List<FunctionFrame<T>>();
        private List<FunctionFrame<T>> receivers = new List<FunctionFrame<T>>();


        /*
        Observer�� FunctionFrame�� ���
        ���� -> 
        observer : ����� FunctionFrame
        */
        public void RaiseObserver(FunctionFrame<T> observer)
        {
            if (observer.Type == ObserveType.Send)
            {
                observer.SetNotify(NotifyChanged);
                senders.Add(observer);
            }
            else
                receivers.Add(observer);
        }

        /*
        Observer�� FunctionFrame�� ����
        ���� ->
        observer : ������ FunctionFrame
        */
        public void LowerObserver(FunctionFrame<T> observer)
        {
            if (observer.Type == ObserveType.Send)
            {
                observer.ResetNotify();
                senders.Remove(observer);
            }
            else
                receivers.Remove(observer);
        }

        /*
        ��� : �������� ������ Receiver�鿡�� �˸�
        ���� ->
        data : Sender�� ���� ������
        */
        public void NotifyChanged(T data)
        {
            for(int i = 0; i < receivers.Count; i++)
                receivers[i].CallNotify(data);
        }

    }
}
