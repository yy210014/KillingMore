using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Emitter;

public class ActorData
{
    public delegate void ChangeValueDelegate(float value);
    public event ChangeValueDelegate OnChangeValue;
    //基础数值
    public float Value
    {
        get
        {
            return mClampedValue.Value;
        }
        set
        {
            mClampedValue.Reset(Mathf.Clamp(value, 0, MaxValue == 0 ? value : MaxValue));
            if (OnChangeValue != null)
            {
                OnChangeValue(value);
            }
        }
    }
    //加成数值
    public float IncreaseValue { get; set; }
    //最大数值
    public float MaxValue { get { return mClampedValue.Max; } set { mClampedValue.Reset(0, value, value); } }
    //当前数值
    public float CurrentValue { get { return Value + IncreaseValue; } }

    FloatClampedRange mClampedValue = new FloatClampedRange();
}
public class Actor : MonoBehaviour
{
    public enum ActorType
    {
        Invalid = 0,
        Character,
        Enemy,
        Obstacle,
        // TODO
    }

    public const int PlayerTeamFactionId = 0;
    public const int EnemyTeamFactionId = 1;
    public const int NeutralTeamFactionId = 2;
    public const int UnknownFactionId = -1;
    // 区别于策划表伤默认为无效的-1
    public const int InvalidActorBehaviorType = -2;

    protected Dictionary<System.Type, BasicAbility> mAbilityMap = new Dictionary<System.Type, BasicAbility>();

    public Dictionary<string, object> value = new Dictionary<string, object>();//内置字典类，可用来绑定一个自定义数据
    public ActorPathMotor Locomotion { get { return mLocomotion; } }
    public ActorBehaviorFSM BehaviorFSM { get { return mBehaviorFSM; } }
    public ActorAnimator ActorAnimator_ { get { return mAnimator; } }
    //攻防体 移动速度
    public ActorData Force { get { return mForce; } set { mForce = value; } }
    public ActorData Def { get { return mDef; } set { mDef = value; } }
    public ActorData Hp { get { return mHp; } set { mHp = value; } }
    public float Speed = 2f;          

    public int FactionId { get { return mFactionId; } }
    public bool TheSameFactionOf(Actor ac) { return FactionId == ac.FactionId; }
    public bool NotTheSameFactionOf(Actor ac) { return FactionId != ac.FactionId; }
    public bool IsInPlayerTeam { get { return mFactionId == PlayerTeamFactionId; } }
    public bool IsInEnemyTeam { get { return mFactionId == EnemyTeamFactionId; } }
    public bool IsAlive { get { return Hp.Value > 0; } }
    public bool IsDead { get { return Hp.Value <= 0; } }
    public float Depth { get { return mDepth; } }
    public float SelfTimeScale { get { return mSelfTimeScale; } set { mSelfTimeScale = Mathf.Clamp(value, 0, 99.0f); } }

    public T GetAbility<T>() where T : BasicAbility
    {
        BasicAbility ability = null;
        if (mAbilityMap.TryGetValue(typeof(T), out ability))
        {
            return (T)ability;
        }
        return null;
    }

    public virtual ActorType GetActorType()
    {
        return ActorType.Invalid;
    }

    public virtual void SetFaction(int id)
    {
        mFactionId = id;
    }

    public void SetDepth(float d)
    {
        mDepth = d;
    }

    public void SetPath(ActorPath ap)
    {
        if (mLocomotion == null)
        {
            mLocomotion = new ActorPathMotor(gameObject);
        }
        ActorPath.QuatT curQt;
        curQt.T = transform.position;
        curQt.Rot = transform.rotation;
        mLocomotion.SetPath(ap, curQt, false);
    }

    public void SetPath(ActorPath ap, ActorPath.QuatT groupQT)
    {
        if (mLocomotion == null)
        {
            mLocomotion = new ActorPathMotor(gameObject);
        }
        mLocomotion.SetPath(ap, groupQT, false);
    }

    public virtual void InitEmittersBySubTransfroms()
    {
        mEmitters.Clear();
        foreach (Transform tf in transform)
        {
            Emitter_Basic be = tf.GetComponent<Emitter_Basic>();
            if (be != null)
            {
                mEmitters.Add(be);
            }
        }
    }

    public void InitialAddEmitter(Emitter_Basic be)
    {
        if (be != null && !mEmitters.Contains(be))
        {
            be.transform.parent = transform;
            be.transform.localPosition = Vector3.zero;
            be.transform.localRotation = Quaternion.identity;
            mEmitters.Add(be);
        }
    }

    public void InitialAddEmitter(Emitter_Basic be, Vector3 localOffset, Quaternion localRot)
    {
        if (be != null && !mEmitters.Contains(be))
        {
            be.transform.parent = transform;
            be.transform.localPosition = localOffset;
            be.transform.localRotation = localRot;
            mEmitters.Add(be);
        }
    }

    public BasicAbility AddAbility(BasicAbility ability)
    {
        System.Type t = ability.GetType();
        if (mAbilityMap.ContainsKey(t))
        {
            mAbilityMap[t] = ability;
        }
        else
        {
            mAbilityMap.Add(t, ability);
        }
        return ability;
    }

