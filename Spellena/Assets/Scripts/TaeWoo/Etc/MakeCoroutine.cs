using UnityEngine;
using System.Collections;

namespace CoroutineMaker
{
    public class MakeCoroutine : MonoBehaviour
    {
        IEnumerator enumerator = null;
        private void Coroutine(IEnumerator coro)
        {
            enumerator = coro;
            StartCoroutine(coro);
        }

        void Update()
        {
            if (enumerator != null)
            {
                if (enumerator.Current == null) // 코루틴의 현재 진행 상태
                {
                    Destroy(gameObject);
                }
            }
        }

        public void Stop()
        {
            StopCoroutine(enumerator.ToString());
            Destroy(gameObject);
        }

        public static MakeCoroutine Start_Coroutine(IEnumerator coro)
        {
            GameObject obj = new GameObject("MakeCoroutine");
            MakeCoroutine handler = obj.AddComponent<MakeCoroutine>();
            if (handler)
            {
                handler.Coroutine(coro);
            }
            return handler;
        }
    }
}