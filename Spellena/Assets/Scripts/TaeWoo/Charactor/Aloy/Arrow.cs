using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;

public class Arrow : PoolObject
{
    public int damage;
    public float arrowVelocity;
    public float lifeTiming;
    public float rotateSpeed;

    public bool ableHitParticle;
    public bool ableArrowStuck;
    public bool isUltimate;

    public GameObject hitParticleObject;
    public GameObject arrowStuckObject;

    private GameObject makeHitParticleObject;
    private GameObject makeArrowStuckObject;

    public float hitParticleDestoryTime;
    public float arrowStuckDestoryTime;

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
        if (collision.transform.name != gameObject.name)
        {
            if(isUltimate) DisActive();

            if (ableArrowStuck)
            {
                MakeArrowStuck(collision);
            }

            DisActive();

            if (ableHitParticle)
            {
                MakeHitParticle(collision);
            }

            DisActive();
        }
    }

    void MakeHitParticle(Collision collision)
    {
        makeHitParticleObject = Instantiate(hitParticleObject, collision.GetContact(0).point, Quaternion.identity);
        Destroy(makeHitParticleObject, hitParticleDestoryTime);
    }

    void MakeArrowStuck(Collision collision)
    {
        makeArrowStuckObject = Instantiate(arrowStuckObject, collision.GetContact(0).point, transform.rotation);
        makeArrowStuckObject.transform.parent = collision.transform;
        Destroy(makeArrowStuckObject, arrowStuckDestoryTime);
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
