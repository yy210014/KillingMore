using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//施放一个技能总体需要五步：引导->施放->放完->技能效果->技能放完
public class Ability : BasicAbility
{
    public AbilityEventListener OnSpellChannel;
    public AbilityEventListener OnSpellCast;
    public AbilityEventListener OnEndCast;
    public AbilityEventListener OnSpellEffect;
    public AbilityEventListener OnSpellFinish;
    public AbilityEventListener OnSpellStop;

    protected List<string> mAbilityEvents = new List<string>();

    public Ability(Actor owner)
        : base(owner)
    {
        OnSpellChannel = new AbilityEventListener(this);
        OnSpellCast = new AbilityEventListener(this);
        OnEndCast = new AbilityEventListener(this);
        OnSpellEffect = new AbilityEventListener(this);
        OnSpellFinish = new AbilityEventListener(this);
        OnSpellStop = new AbilityEventListener(this);
    }

    public float CastingTime
    {
        get { return mCastingTime; }
    }

    public float Cooldown
    {
        get { return mCooldown; }
    }

    public float Duration
    {
        get { return mDuration; }
    }

    public bool IsCD()
    {
        return mCastTime != 0;
    }

    public void SetAudioName(string name)
    {
        mAudioName = name;
    }
    /// <summary>
    /// 设置施法动作
    /// </summary>
    /// <param name="name"></param>
    public void SetAnimationName(string name)
    {
        mAnimationName = name;
    }

    /// <summary>
    /// 设置施法时间
    /// </summary>
    /// <param name="time"></param>
    public void SetCastingTime(float time)
    {
        mCastingTime = time;
    }

    /// <summary>
    /// 设置冷却时间
    /// </summary>
    /// <param name="time"></param>
    public void SetCooldown(float time)
    {
        mCooldown = time;
        mCastTime = GameScene.Singleton.Elapsed;
    }

    public void SetDuration(float time)
    {
        mDuration = time;
    }

    public override void OnGameUpdate(float dt)
    {
        base.OnGameUpdate(dt);
        if (mMainAction != null)
        {
            mMainAction(dt);
        }

        if (mCastTime != 0)
        {
            float ca = CooldownAmount();
            if (ca < 0)
            {
                mCastTime = 0;
            }
        }
    }

    float CooldownAmount()
    {
        if (Cooldown == 0) return 0;
        return 1 - (GameScene.Singleton.Elapsed - mCastTime) / Cooldown;
    }

    protected virtual void ApplyChannel(float dt)
    {
        mTimer += dt;
        if (mTimer >= mCastingTime)
        {
            SpellCast();
            return;
        }
        //引导过程中如果被打断则终止施放技能,比如死亡，眩晕状态
        if (Owner.IsDead && !Owner.gameObject.activeSelf)
        {
            Stop();
        }
    }

    void ApplyCast(float dt)
    {
        mTimer += dt;
        if (mTimer >= 0)
        {
            EndCast();
            return;
        }
        //施法过程中如果被打断则终止施放技能,比如死亡，眩晕状态
        if (Owner.IsDead && !Owner.gameObject.activeSelf)
        {
            Stop();
        }
    }

    void ApplySpell(float dt)
    {
        mTimer += dt;
        if (mTimer >= mDuration)
        {
            SpellFinish();
            return;
        }
        //施法过程中如果被打断则终止施放技能,比如死亡，眩晕状态
        if (Owner.IsDead && !Owner.gameObject.activeSelf)
        {
            Stop();
        }
    }

    /// <summary>
    /// 重置冷却时间,未实现
    /// </summary>
    public void ResetCooldown()
    {
    }

    public void Cast()
    {
        if (IsCD()) return;
        mTimer = 0;
        SpellChannel();
    }

    protected virtual void SpellChannel()
    {
        if (OnSpellChannel != null)
        {
            OnSpellChannel.Call();
        }
        if (Cooldown > 0)
        {
            mCastTime = GameScene.Singleton.Elapsed;//开始计算cd
        }
        mMainAction = ApplyChannel;
    }

    protected virtual void SpellCast()
    {
        if (OnSpellCast != null)
        {
            OnSpellCast.Call();
        }
        mMainAction = ApplyCast;
        //改成获取施法动作，得到播放结束的回调->EndCast
    }

    /// <summary>
    /// 施法动作播放结束后的回调,目前用0代替
    /// </summary>
    protected virtual void EndCast()
    {
        mMainAction = null;
        if (OnEndCast != null)
        {
            OnEndCast.Call();
        }
        SpellEffect();
    }

    protected virtual void SpellEffect()
    {
        if (OnSpellEffect != null)
        {
            OnSpellEffect.Call();
        }
        //有持续时间的技能由MainAction控制更新
        if (mDuration != -1)
        {
            mMainAction = ApplySpell;
        }
        else
        {
            //  SpellFinish();//没有持续时间的技能需要自己调用SpellFinish
        }
    }

    protected virtual void SpellFinish()
    {
        if (OnSpellFinish != null)
        {
            OnSpellFinish.Call();
        }
        Stop();
    }

    public virtual void Stop()
    {
        if (OnSpellStop != null)
        {
            OnSpellStop.Call();
        }
        mMainAction = null;
        mCurAction = null;
        mTimer = 0;
    }

    System.Action<float> mMainAction;//主方法
    protected float mTimer;
    string mAudioName;
    string mAnimationName;
    protected float mCastingTime;
    float mCooldown;
    float mDuration = -1;
    float mCastTime;
}
