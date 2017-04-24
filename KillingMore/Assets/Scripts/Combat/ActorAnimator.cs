using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimator
{
    public Animator Animator { get; private set; }

    bool mLoop;
    public int CurrentAnimState
    {
        get
        {
            return mCurrentState;
        }
    }

    public float PlaybackSpeed
    {
        get
        {
            if (Animator != null) return Animator.playbackTime;
            return 1;
        }
        set
        {
            Animator.playbackTime = value;
        }
    }

    public virtual bool loop
    {
        get { return mLoop; }
        set
        {
            mLoop = value;
        }
    }

    public virtual void Initialize(Animator animator)
    {
        Animator = animator;
    }

    public virtual void ResetPlayback()
    {
        mCurrentState = 0;
    }

    public virtual void SwitchAnimation(int st, float playbackSpeed = 1.0f)
    {
        if (Animator != null && mCurrentState != st)
        {
            Animator.Play(GetAnimNameByState(st));
            mCurrentState = st;
            // Animator.StartPlayback();
            //    PlaybackSpeed = playbackSpeed;
        }
    }

    public virtual void PlayAnimation(string animName, bool loop)
    {
    }

    public virtual void SyncGameSceneTimeScale()
    {
    }

    public virtual string GetAnimNameByState(int st)
    {
        switch (st)
        {
            case CharacterBehaviors.IDLE:
                return "Idle";
            case CharacterBehaviors.MOVE:
                return "Move";
        }
        return Misc.String_Unknown;
    }

    protected int mCurrentState = Actor.InvalidActorBehaviorType;
}
