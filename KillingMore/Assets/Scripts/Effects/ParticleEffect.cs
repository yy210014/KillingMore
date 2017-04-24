using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleEffect : SpecialEffect
{

    float mSpeed;
    float mSize;
    bool PlayFinishDestroy;

    List<ParticleSystem> mParticleSystems = new List<ParticleSystem>();

    public List<ParticleSystem> ParticleSystems
    {
        get
        {
            return mParticleSystems;
        }
    }

    public override EffectType GetActorType()
    {
        return EffectType.ParticleSystem;
    }

    public override void Initialize()
    {
        AddAllChilds<ParticleSystem>(mParticleSystems, transform);
        base.Initialize();
        //记录mSpeed mSize
        //    mSize = MainParticleSystem.startSize;
        //    mSpeed = MainParticleSystem.startSpeed;
    }

    public override void OnGameUpdate(float dt)
    {
        base.OnGameUpdate(dt);


    }

    public override void Replay()
    {
        for (int i = 0; i < mParticleSystems.Count; i++)
        {
            mParticleSystems[i].Play();
        }
        ComputeMaxDuration();
        base.Replay();
    }

    public override void SetSize(float size)
    {
        foreach (ParticleSystem ps in mParticleSystems)
        {
            //   ps.startSize *= size;
            ParticleSystem.MainModule main = ps.main;
            main.startSizeMultiplier *= size;
        }
    }

    public override void Destroy()
    {
        mParticleSystems.Clear();
        base.Destroy();
    }

    void ComputeMaxDuration()
    {
        foreach (ParticleSystem ps in mParticleSystems)
        {
            if (ps.emission.enabled)
            {
                if (ps.main.loop)
                {
                    if (mDuration == 0)
                        mDuration = -1f;
                    continue;
                }
                float dunration = 0f;
                if (ps.emission.rateOverTimeMultiplier <= 0)
                {
                    dunration = ps.main.startDelayMultiplier + ps.main.startLifetimeMultiplier;
                }
                else
                {
                    dunration = ps.main.startDelayMultiplier + Mathf.Max(ps.main.duration, ps.main.startLifetimeMultiplier);
                }
                if (dunration > mDuration)
                {
                    mDuration = dunration;
                }
            }
        }
    }
}
