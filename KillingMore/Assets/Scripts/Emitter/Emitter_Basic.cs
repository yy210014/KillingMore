using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Emitter
{
    public enum EmitterType
    {
        None = 0,
        Bullet,
        Laser,
        Missile,
        Fire
    }
    public class Emitter_Basic : MonoBehaviour
    {
        [Tooltip("Projectile Prefab")]
        public Projectile ProjectilePrefab;
        [Tooltip("子弹发射点，默认为发射器位置")]
        public Transform EmitterPoint;
        // bullet locomotion
        [Tooltip("Projectile初始速度")]
        public float InitialVeloc = 5;
        [Tooltip("Projectile运行的加速度")]
        public float Acc = 0;
        [Tooltip("加速持续时间")]
        public float AccDuration = -1;
        [Tooltip("每个运行周期的延迟时间")]
        public float Delay;
        [Tooltip("每个运行周期之间的冷却间隔，大于0时有效，否则将只有一个运行周期")]
        public float Cooldown = -1;
        [Tooltip("发射器自身的旋转速度（角度）")]
        public float RotatingSpeed;
        [Tooltip("逆向阀值（角度）")]
        public float RevertAngle = 0;
        [Tooltip("是否使用恒定速度旋转")]
        public bool LinearRotating;//
        [Tooltip("是否使用发射器定位瞄准")]
        public bool Orientate;//
        [Tooltip("Projectile贯穿敌机")]
        public bool Penetrate;
        [Tooltip("路径名")]
        public string PathName;
        [Tooltip("射击特效")]
        public string ShotEffect;
        [Tooltip("射击音效")]
        public AudioClip ShootClip;
        public float PositionRandomness;

        public EmitterEventListener OnEmitStart;
        public EmitterEventListener OnEmit;
        public EmitterEventListener OnEmitEnd;

        public Actor Owner { get { return mOwner; } }
        public float Force { get { return mForce; } }
        public ActorPathMotor Locomotion { get { return mLocomotion; } }
        public bool AutoDestructWhenDetach { get; set; }//自动销毁，当发射器独立于飞机时，默认为false

        public virtual EmitterType GetEmitterType()
        {
            throw new System.NotImplementedException();
        }

        // 计算该发射器单独的dps
        public virtual float CalcBPS()
        {
            throw new System.NotImplementedException();
        }

        void Awake()
        {
            OnEmitStart = new EmitterEventListener(this);
            OnEmit = new EmitterEventListener(this);
            OnEmitEnd = new EmitterEventListener(this);
            if (EmitterPoint == null) EmitterPoint = transform;
        }

        public virtual void Initialize(bool resetTimer = true)
        {
            if (resetTimer)
            {
                mTimer = 0;
                mRevertRotating = false;
                mCurrentAngle = 0;
                float absRs = Mathf.Abs(RotatingSpeed);
                float T = absRs > 0 ? Mathf.Abs(RevertAngle) * 4.0f / absRs : 0.001f;
                mRotateElapsed = T * 0.25f;
            }
            EmitStart();
            LoadShootSound();
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

        // 仅在独立管理自身时调用
        void Update()
        {
            if (AutoDestructWhenDetach)
            {
                float dt = GameScene.Singleton.TimeDelta;
                OnGameUpdate(GameScene.Singleton.TimeDelta);
            }
        }

        public virtual void OnGameUpdate(float dt)
        {
            if (mCurAction != null)
            {
                mCurAction(dt);
            }
            if (mLocomotion != null)
            {
                mLocomotion.OnGameUpdate(dt);
            }
            mCurrentAngle = transform.rotation.eulerAngles.y;
            /*
            //Rotating
            if (mCurAction != ApplyDelay && mCurAction != ApplyCooldown && !Orientate)
            {
                mRotateElapsed += dt;
                float sign = mRevertRotating ? -1.0f : 1.0f;
                mCurrentAngle += RotatingSpeed * dt * sign;
                if (RevertAngle > 0)
                {
                    if (LinearRotating)
                    {
                        if (mCurrentAngle >= RevertAngle)
                        {
                            mRevertRotating = !mRevertRotating;
                            float it = Mathf.Floor(mCurrentAngle / RevertAngle);
                            float res = mCurrentAngle - it * RevertAngle;
                            mCurrentAngle -= 2.0f * res;
                        }
                        else if (mCurrentAngle <= -RevertAngle)
                        {
                            mRevertRotating = !mRevertRotating;
                            float it = Mathf.Floor(-mCurrentAngle / RevertAngle);
                            float res = mCurrentAngle + it * RevertAngle;
                            mCurrentAngle -= 2.0f * res;
                        }
                    }
                    else
                    {
                        float absRs = Mathf.Abs(RotatingSpeed);
                        if (absRs > 0)
                        {
                            float T = Mathf.Abs(RevertAngle) * 4.0f / Mathf.Abs(RotatingSpeed);
                            float _sign = Mathf.Sign(RotatingSpeed);
                            mRotateElapsed = Mathf.Clamp(mRotateElapsed, 0, T);
                            float rd = (mRotateElapsed / T) * Mathf.PI * 2.0f;
                            mCurrentAngle = RevertAngle * Mathf.Cos(rd) * _sign;
                            if (mRotateElapsed >= T)
                            {
                                mRotateElapsed = 0;
                            }
                        }
                    }
                }
                Quaternion rot = Quaternion.identity;
                if (Mathf.Abs(mCurrentAngle) > 0.005f)
                {
                    rot = Quaternion.Euler(0, mCurrentAngle, 0);
                }
                transform.rotation = rot;
            }
            if (Orientate)
            {
                List<Actor> airs = Owner.FactionId == Actor.PlayerTeamFactionId ? ActorManage.Singleton.Aircrafts : ActorManage.Singleton.PlayerActors;
                if (airs.Count > 0)
                {
                    if (Mathf.Floor(GameScene.Singleton.Elapsed) % 2 == 0)
                    {
                        //筛选
                        airs = airs.FindAll((air) =>
                        {
                            return air.GetComponent<Collider>() != null && !GameScene.Singleton.invisible(air.transform.position); ;
                        });
                        //排序
                        airs.Sort((x, y) =>
                        {
                            float p1 = (x.transform.position - transform.position).magnitude;
                            float p2 = (y.transform.position - transform.position).magnitude;
                            return p1.CompareTo(p2);
                        });
                        if (airs.Count <= 0) return;
                        mOrientateAc = airs[0] as Actor;
                    }
                }
                if (mOrientateAc != null)
                {
                    float turnSpeed = 5;
                    Vector3 dir = (mOrientateAc.transform.position - transform.position).normalized;
                    Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * dt);
                }
            } */
        }

        protected virtual void ApplyDelay(float dt)
        {
            mTimer += dt;
            if (mTimer >= Delay)
            {
                mTimer = mKeepTimeResidue ? Misc.FracIfGreater(mTimer, Delay) : 0;
                Emit();
            }
        }

        protected virtual void ApplyCooldown(float dt)
        {
            mTimer += dt;
            if (mTimer >= Cooldown)
            {
                mTimer = mKeepTimeResidue ? Misc.FracIfGreater(mTimer, Cooldown) : 0;
                Initialize(false);
            }
        }

        public virtual void SetOwner(Actor o)
        {
            mOwner = o;
        }

        public virtual void SetForce(float force)
        {
            mForce = force;
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

        protected void LoadShootSound()
        {
            if (ShootClip == null) return;
            mShootSound = GetComponent<AudioSource>();
            if (mShootSound == null)
            {
                mShootSound = gameObject.AddComponent<AudioSource>();
            }
            mShootSound.clip = ShootClip;
        }

        protected virtual void EmitStart()
        {
            if (OnEmitStart != null)
            {
                OnEmitStart.Call();
            }
            mCurAction = ApplyDelay;
        }

        protected virtual void Emit()
        {
            if (OnEmit != null)
            {
                OnEmit.Call();
            }
            if (mShootSound != null)
            {
                mShootSound.Stop();
                mShootSound.Play();
            }
            EmitEnd();
        }

        protected virtual void EmitEnd()
        {
            if (OnEmitEnd != null)
            {
                OnEmitEnd.Call();
            }
            if (Cooldown > 0)
            {
                mCurAction = ApplyCooldown;
            }
            else
            {
                DisableEmitter();
            }
        }

        public virtual void DisableEmitter(bool Destroy = true)
        {
            mCurAction = null;
            if (Destroy && AutoDestructWhenDetach
                && transform.parent == null)
            {
                GameObject.Destroy(gameObject);
            }
        }

        protected Projectile Spawn(Quaternion rot)
        {
            if (ProjectilePrefab == null)
            {
                return null;
            }
            GameObject g = AssetsManager.Singleton.FindNPopFromPool(ProjectilePrefab.name, AssetsManager.ResType.Projectile);
            if (g == null)
            {
                g = GameObject.Instantiate(ProjectilePrefab.gameObject) as GameObject;
                g.name = ProjectilePrefab.name;
            }
            if (g != null)
            {
                g.transform.position = EmitterPoint != null ? EmitterPoint.position : transform.position;
                g.transform.rotation = rot;
                return g.GetComponent<Projectile>();
            }
            return null;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (mLocomotion != null)
            {
                mLocomotion.DebugDrawPath();
            }
        }
#endif
        protected Actor mOwner;
        protected System.Action<float> mCurAction;
        protected float mCurrentAngle;
        protected float mTimer;
        protected bool mKeepTimeResidue = false;
        protected bool mRevertRotating = false;
        // 记录发射器旋转
        protected float mRotateElapsed = 0;
        // Path
        protected ActorPathMotor mLocomotion = null;
        protected float mForce;
        protected AudioSource mShootSound;
        Actor mOrientateAc;
    }
}