using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Emitter
{
    public class EmitterParent : Emitter_Basic
    {
        [Tooltip("更新序列间隔，如果大于0则按顺序更新子发射器，否则则为同步")]
        public float SequentialInterval = 0;

        public bool SequentialGroup { get { return SequentialInterval > 0; } }
        public List<Emitter_Basic> SubEmiters { get { return mSubEmiters; } }

        public override EmitterType GetEmitterType()
        {
            return EmitterType.None;
        }

        public override void SetOwner(Actor o)
        {
            mOwner = o;
            for (int i = 0; i < mSubEmiters.Count; ++i)
            {
                mSubEmiters[i].SetOwner(o);
            }
        }

        public override void SetForce(float force)
        {
            mForce = force;
            if (mSubEmiters.Count <= 0 || force == 0)
            {
                return;
            }
            if (mOwner.FactionId == Actor.PlayerTeamFactionId)
            {
                float bps = 0;
                for (int i = 0; i < mSubEmiters.Count; ++i)
                {
                    bps += mSubEmiters[i].CalcBPS(); //每发子弹每秒的攻击力
                }
                float dpbs = force / bps * mSubEmiters.Count;//所有发射器的Force
                if (bps == 0)
                {
                    dpbs = 0;
                    Game.Log("Emiter:" + name + ", bps=0,请检查攻击力");
                }
                for (int i = 0; i < mSubEmiters.Count; ++i)
                {
                    mSubEmiters[i].SetForce(dpbs / mSubEmiters.Count);
                }
            }
            else
            {
                for (int i = 0; i < mSubEmiters.Count; ++i)
                {
                    mSubEmiters[i].SetForce(mForce);
                }
            }
        }

        public override void Initialize(bool resetTimer = true)
        {
            if (resetTimer)
            {
                mTimer = 0;
            }
            LoadSubEmiter();
            for (int i = 0; i < mSubEmiters.Count; ++i)
            {
                if (SequentialGroup)
                {
                    mSubEmiters[i].gameObject.SetActive(false);
                }
                mSubEmiters[i].Initialize(resetTimer);
                mSubEmiters[i].OnEmitEnd.Add(OnSubEmitterEnd);
            }
            if (mSubEmiters.Count > 0)
            {
                mCurEmitterInSeq = mSubEmiters[0];
                if (SequentialGroup)
                {
                    mCurEmitterInSeq.gameObject.SetActive(true);
                }
                mSequentialTimer = 0;
                mSequentialCooling = false;
            }

            if (PathName != "")
            {
                ActorPath ap = PathManager.Singleton.CreatePath(PathName);
                if (ap != null)
                {
                    SetPath(ap);
                }
                else
                {
                    Game.LogError("创建Path :" + PathName + " 失败");
                }
            }
        }

        protected override void ApplyCooldown(float dt)
        {
            if (SequentialGroup)
            {
                // 作为序列组时，忽略cooldown.
                mTimer = mKeepTimeResidue ? Misc.FracIfGreater(mTimer, Cooldown) : 0;
                mCurAction = null;
                return;
            }
            mTimer += dt;
            if (mTimer >= Cooldown)
            {
                mTimer = mKeepTimeResidue ? Misc.FracIfGreater(mTimer, Cooldown) : 0;
                Initialize(!mKeepTimeResidue);
            }
        }

        // 仅在独立管理自身时调用
        void Update()
        {
            if (!AutoDestructWhenDetach) return;
            OnGameUpdate(GameScene.Singleton.TimeDelta);
        }

        public override void OnGameUpdate(float dt)
        {
            if (SequentialInterval > 0)
            {
                if (mSequentialCooling)
                {
                    mSequentialTimer += dt;
                    if (mSequentialTimer >= SequentialInterval)
                    {
                        mSequentialCooling = false;
                        mCurEmitterInSeq.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (mCurEmitterInSeq != null)
                    {
                        mCurEmitterInSeq.OnGameUpdate(dt);
                    }
                }
            }
            else
            {
                for (int i = 0; i < mSubEmiters.Count; ++i)
                {
                    mSubEmiters[i].OnGameUpdate(dt);
                }
            }
            if (mLocomotion != null)
            {
                mLocomotion.OnGameUpdate(dt);
            }
        }

        public void OnSubEmitterEnd(Emitter_Basic be)
        {
            if (SequentialInterval > 0)
            {
                for (int i = 0; i < mSubEmiters.Count; ++i)
                {
                    mSubEmiters[i].gameObject.SetActive(false);
                    if (be == mSubEmiters[i])
                    {
                        mCurEmitterInSeq = i < mSubEmiters.Count - 1 ? mSubEmiters[i + 1] : mSubEmiters[0];
                        mSequentialCooling = true;
                        mSequentialTimer = 0;
                        mCurEmitterInSeq.Initialize(!mKeepTimeResidue);
                    }
                }
            }
        }

        public void LoadSubEmiter()
        {
            if (mSubEmiters.Count == 0)
            {
                foreach (Transform tf in transform)
                {
                    Emitter_Basic be = tf.GetComponent<Emitter_Basic>();
                    if (be != null)
                    {
                        mSubEmiters.Add(be);
                        be.SetOwner(Owner);
                    }
                }
            }
        }

        public override void DisableEmitter(bool Destroy = true)
        {
            mSequentialTimer = 0;
            mSequentialCooling = false;
            for (int i = 0; i < mSubEmiters.Count; i++)
            {
                mSubEmiters[i].DisableEmitter();
            }
            mSubEmiters.Clear();
            mCurEmitterInSeq = null;
            mCurAction = null;
            if (Destroy && AutoDestructWhenDetach
                && transform.parent == null)
            {
                GameObject.Destroy(gameObject);
            }
        }

        List<Emitter_Basic> mSubEmiters = new List<Emitter_Basic>();
        Emitter_Basic mCurEmitterInSeq = null;//当前的发射器
        // 顺序发射控制
        float mSequentialTimer = 0;
        bool mSequentialCooling = false;
    }
}