    public void RemoveAbility(BasicAbility ability)
    {
        mAbilityMap.Remove(ability.GetType());
        ability = null;
    }

    public void ClearAbility()
    {
        mAbilityMap.Clear();
    }

    public virtual void OnGameInitialize()
    {
        Hp.OnChangeValue += OnHpChangeValue;
    }

    public virtual void OnGameUninitialize()
    {
        Hp.OnChangeValue -= OnHpChangeValue;
    }

    void OnHpChangeValue(float value)
    {
        if (value <= 0)
        {
            Die();
        }
    }

    public virtual void SwitchBehaviorFSM(int bt)
    {
        mBehavior2Switch = bt;
        BehaviorFSM.Start(bt);
    }

    public virtual void ResetBehavior()
    {
        if (mBehaviorProvider != null)
        {
            mBehaviorProvider.Reset();
        }
    }

    #region In-Game_Functions
    public virtual void OnGameStart()
    {
        for (int i = 0; i < mEmitters.Count; ++i)
        {
            mEmitters[i].SetOwner(this);
            mEmitters[i].Initialize();
        }
        for (int i = 0; i < mEmitters.Count; ++i)
        {
            mEmitters[i].SetForce(Force.CurrentValue);
        }
    }

    public virtual void OnGameUpdate(float dt)
    {
        if (mAnimator != null)
        {
            mAnimator.SyncGameSceneTimeScale();
        }

        if (dt <= 0)
        {
            return;
        }

        if (mBehaviorFSM != null)
        {
            if (mBehavior2Switch >= 0)
            {
                mBehaviorFSM.Switch2(mBehavior2Switch, false);
                mBehavior2Switch = -1;
            }
            mBehaviorFSM.UpdateBehaviors();
        }

        if (mLocomotion != null)
        {
            mLocomotion.OnGameUpdate(dt);
        }
        if (mBehaviorFSM != null)
        {
            mBehaviorFSM.UpdateBehaviors();
        }

        List<BasicAbility> abilitys = new List<BasicAbility>();
        foreach (KeyValuePair<System.Type, BasicAbility> kv in mAbilityMap)
        {
            abilitys.Add(kv.Value);
        }
        for (int i = 0; i < abilitys.Count; i++)
        {
            abilitys[i].OnGameUpdate(dt);
        }
    }

    public virtual void IteraterEmiters(System.Action<Emitter_Basic> act)
    {
        for (int i = 0; i < mEmitters.Count; ++i)
        {
            act(mEmitters[i]);
        }
    }

    public virtual void IteraterAbilitys(System.Action<BasicAbility> act)
    {
        foreach (KeyValuePair<System.Type, BasicAbility> ability in mAbilityMap)
        {
            act(ability.Value);
        }
    }

    public virtual void Die()
    {
        ActorDieEvent ade = new ActorDieEvent();
        ade.mActor = this;
        ade.Destroy = false;
        Game.Singleton.SendEvent(ade, null, 0.01f);
    }

    public virtual void Destroy()
    {
        ActorDieEvent ade = new ActorDieEvent();
        ade.mActor = this;
        ade.Destroy = true;
        Game.Singleton.SendEvent(ade, null, 0.01f);
    }

    public virtual Renderer AccessRenerer()
    {
        return null;
    }

    public virtual void SetAlpha(float a)
    {
        Renderer r = AccessRenerer();
        if (r != null)
        {
            r.material.SetFloat("_Alpha", a);
        }
    }

    #endregion

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == ActorManage.ActorTag_Enemy || other.tag == ActorManage.ActorTag_Player)
        {
            Actor ac = other.GetComponentInParent<Actor>();
            if (ac != null)
            {
                if (FactionId != ac.FactionId)
                    OnCollideWithAircraft(ac);
            }
            else
            {
                Game.LogError("错误的角色物体" + other.name + "缺少角色组件");
            }
        }
    }

    protected virtual void OnCollideWithAircraft(Actor ac)
    {
        ActorCollideWithActor aca = new ActorCollideWithActor();
        aca.mActor = this;
        aca.mOther = ac;
        Game.Singleton.SendEvent(aca, null, 0);
    }

    protected virtual void OnCollideWithBullet(Projectile proj)
    {
        ActorCollideWithProj aca = new ActorCollideWithProj();
        aca.mActor = this;
        aca.mProj = proj;
        Game.Singleton.SendEvent(aca, null, 0);
    }

    ActorData mForce = new ActorData();
    ActorData mDef = new ActorData();
    ActorData mHp = new ActorData();
    protected ActorAnimator mAnimator = null;
    protected ActorPathMotor mLocomotion = null;
    protected ActorBehaviorFSM mBehaviorFSM = null;
    protected ActorBehaivorProvider mBehaviorProvider = null;
    protected List<Emitter_Basic> mEmitters = new List<Emitter_Basic>();
    protected int mFactionId;
    protected float mSelfTimeScale = 1.0f;
    protected int mBehavior2Switch = -1;
    float mDepth;
}