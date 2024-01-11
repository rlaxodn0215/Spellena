using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;

public class MakeMutipleObject : MonoBehaviour
{
    public float startDelay;
    public int makeCount;
    public float makeDelay;
    public Vector3 randomPos;
    public Vector3 randomRot;

    private PoolManager poolManager;

    float time1;
    float time2;
    int count;

    void OnEnable()
    {
        time1 = time2 = Time.time;
        count = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        poolManager = GetComponent<PoolManager>();
        if (poolManager == null) Debug.LogError("No PoolManager");
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > time1 + startDelay)
        {
            if (Time.time > time2 + makeDelay && count < makeCount)
            {
                Vector3 pos = transform.position + GetRandomVector(randomPos);
                Quaternion rot = transform.rotation * Quaternion.Euler(GetRandomVector(randomRot));
                poolManager.GetObject(pos, rot);
                time2 = Time.time;
                count++;
            }
        }
    }

    public float GetRandomValue(float value)
    {
        return Random.Range(-value, value);
    }

    public Vector3 GetRandomVector(Vector3 value)
    {
        Vector3 result;
        result.x = GetRandomValue(value.x);
        result.y = GetRandomValue(value.y);
        result.z = GetRandomValue(value.z);
        return result;
    }
}
