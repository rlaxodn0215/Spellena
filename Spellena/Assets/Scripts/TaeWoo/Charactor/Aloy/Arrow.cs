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

    private CoolTimer lifeTime;
    private CoolTimer stuckTime;
    private new Rigidbody rigidbody;
    private Coroutine lifeTimeCoroutine;

    public override void InitPoolObject()
    {
        lifeTime = new CoolTimer(lifeTiming);
        stuckTime = new CoolTimer(arrowStuckDestoryTime);
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
            lifeTime.UpdateCoolTime(Time.deltaTime);
            transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime);
            yield return null;
        }

        StopCoroutine(lifeTimeCoroutine);
        DisActive();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.name == gameObject.name) return;       
        if (ableArrowStuck)  MakeHitEffect(stuckObjectName,collision, true);
        if (ableHitParticle) MakeHitEffect(hitEffectName,collision, false);
        StopCoroutine(lifeTimeCoroutine);
        DisActive();
    }

    void MakeHitEffect(PoolObjectName name, Collision collision, bool isFollowHitObject)
    {
        PoolObject ob = PoolManager.Instance.GetObject
                (name, collision.GetContact(0).point, transform.rotation);
        if (isFollowHitObject)
        {
            Vector3 distance = collision.GetContact(0).point - collision.transform.position;
            ob.StartCoroutine(FollowHitObject(collision.transform, ob, distance));
        }
        else
        {
            ob.DisActive(hitParticleDestoryTime);
        }
    }

    IEnumerator FollowHitObject(Transform hitObj, PoolObject stuckObj, Vector3 distance)
    {
        while (!stuckTime.IsCoolTimeFinish())
        {
            stuckTime.UpdateCoolTime(Time.deltaTime);
            stuckObj.transform.position = hitObj.position + distance;
            yield return null;
        }

        stuckObj.DisActive();
    }

    public override void DisActive()
    {
        lifeTime.ChangeCoolTime(0.0f);
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        base.DisActive();
    }
}
