using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Emitter;

public class Character : Actor
{
    public override ActorType GetActorType()
    {
        return ActorType.Character;
    }

    public override void OnGameInitialize()
    {
        mAnimator = new ActorAnimator();
        mAnimator.Initialize(gameObject);
        mAnimator.loop = false;
        mBehaviorFSM = new ActorBehaviorFSM(this);
        mBehaviorProvider = new CharacterBehaviors();
        mBehaviorProvider.InitializeFSM(mBehaviorFSM);
        mBehaviorFSM.Start(0);
    }

    public override void OnGameUpdate(float dt)
    {
        base.OnGameUpdate(dt);
        //摄像机跟踪超人
        Vector3 p = Vector3.MoveTowards(Camera.main.transform.position, transform.position, dt * 4);
        Camera.main.transform.position = new Vector3(Mathf.Clamp(p.x, -0.7f, 0.7f), Camera.main.transform.transform.position.y, Camera.main.transform.transform.position.z);
    }

    public override void InitEmittersBySubTransfroms()
    {
        mEmitters.Clear();
        foreach (Transform tf in transform)
        {
            AttackStateSchedule ass = new AttackStateSchedule();
            Emitter_Basic be = tf.GetComponent<Emitter_Basic>();
            if (be != null)
            {
                mEmitters.Add(be);
                ass.Emitter = be;
                AddAttackStateSchedule(ass);
            }
        }
    }

    public override void IteraterEmiters(System.Action<Emitter_Basic> act)
    {
        for (int i = 0; i < mIteraterEmitters.Count; ++i)
        {
            act(mIteraterEmitters[i]);
        }
    }

    public void SetAttackStateSchedule(List<AttackStateSchedule> asss)
    {
        mAttackStateSchedules = asss;
    }

    public void AddAttackStateSchedule(AttackStateSchedule ass)
    {
        mAttackStateSchedules.Add(ass);
    }

    public void SwitchBulletEmitte(int index)
    {
        if (mAttackStateSchedules.Count == 0) return;
        if (index > mAttackStateSchedules.Count - 1) index = mAttackStateSchedules.Count - 1;
        for (int i = 0; i < mIteraterEmitters.Count; i++)
        {
            mIteraterEmitters[i].gameObject.SetActive(false);
        }
        mIteraterEmitters.Clear();
        mAttackStateSchedules[index].Emitter.gameObject.SetActive(true);
        mAttackStateSchedules[index].Emitter.Initialize();
        mIteraterEmitters.Add(mAttackStateSchedules[index].Emitter);
        Force.Value = mAttackStateSchedules[index].Force;
    }

    public void OnHit(double damage)
    {
        if (IsDead || damage == 0)
        {
            return;
        }
        Hp.Value -= (float)damage;
    }

    public  void CreateDieEffect()
    {
    }

    List<Emitter_Basic> mIteraterEmitters = new List<Emitter_Basic>();
    List<AttackStateSchedule> mAttackStateSchedules = new List<AttackStateSchedule>();
}

public class AttackStateSchedule
{
    public Emitter_Basic Emitter;
    public float Force;
}
