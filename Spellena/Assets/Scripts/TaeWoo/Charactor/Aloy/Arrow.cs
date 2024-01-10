using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;

public class Arrow : PoolObject
{
    public int damage;
    public float arrowVelocity;
    public float lifeTiming;
    public float maxLength;
    public float rotateSpeed;

    public bool ableHit;
    public GameObject hitObject;
    private GameObject makeHitObject;
    public float hitObjectDestoryTime;

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

    public override void SetHitObject(GameObject _hitObject)
    {
        hitObject = _hitObject;
    }

    void OnEnable()
    {
        if (rigidbody != null)
            rigidbody.velocity = transform.forward * arrowVelocity;
    }

    void Update()
    {
        lifeTime.UpdateCurCoolTime();

        Rotate();
        CheckDisActive();
    }


    void Rotate()
    {
        transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ableHit)
        {
            if (collision.transform.name != gameObject.name)
                HitObj(collision);
        }
    }

    void HitObj(Collision collision)
    {
        DisActive();
        makeHitObject = Instantiate(hitObject, collision.GetContact(0).point, Quaternion.identity);
        Destroy(makeHitObject, hitObjectDestoryTime);
    }

    void CheckDisActive()
    {
        if (lifeTime.CheckCoolTime())
        {
            DisActive();
        }
    }

    protected override void DisActive()
    {
        lifeTime.UpdateCurCoolTime(0.0f);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        base.DisActive();
    }
}
