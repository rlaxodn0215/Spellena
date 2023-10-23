using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance()
    {
        if (instance == null)
        {
            GameObject newGameObject = new GameObject("GameManager");
            instance = newGameObject.AddComponent<GameManager>();
        }
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public Vector3 GetElementalOrderCameraDistance()
    {
        return new Vector3(0, 0, 0);
    }
}