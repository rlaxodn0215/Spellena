using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;

public class Arrow : PoolObject
{
    public int damage = 10;
    public float arrowVelocity = 120.0f;
    public float lifeTiming = 100f;
    private CheckGauge lifeTime;
    private Rigidbody rigidbody;

    public override void InitPoolObject()
    {
        lifeTime = new CheckGauge(lifeTiming);
        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null) Debug.LogError("rigidbody가 할당되지 않았습니다");
    }
    public override void SetPoolObject(Vector3 direction)
    {
        transform.LookAt(direction);
    }

    void OnEnable()
    {
        if (rigidbody != null)
            rigidbody.velocity = transform.forward * arrowVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

    void Update()
    {
        lifeTime.UpdateCurCoolTime();
        CheckDisActive();
    }

    void CheckDisActive()
    {
        if (lifeTime.CheckCoolTime())
        {
            lifeTime.UpdateCurCoolTime(0.0f);
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            DisActive();
        }
    }
}
