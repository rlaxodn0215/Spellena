using System.Collections;
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
    public float hitParticleDestoryTime;
    public float arrowStuckDestoryTime;
    public PoolObjectName hitEffectName;
    public PoolObjectName stuckObjectName;

    private Gauge lifeTime;
    private new Rigidbody rigidbody;
    private Coroutine lifeTimeCoroutine;

    public override void InitPoolObject()
    {
        lifeTime = new Gauge(lifeTiming);
        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null) 
            rigidbody = gameObject.AddComponent<Rigidbody>();
    }
    public override void SetPoolObjectTransform(Transform trans)
    {
        transform.LookAt(trans.position);
    }
    void OnEnable()
    {
        if (rigidbody != null)
        {
            rigidbody.velocity = transform.forward * arrowVelocity;
            lifeTimeCoroutine = StartCoroutine(LifeTimeUpdate());
        }
    }

    IEnumerator LifeTimeUpdate()
    {
        while(!lifeTime.IsCoolTimeFinish())
        {
            lifeTime.UpdateCurCoolTime(Time.deltaTime);
            transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime);
            yield return null;
        }

        StopCoroutine(lifeTimeCoroutine);
        DisActive();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.name == gameObject.name) return;       
        if (ableArrowStuck)  MakeArrowStuck(collision);
        if (ableHitParticle) MakeHitParticle(collision);
        StopCoroutine(lifeTimeCoroutine);
        DisActive();
    }

    void MakeHitParticle(Collision collision)
    {
        PoolObject ob = PoolManager.Instance.
            GetObject(hitEffectName, collision.GetContact(0).point, Quaternion.identity);
        ob.DisActive(hitParticleDestoryTime);
    }

    void MakeArrowStuck(Collision collision)
    {
        PoolObject ob = PoolManager.Instance.
            GetObject(stuckObjectName, collision.GetContact(0).point, Quaternion.identity);
        ob.transform.parent = collision.transform;
        ob.DisActive(arrowStuckDestoryTime);
    }

    public override void DisActive()
    {
        lifeTime.ChangeCurCoolTime(0.0f);
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        base.DisActive();
    }
}
