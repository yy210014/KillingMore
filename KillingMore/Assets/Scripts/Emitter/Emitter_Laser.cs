using UnityEngine;
using System.Collections;

namespace Emitter
{
    public class Emitter_Laser : Emitter_Basic
    {
        [Tooltip("激光蓄电时间")]
        public float BatteryTime = 0.1f;
        [Tooltip("蓄电伤害百分比加成,取值范围0~1")]
        public float BatteryPercent = 0.2f;
        [Tooltip("持续发射时间")]
        public float Duration;
        [Tooltip("激光宽度")]
        public float Width = 1;
        [Tooltip("激光节点数")]
        public int VertexCount = 0;

        public override float CalcBPS()
        {
            return base.CalcBPS();
        }

        public override void Initialize(bool resetTimer = true)
        {
            base.Initialize(resetTimer);
        }

        protected void ApplyProgress(float dt)
        {
            mTimer += dt;
            if (mTempLaser == null) return;
            if (mTimer > Duration)
            {
                mTimer = mKeepTimeResidue ? Misc.FracIfGreater(mTimer, Duration) : 0;
                EmitEnd();
                if (InitialVeloc > 0)
                {
                    CurvePath cp = new CurvePath();
                    Vector3 forward = mTempLaser.transform.forward;
                    CurvePath.ConfigPath(cp, mTempLaser.transform.position, mTempLaser.transform.rotation, forward * InitialVeloc, forward * Acc, AccDuration);
                    mTempLaser.SetLocomotion(cp);
                }
                return;
            }
            if (InitialVeloc > 0)
            {
                mTempLaser.SetEndPosition(mTempLaser.EndPosition + Vector3.forward * mTimer / InitialVeloc);
            }
            else
            {
                if (BatteryTime > 0 && mTimer <= BatteryTime)
                {
                    float tw = (1 + BatteryPercent) * Width;
                    mTempLaser.SetWidth(Mathf.MoveTowards(mTempLaser.Width, tw, (tw - mTempLaser.Width) / BatteryTime * dt));
                }
                mTempLaser.SetEndPosition((Game.SceneMagnitude - Game.SceneHeight - mTempLaser.transform.position.z + 2) * Vector3.forward);//2是额外的超出屏幕长度
            }
        }

        protected override void Emit()
        {
            OnLaserLoaded(Spawn(transform.rotation));
            if (OnEmit != null)
            {
                OnEmit.Call();
            }
            if (!mKeepTimeResidue) mTimer = 0;
            mCurAction = ApplyProgress;
        }

        protected override void EmitEnd()
        {
            base.EmitEnd();
            if (InitialVeloc <= 0)
            {
                mTempLaser.Destroy();
            }
        }

        void OnLaserLoaded(Projectile pj)
        {
            if (gameObject == null)
            {
                Game.LogError("发射器已销毁但还在调用！ " + "Owner: " + mOwner + ", Bullet: " + pj.name);
                return;
            }
            Proj_Laser pl = pj.GetComponent<Proj_Laser>();
            if (pl != null)
            {
                pl.OnSpawn(this);
                ActorManage.Singleton.OnSpawnProjectile(pl);
                pl.transform.parent = transform.transform;
                pl.transform.localPosition = Vector3.up * 0.5f;
                pl.PrefabName = ProjectilePrefab.name;
                pl.Penetrate = Penetrate;
                pl.FactionId = mOwner != null ? mOwner.FactionId : Actor.EnemyTeamFactionId;
                pl.SetVertexCount(VertexCount);
                pl.SetWidth(Width);
                pl.SetLocomotion(null);
                pl.Force = mForce;
                mTempLaser = pl;
            }
        }

        Proj_Laser mTempLaser;
    }
}
