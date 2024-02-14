using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Managers;
using DefineDatas;

public class Arrow : PoolObject
{
    enum ArrowCoolTime
    {
        LifeTime,
        HitParticleDestoryTime,
        ArrowStuckDestoryTime
    }

    public int damage;
    public float arrowVelocity;
    public float rotateSpeed;
    public bool ableHitParticle;
    public bool ableArrowStuck;

    public float[] times;
    public PoolObjectName hitEffectName;
    public PoolObjectName stuckObjectName;

    private List<CoolTimer> timers = new List<CoolTimer>();
    private new Rigidbody rigidbody;
    private Coroutine lifeTimeCoroutine;

    public override void InitPoolObject()
    {
        for (int i = 0; i < times.Length; i++) timers.Add(new CoolTimer(times[i]));
        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null) rigidbody = gameObject.AddComponent<Rigidbody>();
    }
    public override void SetPoolObjectTransform(Transform trans)
    {
        transform.LookAt(trans.position);
    }
    void OnEnable()
    {
        if (ObjID > 0)
        {
            if (rigidbody == null) rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.velocity = transform.forward * arrowVelocity;
            lifeTimeCoroutine = StartCoroutine(TimerUpdate(ArrowCoolTime.LifeTime, this));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.name == gameObject.name) return;       
        if (ableArrowStuck)  MakeHitEffect(stuckObjectName,collision, true);
        if (ableHitParticle) MakeHitEffect(hitEffectName,collision, false);
        if (collision.transform.CompareTag(TagName.TeamA)) PlayerAttack(collision);
        DisActive();
    }

    void MakeHitEffect(PoolObjectName name, Collision collision, bool isFollowHitObject)
    {
        PoolObject ob = PoolManager.Instance.
            GetObject(CharacterName.Character_2, name, collision.GetContact(0).point, transform.rotation);
        if (isFollowHitObject)
        {
            Vector3 distance = collision.GetContact(0).point - collision.transform.position;
            ob.StartCoroutine(FollowHitObject(collision.transform, ob, distance));
        }
        else
        {
            ob.StartCoroutine(TimerUpdate(ArrowCoolTime.HitParticleDestoryTime, ob));
        }
    }

    IEnumerator TimerUpdate(ArrowCoolTime name, PoolObject ob)
    {
        while (!timers[(int)name].IsCoolTimeFinish())
        {
            timers[(int)name].UpdateCoolTime(Time.deltaTime);
            if(name == ArrowCoolTime.LifeTime)
                ob.transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime);
            yield return null;
        }

        timers[(int)name].ChangeCoolTime(DefineNumber.ZeroCount);
        ob.DisActive();
    }

    IEnumerator FollowHitObject(Transform hitObj, PoolObject stuckObj, Vector3 distance)
    {
        while (!timers[(int)ArrowCoolTime.ArrowStuckDestoryTime].IsCoolTimeFinish())
        {
            timers[(int)ArrowCoolTime.ArrowStuckDestoryTime].UpdateCoolTime(Time.deltaTime);
            stuckObj.transform.position = hitObj.position + distance;
            yield return null;
        }

        timers[(int)ArrowCoolTime.ArrowStuckDestoryTime].ChangeCoolTime(DefineNumber.ZeroCount);
        stuckObj.DisActive();
    }

    void PlayerAttack(Collision collision)
    {
        Character enemyCharacter = collision.transform.GetComponent<Character>();
        if (enemyCharacter == null) return;
        enemyCharacter.PlayerDamaged(CharacterName.Character_2, damage, null, Vector3.zero, 0.0f);
    }

    public override void DisActive()
    {
        StopCoroutine(lifeTimeCoroutine);
        timers[(int)ArrowCoolTime.LifeTime].ChangeCoolTime(DefineNumber.ZeroCount);
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        base.DisActive();
    }
}
