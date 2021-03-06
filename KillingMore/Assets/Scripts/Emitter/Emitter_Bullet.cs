using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Emitter
{
    public class Emitter_Bullet : Emitter_Basic
    {
        // emit
        [Tooltip("每个运行周期发射的次数，-1 表示无限制")]
        public int EmitTimes = 1;                   // -1 表示无限制
        [Tooltip("每次发射的间隔")]
        public float EmitInterval = 1;
        [Tooltip("每次发射的子弹数量")]
        public int EmitNumPerShot = 1;
        [Tooltip("扇形发射的间隔角度")]
        public float DistributionAngle;

        [Tooltip("子弹自旋转速度(角度)")]
        public float BulletRotatingSpeed = 0;
        [Tooltip("子弹的最大弹道修正速度，单位角度")]
        public float MaxBCSpeed = 0;
        [Tooltip("子弹的弹道修正有效时间")]
        public float BCDuration = 0;
        [Tooltip("子弹爆炸的时间, 大于0时有效")]
        public float ExplodeDelay = -1;

        public override EmitterType GetEmitterType()
        {
            return EmitterType.Bullet;
        }

        public override float CalcBPS()
        {
            return ((float)(EmitTimes * EmitNumPerShot)) / ((float)(EmitTimes - 1) * EmitInterval + Cooldown);
        }

        public override void Initialize(bool resetTimer = true)
        {
            LoadShootSound();
            mEmitCount = EmitTimes;
            base.Initialize(resetTimer);
        }

        protected void ApplyEmitInterval(float dt)
        {
            mTimer += dt;
            if (mTimer >= EmitInterval)
            {
                mTimer = mKeepTimeResidue ? Misc.FracIfGreater(mTimer, EmitInterval) : 0;
                Emit();
            }
        }

        protected override void Emit()
        {
            float angs = DistributionAngle * (float)(EmitNumPerShot - 1) * 0.5f;
            for (int i = 0; i < EmitNumPerShot; ++i)
            {
                float ag = mCurrentAngle - angs + i * DistributionAngle;
                Quaternion rot = Quaternion.identity;
                if (Mathf.Abs(ag) > 0.005f)
                {
                    rot = Quaternion.Euler(0, ag, 0);
                }
                OnBulletLoaded(Spawn(rot));
            }
            --mEmitCount;
            if (OnEmit != null)
            {
                OnEmit.Call();
            }
            if (mShootSound != null)
            {
                mShootSound.Stop();
                mShootSound.Play();
            }
            if (mEmitCount <= 0)
            {
                EmitEnd();
            }
            else
            {
                if (!mKeepTimeResidue) mTimer = 0;
                mCurAction = ApplyEmitInterval;
            }
        }

        void OnBulletLoaded(Projectile pj)
        {
            if (gameObject == null)
            {
                Game.LogError("发射器已销毁但还在调用！ " + "Owner: " + mOwner + ", Bullet: " + pj.name);
                return;
            }
            Vector3 point = EmitterPoint != null ? EmitterPoint.position : transform.position;
            pj.transform.position = point + new Vector3(Random.Range(0f, 1f) * PositionRandomness - PositionRandomness / 2, 0,
                    Random.Range(0f, 1f) * PositionRandomness - PositionRandomness / 2);
            pj.SetLocomotion(InitialVeloc, Acc, AccDuration);
            pj.OnSpawn(this);
            ActorManage.Singleton.OnSpawnProjectile(pj);
            pj.PrefabName = ProjectilePrefab.name;
            pj.RotatingSpeed = BulletRotatingSpeed;
            pj.MaxBCSpeed = MaxBCSpeed;
            pj.BCDuration = BCDuration;
            pj.ExplodeDelay = ExplodeDelay;
            pj.Penetrate = Penetrate;
            pj.Force = mForce;
            pj.FactionId = mOwner != null ? mOwner.FactionId : Actor.EnemyTeamFactionId;
        }

        protected int mEmitCount;
    }
}