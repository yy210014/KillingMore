using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorBehavior
{
    public const int OverrideBehaviorType = 1000000;

    public int Behavior
    {
        get { return mBehaviorType; }
    }

    public string Name
    {
        get { return mName; }
    }

    public void Start(Actor p, ActorBehavior lastBehv)
    {
        if (mBehaviorStartDel != null)
        {
            mBehaviorStartDel(p, lastBehv);
        }

        mTimer = 0;
    }

    // 当返回值不等于该行为本身的BehaviorType，则状态机会切换到返回值对应的behavior
    public int Update(Actor p)
    {
        if (mBehaviorUpdateDel != null)
        {
            mTimer += GameScene.Singleton.TimeDelta;
            return mBehaviorUpdateDel(p);
        }

        return Behavior;
    }

    public void End(Actor p, ActorBehavior nextBehv)
    {
        if (mBehaviorEndDel != null)
        {
            mBehaviorEndDel(p, nextBehv);
        }
    }

    public int OnAnimEvent(Actor p, string animName, int param)
    {
        if (mAnimEventDel != null)
        {
            return mAnimEventDel(p, animName, param);
        }

        return Behavior;
    }

    public float Timer_
    {
        get
        {
            return mTimer;
        }
    }

    public delegate int UpdateDel(Actor p);
    public delegate void StartDel(Actor p, ActorBehavior last);
    public delegate void EndDel(Actor p, ActorBehavior next);
    public delegate int AnimEventDel(Actor p, string animName, int param);

    int mBehaviorType;
    StartDel mBehaviorStartDel = null;
    UpdateDel mBehaviorUpdateDel = null;
    EndDel mBehaviorEndDel = null;
    AnimEventDel mAnimEventDel = null;
    string mName;
    float mTimer;

    public ActorBehavior(int behv, StartDel startDel,
        UpdateDel updateDel, EndDel endDel, AnimEventDel animDel, string name)
    {
        mBehaviorType = behv;
        mBehaviorStartDel = startDel;
        mBehaviorUpdateDel = updateDel;
        mBehaviorEndDel = endDel;
        mAnimEventDel = animDel;
        mName = name;
    }
}

//  用于路径中设置行为
public enum PathBehavior
{
    Move = 0,
    Idle,
    Attack,
    MoveNAttack,

    // TODO
}

public class ActorBehaviorFSM
{
    public int CurrentBehavior
    {
        get
        {
            return mCurrentBehaivor != null ? mCurrentBehaivor.Behavior : Actor.InvalidActorBehaviorType;
        }
    }

    public string CurrentBehaviorName
    {
        get { return mCurrentBehaivor != null ? mCurrentBehaivor.Name : "None"; }
    }

    public ActorBehavior OverrideBehavior
    {
        get
        {
            return mOverrideBehavior;
        }
    }

    public ActorBehavior CurrentBehaviorInst
    {
        get
        {
            return mCurrentBehaivor;
        }
    }

    public void InitialAddBehavior(ActorBehavior dpb)
    {
        if (mBehaviors.Contains(dpb))
        {
            return;
        }

        mBehaviors.Add(dpb);
    }

    public void SetOverrideBehavior(ActorBehavior bh)
    {
        mOverrideBehavior = bh;
        if (mOverrideBehavior != null)
        {
            if (mCurrentBehaivor != mOverrideBehavior)
            {
                if (mCurrentBehaivor != null)
                {
                    mCurrentBehaivor.End(mActor, mOverrideBehavior);
                }

                ActorBehavior lastBehv = mCurrentBehaivor;
                mCurrentBehaivor = mOverrideBehavior;
                mOverrideBehavior.Start(mActor, lastBehv);
            }
        }
    }

    public void Start(int defaultType)
    {
        mDefaultBehavior = GetBehavior(defaultType);
    }

    public ActorBehavior GetBehavior(int type)
    {
        foreach (ActorBehavior dbp in mBehaviors)
        {
            if (dbp.Behavior == type)
            {
                return dbp;
            }
        }

        return null;
    }

    // 注意：不要在brehavior的start, update, end过程中调用
    public void Switch2(int bt, bool voidRepeat)
    {
        ActorBehavior bh = GetBehavior(bt);
        if (bh == null)
        {
            return;
        }

        if (bh == mCurrentBehaivor)
        {
            if (!voidRepeat)
            {
                mCurrentBehaivor.End(mActor, mCurrentBehaivor);
                mCurrentBehaivor.Start(mActor, mCurrentBehaivor);
            }

            return;
        }

        if (mCurrentBehaivor != null)
        {
            mCurrentBehaivor.End(mActor, bh);
        }

        ActorBehavior lastBehv = mCurrentBehaivor;
        mCurrentBehaivor = bh;
        mCurrentBehaivor.Start(mActor, lastBehv);
    }

    void SwitchByRet(int ret)
    {
        if (ret != mCurrentBehaivor.Behavior)
        {
            if (mCurrentBehaivor == mOverrideBehavior)
            {
                // pop override behavior.
                mOverrideBehavior = null;
            }

            if (ret == Actor.InvalidActorBehaviorType
                && mDefaultBehavior != null)
            {
                ret = mDefaultBehavior.Behavior;
            }

            ActorBehavior nextBh = GetBehavior(ret);
            if (nextBh == null)
            {
                nextBh = mDefaultBehavior;
            }

            if (nextBh != null)
            {
                Switch2(nextBh.Behavior, true);
            }
            else
            {
                mCurrentBehaivor = null;
                Game.LogError("No valid behavior to switch to.");
            }
        }
    }

    public void OnAnimEvent(string animName, int param)
    {
        if (mCurrentBehaivor != null)
        {
            int ret = mCurrentBehaivor.OnAnimEvent(mActor, animName, param);
            SwitchByRet(ret);
        }
    }

    public void UpdateBehaviors()
    {
        if (mCurrentBehaivor == null)
        {
            if (mDefaultBehavior != null)
            {
                mCurrentBehaivor = mDefaultBehavior;
                mCurrentBehaivor.Start(mActor, null);
            }

            return;
        }

        int ret = mCurrentBehaivor.Update(mActor);
        SwitchByRet(ret);
    }

    public void Reset()
    {
        mBehaviors.Clear();
    }

    //public ActorAnimationTrigger AnimTrigger
    //{
    //    get { return mAnimTrigger; }
    //}

    public ActorBehaviorFSM(Actor ac)
    {
        mActor = ac;
        //mAnimTrigger = new ActorAnimationTrigger(ac);
    }

    ActorBehavior mOverrideBehavior = null;
    ActorBehavior mCurrentBehaivor = null;
    ActorBehavior mDefaultBehavior = null;
    List<ActorBehavior> mBehaviors = new List<ActorBehavior>();
    Actor mActor;
    //ActorAnimationTrigger mAnimTrigger;
}

public abstract class ActorBehaivorProvider
{
    public abstract bool InitializeFSM(ActorBehaviorFSM fsm);

    public abstract void Reset();
}