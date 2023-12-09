using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonTemplate<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null) //인스턴스가 없으면
            {
                instance = (T)FindObjectOfType(typeof(T)); //찾는다

                if (instance == null) //그래도 없으면 생성
                {
                    GameObject obj = new GameObject(typeof(T).Name, typeof(T));
                    instance = obj.GetComponent<T>();
                }

            }

            return instance;
        }
    }

    public virtual void Awake()
    {
        if(instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }

        else
        {
            Destroy(gameObject);
        }

        if (transform.parent != null && transform.root != null) // 매니저가 자식으로 묶여있을 때
        {
            DontDestroyOnLoad(this.transform.root.gameObject);
        }

        else
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}