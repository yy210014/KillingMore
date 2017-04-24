using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Emitter;

public abstract class GameEvent
{
    // 这里定义游戏事件ID
    public const int GameStart = 1;
    public const int GameEnd = 2;
    public const int SwitchScene = 3;
    public const int ActorCollideWithActor = 4;
    public const int ActorCollideWithProjectile = 5;
    public const int ActorCollideWithLaser = 6;
    public const int SuperManCollideWithItem = 7;
    public const int ActorSpawn = 8;
    public const int ActorDie = 9;
    public const int DoubelClick = 10;
    public const int AllWavesSpawned = 11;
    public const int Restart = 12;
    public const int SpawnWave = 13;
    public const int NotifyTutorialEvent = 14;
    public const int AllWavesClear = 15;


    // 特殊的引导事件
    public const int TutorialConditionVerify = 9999;

    public abstract int GetEventType();

    // internal usage.
    public float Delay
    {
        get;
        set;
    }

    public object Sender
    {
        get;
        set;
    }

    public static T GameEventCast<T>(GameEvent cge, int et) where T : GameEvent
    {
        if (cge.GetEventType() == et)
        {
            return (T)cge;
        }

        return null;
    }
}

public delegate void GameEventDel(GameEvent ge);

public class GameEventProc
{

    public void RegisterHandler(int evtType, GameEventDel del)
    {
        List<GameEventDel> dels = null;
        if (mHandlers.TryGetValue(evtType, out dels))
        {
            if (!dels.Contains(del))
            {
                dels.Add(del);
            }
        }
        else
        {
            dels = new List<GameEventDel>();
            dels.Add(del);
            mHandlers.Add(evtType, dels);
        }
    }

    public void UnregisterHandler(int evtType, GameEventDel del)
    {
        List<GameEventDel> dels = null;
        if (mHandlers.TryGetValue(evtType, out dels))
        {
            dels.Remove(del);
        }
    }

    void DispatchEvent(GameEvent ge)
    {
        List<GameEventDel> dels = null;
        if (mHandlers.TryGetValue(ge.GetEventType(), out dels))
        {
            // 避免在处理事件时进行事件处理器的注册导致迭代问题
            mTempHandlers.Clear();
            mTempHandlers.AddRange(dels);
            foreach (GameEventDel dl in dels)
            {
                if (dl != null)
                {
                    dl(ge);
                }
            }
        }
    }

    public void SendEvent(GameEvent ge, object sender, float delay)
    {
        if (ge != null)
        {
            ge.Delay = delay;
            ge.Sender = sender;

            if (delay <= 0)
            {
                DispatchEvent(ge);
            }
            else
            {
                mBufferedEvent.Add(ge);
            }
        }

    }

    public void DispatchAll()
    {
        mTempEvents.Clear();
        mTempEvents.AddRange(mBufferedEvent);
        float dt = Game.Singleton.RealtimeDelta;

        for (int i = 0; i < mTempEvents.Count; ++i)
        {
            mTempEvents[i].Delay -= dt;
            if (mTempEvents[i].Delay <= 0)
            {
                DispatchEvent(mTempEvents[i]);
            }
        }

        for (int i = 0; i < mBufferedEvent.Count; )
        {
            if (mBufferedEvent[i].Delay <= 0)
            {
                mBufferedEvent.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }

    public void Reset()
    {
        mBufferedEvent.Clear();
        mTempEvents.Clear();
        mTempHandlers.Clear();
        mHandlers.Clear();
    }

    List<GameEvent> mBufferedEvent = new List<GameEvent>();

    // 为了避免在事件处理时注册hanlder导致迭代访问问题
    List<GameEvent> mTempEvents = new List<GameEvent>();
    List<GameEventDel> mTempHandlers = new List<GameEventDel>();

    Dictionary<int, List<GameEventDel>> mHandlers = new Dictionary<int, List<GameEventDel>>();
}

////////////////////////////////////////////////////////////////////////
// 事件定义

// 切换场景
public class SwitchSceneEvent : GameEvent
{
    public override int GetEventType()
    {
        return GameEvent.SwitchScene;
    }
}

// 碰撞
public class ActorCollideWithActor : GameEvent
{
    public override int GetEventType()
    {
        return GameEvent.ActorCollideWithActor;
    }

    public Actor mActor;
    public Actor mOther;
}

public class ActorCollideWithProj : GameEvent
{
    public override int GetEventType()
    {
        return GameEvent.ActorCollideWithProjectile;
    }

    public Actor mActor;
    public Projectile mProj;
}

public class ActorCollideWithLaser : GameEvent
{
    public override int GetEventType()
    {
        return GameEvent.ActorCollideWithLaser;
    }

    public Actor mActor;
    public Projectile mProj;
    public Vector3 mHitPoint;
}

public class WaveSpawnEvent : GameEvent
{
    public override int GetEventType()
    {
        return GameEvent.SpawnWave;
    }

    public int mWaveIndex;
}

public class ActorSpawnEvent : GameEvent
{
    public override int GetEventType()
    {
        return GameEvent.ActorSpawn;
    }

    public Actor mActor;
}

public class ActorDieEvent : GameEvent
{
    public override int GetEventType()
    {
        return GameEvent.ActorDie;
    }

    public Actor mActor;
    public bool Destroy = false;//不走die方法，而是直接销毁,默认为false
}

public class DoubleClickEvent : GameEvent
{
    public override int GetEventType()
    {
        return GameEvent.DoubelClick;
    }
}

public class AllWavesSpawnedEvent : GameEvent
{
    public override int GetEventType()
    {
        return GameEvent.AllWavesSpawned;
    }
}
public class AllWavesDieEvent : GameEvent
{
    public override int GetEventType()
    {
        return GameEvent.AllWavesClear;
    }
}


public class RestartEvent : GameEvent
{
    public int actionType;

    public override int GetEventType()
    {
        return GameEvent.Restart;
    }
}

public class NotifyTutorialEvent : GameEvent
{
    public override int GetEventType()
    {
        return GameEvent.NotifyTutorialEvent;
    }

    public bool mEnterOrLeave;
    public bool mRestoreInput;
}