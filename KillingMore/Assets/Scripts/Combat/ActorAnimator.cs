﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimator
{
    public const int UP = 0;
    public const int DOWN = 1;
    public const int LEFT = 2;
    public const int RIGHT = 3;
    public const int UP_LEFT = 4;
    public const int UP_RIGHT = 5;

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

    public virtual void SwitchAnimation(int st, int dir = -1, float playbackSpeed = 1.0f)
    {
        if (Animator != null && (mCurrentState != st || mCurrentDirection != dir))
        {
            if (dir == -1)
            {
                Animator.Play(GetAnimNameByState(st));
            }
            else
            {
                Animator.Play(GetAnimNameByState(st) + "_" + GetAnimNameByDirection(dir));
            }
            mCurrentState = st;
            mCurrentDirection = dir;
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

    public virtual string GetAnimNameByDirection(int dir)
    {
        switch (dir)
        {
            case UP:
                return "Up";
            case DOWN:
                return "Down";
            case LEFT:
                return "Left";
            case RIGHT:
                return "Right";
            case UP_LEFT:
                return "Up_Left";
            case UP_RIGHT:
                return "Up_Right";
                
        }
        return "";
    }

    protected int mCurrentState = Actor.InvalidActorBehaviorType;
    protected int mCurrentDirection = Actor.InvalidActorBehaviorType;
}
