using UnityEngine;
using System.Collections;

namespace Emitter
{
    public class Proj_Laser : Projectile
    {
        public LineRenderer LineRenderer { get; private set; }
        public int VertexCount { get { return mVertexCount; } }
        public float Width { get { return LineRenderer.startWidth; } }
        public float Length { get { return (StartPosition - EndPosition).magnitude; } }
        public Vector3[] VertexPosition { get { return mVertexPositions; } }
        public Vector3 StartPosition { get { return LineRenderer.GetPosition(0); } }
        public Vector3 EndPosition { get { return LineRenderer.GetPosition(LineRenderer.numPositions - 1); } }

        public override ProjectileType GetProjectileType()
        {
            return ProjectileType.Laser;
        }

        public override void OnSpawn(Emitter_Basic bm)
        {
            LineRenderer = GetComponentInChildren<LineRenderer>();
            if (LineRenderer == null)
            {
                LineRenderer = new GameObject().AddComponent<LineRenderer>();
                LineRenderer.transform.parent = transform;
                LineRenderer.name = "LineRenderer";
            }
            LineRenderer.useWorldSpace = false;
            LineRenderer.numPositions = 0;
            mVertexCount = 0;
            SetStartPosition(Vector3.zero);
            SetEndPosition(Vector3.zero);
            mCurAction = Flow;
            base.OnSpawn(bm);
        }

        public void Flow(float dt)
        {
            mTimer -= dt;
            LineRenderer.material.mainTextureOffset = Vector2.right * mTimer;
        }

        public override void OnGameUpdate(float dt)
        {
            mLife += dt;
            if (MaxLife > 0 && mLife >= MaxLife)
            {
                Die();
            }
            if (mCurAction != null)
            {
                mCurAction(dt);
            }
            if (mMotor != null)
            {
                mMotor.OnGameUpdate(dt);
            }
            //碰撞检测
            RaycastHit hit = new RaycastHit();
            Vector3 dir = transform.forward;
            if (Physics.Raycast(transform.position + StartPosition, dir, out hit, Length, 1 << LayerMask.NameToLayer("LaserActive")))
            {
                if (hit.collider.tag == ActorManage.ActorTag_Enemy || hit.collider.tag == ActorManage.ActorTag_Player)
                {
                    Actor ac = hit.collider.GetComponentInParent<Actor>();
                    if (ac != null)
                    {
                        if (FactionId != ac.FactionId)
                        {
                            OnCollideWithLaser(ac, hit.point);
                        }
                    }
                    else
                    {
                        Game.LogError("错误的角色物体" + hit.collider.name + "缺少角色组件");
                    }
                }
            }
        }

        public void SetWidth(float width)
        {
#if UNITY_5_5
            LineRenderer.startWidth = width;
            LineRenderer.endWidth = width;
#else
           // mLaser.SetWidth(width, width);
#endif
        }

        public void SetStartPosition(Vector3 position)
        {
            if (LineRenderer.numPositions < 1) LineRenderer.numPositions = 1;
            LineRenderer.SetPosition(0, position);
        }

        public void SetEndPosition(Vector3 position)
        {
            if (LineRenderer.numPositions < 2) LineRenderer.numPositions = 2;
            LineRenderer.SetPosition(mVertexCount + 1, position);
        }

        public void SetVertexPosition(int index, Vector3 position)
        {
            mVertexPositions[index] = position;
            LineRenderer.SetPosition(index + 1, position);
        }

        public void SetVertexCount(int count)
        {
            mVertexCount = count;
            if (count <= 0) return;
            LineRenderer.numPositions = count + 2;
            mVertexPositions = new Vector3[count];
            for (int i = 0; i < VertexCount; i++)
            {
                SetVertexPosition(i, Vector3.forward * 0.01f);
            }
#if UNITY_5_5
#else
          //  mLaser.SetVertexCount(count + 2);
#endif
        }

        protected virtual void OnCollideWithLaser(Actor ac, Vector3 hitPoint)
        {
            ActorCollideWithLaser acl = new ActorCollideWithLaser();
            acl.mActor = ac;
            acl.mProj = this;
            acl.mHitPoint = hitPoint;
            Game.Singleton.SendEvent(acl, null, 0);
        }

        public virtual void CreateHitEffect(Vector3 point)
        {
            if (HitEffect != "")
            {
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
                        se.Initialize();
                    }
                }
                else
                {
                    Game.LogError("找不到" + HitEffect + "效果，请检查名字！");
                }
            }
        }

        Vector3[] mVertexPositions;
        Vector3 mVertexPosition;
        Vector3 mNodePosition;
        int mVertexCount;
        System.Action<float> mCurAction;
        float mTimer;
    }
}