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
        //data값이 변경되면 Notify가 실행됨
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
        기능 : Receiver가 등록함 함수 실행
        인자 ->
        receiverData : Sender가 보낸 정보
        */
        public void CallNotify(T receiveData)
        {
            Notify?.Invoke(receiveData);
        }

        /*
        기능 : Receiver가 Observer로 부터 받을 함수 등록
        인자 ->
        Notifier : Receiver의 함수
        */
        public void SetNotify(Action<T> Notifier)
        {
            Notify = Notifier;
        }

        /*
        기능 : Notify 초기화
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
        Observer에 FunctionFrame을 등록
        인자 -> 
        observer : 등록할 FunctionFrame
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
        Observer에 FunctionFrame을 제거
        인자 ->
        observer : 제거할 FunctionFrame
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
        기능 : 데이터의 변경을 Receiver들에게 알림
        인자 ->
        data : Sender가 보낸 데이터
        */
        public void NotifyChanged(T data)
        {
            for(int i = 0; i < receivers.Count; i++)
                receivers[i].CallNotify(data);
        }

    }
}
