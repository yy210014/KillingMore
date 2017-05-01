using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Emitter
{
    public class Projectile : MonoBehaviour
    {
        public enum ProjectileType
        {
            Invalid = 0,
            Bullet,
            Laser
        }
        // 注意，不直接使用emiter的引用，因为emiter可能已经消失
        [Tooltip("名字")]
        public string PrefabName;
        [Tooltip("追踪目标")]
        public Actor Target;
        [Tooltip("最大生命周期")]
        public float MaxLife = 99.0f;
        [Tooltip("击中效果")]
        public string HitEffect;

        public int FactionId { get; set; }
        public float Force { get; set; }
        public bool IsDead { get; private set; }
        public ActorPathMotor Locomotion { get { return mMotor; } }

        public float ExplodeDelay { get; set; }
        public float RotatingSpeed { get; set; }
        public float MaxBCSpeed { get; set; }
        public float BCDuration { get; set; }
        public bool Penetrate { get; set; }

        public virtual ProjectileType GetProjectileType()
        {
            return ProjectileType.Invalid;
        }

        public virtual void OnSpawn(Emitter_Basic bm)
        {
            if (mInitScale == Vector3.zero) mInitScale = transform.localScale;
            transform.localScale = mInitScale;
            if (mProcEmiter == null && mExplodeEmiter == null)
            {
                foreach (Transform tf in transform)
                {
                    Emitter_Basic be = tf.GetComponent<Emitter_Basic>();
                    if (be != null)
                    {
                        string tfName = tf.name.ToLower();
                        if (tfName.Contains("explode"))
                        {
                            mExplodeEmiter = be;
                            mExplodeEmiter.gameObject.SetActive(false);
                        }
                        else
                        {
                            if (mProcEmiter != null)
                            {
                                Game.LogError("过程发射器数量多于1个，将被屏蔽!");
                                continue;
                            }
                            mProcEmiter = be;
                            mProcEmiter.Initialize();
                        }
                    }
                }
            }

            if (mExplodeEmiter != null)
            {
                mExplodeEmiter.gameObject.SetActive(false);
                mExplodeEmiter.SetOwner(bm.Owner);
            }

            if (mProcEmiter != null)
            {
                mProcEmiter.SetOwner(bm.Owner);
            }

            IsDead = false;
            mLife = 0;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.tag == ActorManage.ActorTag_Enemy || other.tag == ActorManage.ActorTag_Player)
            {
                Actor ac = other.GetComponentInParent<Actor>();
                if (ac != null)
                {
                    if (FactionId != ac.FactionId)
                    {
                        OnCollideWithAircraft(ac);
                    }
                }
                else
                {
                    Game.LogError("错误的角色物体" + other.name + "缺少角色组件");
                }
            }
        }

        protected virtual void OnCollideWithAircraft(Actor ac)
        {
            ActorCollideWithProj aca = new ActorCollideWithProj();
            aca.mActor = ac;
            aca.mProj = this;
            Game.Singleton.SendEvent(aca, null, 0);
        }

        public virtual void Explode()
        {
            if (mExplodeEmiter != null)
            {
                mExplodeEmiter.gameObject.SetActive(true);
                GameObject emiterObject = GameObject.Instantiate(mExplodeEmiter.gameObject) as GameObject;
                emiterObject.transform.position = mExplodeEmiter.transform.position;
                emiterObject.transform.rotation = mExplodeEmiter.transform.rotation;

                // 设置自动销毁
                Emitter_Basic em = emiterObject.GetComponent<Emitter_Basic>();
                em.SetForce(Force);
                em.AutoDestructWhenDetach = true;
                em.SetOwner(mExplodeEmiter.Owner);
                em.Initialize();
            }
            Die();
        }

        public virtual void Die()
        {
            if (!Penetrate)
            {
                IsDead = true;
                CreateHitEffect(null);
                Destroy();
            }
        }

        public virtual void CreateHitEffect(Actor target)
        {
            if (HitEffect != "")
            {
                Vector3 RangePoint = new Vector3(Random.onUnitSphere.x, 0, Random.onUnitSphere.z) * 0.2f;
                Transform Quad = transform.Find("Quad");
                Vector3 point;
                if (Quad != null)
                {
                    point = transform.position + Quad.GetComponent<Renderer>().bounds.size.z / 2 * Vector3.forward + RangePoint;//碰撞点+随机0.2范围
                }
                else
                {
                    point = transform.position;
                }
                GameObject de = AssetsManager.Singleton.LoadGameObjectByResType(
                    HitEffect, AssetsManager.RestType2PathName(AssetsManager.ResType.Effect),
                    AssetsManager.ResType.Effect);
                if (de != null)
                {
                    SpecialEffect se = de.GetComponent<SpecialEffect>();
                    if (se != null)
                    {
                        se.transform.position = point;
                        se.transform.rotation = transform.rotation;
                        se.PrefabName = HitEffect;
                        //    se.SetTarget(target.gameObject);
                        se.Initialize();
                    }
                }
                else
                {
                    Game.LogError("找不到" + HitEffect + "效果，请检查名字！");
                }
            }
        }

        public virtual void Destroy()
        {
            IsDead = true;
            ActorManage.Singleton.DestroyProjectile(this);
        }

        void UpdateBallisticCorrection(float dt)
        {
            if (BCDuration > 0 && mMotor.Path != null)
            {
                List<Actor> airs = FactionId == Actor.PlayerTeamFactionId ? ActorManage.Singleton.Aircrafts : ActorManage.Singleton.PlayerActors;
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
                        Target = airs[0] as Actor;
                    }
                }
                BCDuration -= dt;
                if (BCDuration > 0 && Target != null)
                {
                    Vector3 diff = Target.transform.position - transform.position;
                    Vector3 curV = mMotor.Path.CurVeloc();
                    Vector3 d = diff.normalized;
                    Vector3 v = transform.forward;
                    if (diff.magnitude <= 0.0001f)
                    {
                        d = Target.transform.forward;
                    }
                    if (curV.magnitude > 0.0001f)
                    {
                        v = curV.normalized;
                    }

                    Quaternion dr = Quaternion.LookRotation(d, Vector3.up);
                    Quaternion vr = Quaternion.LookRotation(v, Vector3.up);
                    Quaternion newRot = Quaternion.RotateTowards(vr, dr, Mathf.Abs(MaxBCSpeed) * dt);
                    CurvePath cp = mMotor.Path as CurvePath;
                    float acc = cp.mAcceleraction.magnitude;
                    Vector3 newV = newRot * Vector3.forward;
                    Vector3 newAcc = newV * acc;
                    newV = newV * curV.magnitude;
                    CurvePath.ConfigPath(cp, transform, newV, newAcc, cp.mAcceleratorLife);
                    cp.Reset();
                }
            }
        }

        public virtual void SetLocomotion(float initialVeloc, float acc, float accDuration)
        {
            CurvePath cp = new CurvePath();
            Vector3 forward = transform.forward;
            CurvePath.ConfigPath(cp, transform, forward * initialVeloc, forward * acc, accDuration);
            if (mMotor == null)
            {
                mMotor = new ActorPathMotor(gameObject);
            }
            ActorPath.QuatT qt;
            qt.T = Vector3.zero;
            qt.Rot = transform.rotation;
            mMotor.SetPath(cp, qt);
        }

        public void SetLocomotion(ActorPath path)
        {
            if (mMotor == null)
            {
                mMotor = new ActorPathMotor(gameObject);
            }
            ActorPath.QuatT qt;
            qt.T = Vector3.zero;
            qt.Rot = transform.rotation;
            mMotor.SetPath(path, qt);
        }

        public virtual void OnGameUpdate(float dt)
        {
            if (mMotor != null)
            {
                // 缓存之前的euler角
                Vector3 euler = transform.rotation.eulerAngles;
                UpdateBallisticCorrection(dt);
                mMotor.OnGameUpdate(dt);
                // 自身旋转
                if (RotatingSpeed != 0)
                {
                    float angDelta = RotatingSpeed * dt;
                    euler.y += angDelta;
                    transform.rotation = Quaternion.Euler(euler);
                }
            }

            if (mProcEmiter != null)
            {
                mProcEmiter.OnGameUpdate(dt);
            }

            mLife += dt;
            if (mExplodeEmiter != null && ExplodeDelay > 0 && mLife > ExplodeDelay)
            {
                Explode();
                return;
            }

            if (MaxLife > 0 && mLife >= MaxLife)
            {
                Explode();
            }
        }

        private bool waitingForTriggerTime;
        protected ActorPathMotor mMotor = null;
        protected float mLife = 0;
        Emitter_Basic mProcEmiter = null;
        Emitter_Basic mExplodeEmiter = null;
        Vector3 mInitScale;

    }
}