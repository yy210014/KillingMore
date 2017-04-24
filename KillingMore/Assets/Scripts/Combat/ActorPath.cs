using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ActorPath
{
    public const float NeverTimedout = 9999999.0f;

    public enum TypeEnum
    {
        Invalid = 0,
        NodeBasedPath,
        AutoCurvePath,
    }

    public struct QuatT
    {
        public Quaternion Rot;
        public Vector3 T;

        public static readonly QuatT Zero = new QuatT();
    }

    public abstract TypeEnum GetTypeEnum();

    public abstract QuatT Start();

    public abstract QuatT Reset();

    public abstract QuatT NextPosition(float dt);

    public abstract bool PathEnded();

    public abstract Vector3 CurVeloc();

    public abstract Vector3 CurPos();
}

public class NPPath : ActorPath
{
    public struct Pt
    {
        // from editor
        public Vector3 Pos;
        public Quaternion Rot;
        public float Veloc;
        public short Behavior;
        public float BehaviorDuration;
        public float Disturbance;
        public bool KeyPt;

        // 根据两点之间的Veloc和距离计算出两点之间的Dt
        public float Dt;
        public Vector3 Acc;
    }

    public static Pt MakePt(Vector3 pos, Quaternion rot, float v)
    {
        Pt pt;
        pt.Pos = pos;
        pt.Rot = rot;
        pt.Veloc = v;
        pt.Dt = 0;
        pt.Acc = Vector3.zero;
        pt.Behavior = 0;
        pt.BehaviorDuration = 0;
        pt.KeyPt = false;
        pt.Disturbance = 0;

        return pt;
    }

    public static Pt MakePt(Vector3 pos, Quaternion rot, float v, short bh,
        float bhDuration, bool keyPt, float distur)
    {
        Pt pt;
        pt.Pos = pos;
        pt.Rot = rot;
        pt.Veloc = v;
        pt.Dt = 0;
        pt.Acc = Vector3.zero;
        pt.Behavior = bh;
        pt.BehaviorDuration = bhDuration;
        pt.KeyPt = keyPt;
        pt.Disturbance = distur;

        return pt;
    }

    public int NumPts
    {
        get
        {
            return mPts.Count;
        }
    }

    public NPPath()
    {
    }

    public NPPath(List<Pt> pts)
    {
        mPts.AddRange(pts);
    }

    public void ApplyPath(List<Pt> pts, System.Action<Pt> notify = null)
    {
        mPts.Clear();
        mPts.AddRange(pts);
        mNodeNotify = notify;
    }

    public override TypeEnum GetTypeEnum()
    {
        return ActorPath.TypeEnum.NodeBasedPath;
    }

    public override QuatT Start()
    {
        if (mPts.Count < 2)
        {
            Game.LogError("Number of path point less than 2.");
            return QuatT.Zero;
        }

        mNextPt = 1;
        mAccumulatedTime = 0;

        for (int i = 1; i < mPts.Count; ++i)
        {
            Pt prev = mPts[i - 1];
            Pt next = mPts[i];

            if (next.Veloc < 0)
            {
                Game.LogError("路径节点Velocity小于0");
            }

            Vector3 diff = next.Pos - prev.Pos;
            Vector3 vv = diff.normalized;
            Vector3 v0 = vv * prev.Veloc;
            Vector3 v1 = vv * next.Veloc;

            Vector3 dt;
            dt.x = Mathf.Abs(v0.x + v1.x) <= Mathf.Epsilon ? 0 : (2.0f * diff.x) / (v0.x + v1.x);
            dt.y = Mathf.Abs(v0.y + v1.y) <= Mathf.Epsilon ? 0 : (2.0f * diff.y) / (v0.y + v1.y);
            dt.z = Mathf.Abs(v0.z + v1.z) <= Mathf.Epsilon ? 0 : (2.0f * diff.z) / (v0.z + v1.z);
            next.Dt = Mathf.Max(dt.x, Mathf.Max(dt.y, dt.z));
            prev.Acc = (v1 - v0) / next.Dt;

            mPts[i] = next;
            mPts[i - 1] = prev;
        }

        mCurVeloc = CalcPrevInitialVeloc(1);
        mCurPos = mPts[0].Pos;

        QuatT qt;
        qt.Rot = mPts[0].Rot;
        qt.T = mCurPos;

        if (mNodeNotify != null)
        {
            mNodeNotify(mPts[0]);
        }

        return qt;
    }

    public override QuatT Reset()
    {
        mNextPt = 1;
        mAccumulatedTime = 0;

        mCurVeloc = CalcPrevInitialVeloc(1);
        mCurPos = mPts[0].Pos;

        QuatT qt;
        qt.Rot = mPts[0].Rot;
        qt.T = mCurPos;

        if (mNodeNotify != null)
        {
            mNodeNotify(mPts[0]);
        }

        return qt;
    }

    Vector3 CalcPrevInitialVeloc(int index)
    {
        Vector3 v = Vector3.zero;

        Pt prev = mPts[index - 1];
        Pt next = index >= mPts.Count ? prev : mPts[index];

        Vector3 diff = next.Pos - prev.Pos;
        Vector3 vv = diff.normalized;
        v = vv * prev.Veloc;

        return v;
    }

    bool NodeNeedsNotify(Pt n)
    {
        return n.KeyPt && (n.Behavior == (short)PathBehavior.Move
            || (n.Behavior > 0 && n.BehaviorDuration > 0));
    }

    void ToNextNode(float dt)
    {
        Pt prev = mPts[mNextPt - 1];
        Pt next = mPts[mNextPt];
        float ac = mAccumulatedTime + dt;
        if (ac > next.Dt)
        {
            mCurPos = next.Pos;
            mAccumulatedTime = 0;

            dt = ac - next.Dt;
            ++mNextPt;
            mCurRot = next.Rot;
            mCurVeloc = CalcPrevInitialVeloc(mNextPt);

            // notify
            if (mNodeNotify != null && NodeNeedsNotify(next))
            {
                mNodeNotify(next);
            }

            if (mNextPt >= mPts.Count)
            {
                mAccumulatedTime = next.Dt;
                return;
            }
            else
            {
                ToNextNode(dt);
            }
        }
        else
        {
            float t = ac / next.Dt;
            //float curV = Mathf.Lerp(prev.Veloc, next.Veloc, t);
            mCurRot = Quaternion.Lerp(prev.Rot, next.Rot, t);
            //mCurVeloc = (next.Pos - prev.Pos).normalized * curV;
            mCurVeloc = mCurVeloc + prev.Acc * dt;
            mCurPos = mCurPos + mCurVeloc * dt;
            mAccumulatedTime = ac;
        }
    }

    void SkipNodes(float dt)
    {
        Pt next = mPts[mNextPt];
        float ac = mAccumulatedTime + dt;
        if (ac > next.Dt)
        {
            float overtime = ac - next.Dt;
            mAccumulatedTime = _SkipNodes(mNextPt + 1, overtime);
        }
        else
        {
            mAccumulatedTime += dt;
        }
    }

    float _SkipNodes(int nindex, float overtime)
    {
        if (nindex >= mPts.Count)
        {
            return 0;
        }

        mNextPt = nindex;
        Pt next = mPts[nindex];
        if (overtime > next.Dt)
        {
            overtime -= next.Dt;
            return _SkipNodes(nindex + 1, overtime);
        }

        return overtime;
    }

    public override QuatT NextPosition(float dt)
    {
        QuatT qt = QuatT.Zero;

        if (mNextPt >= mPts.Count)
        {
            return qt;
        }

        ToNextNode(dt);

        qt.T = mCurPos;
        qt.Rot = mCurRot;

        //SkipNodes(dt);

        //Pt prev = mPts[mNextPt - 1];
        //Pt next = mPts[mNextPt];

        //float t = mAccumulatedTime / next.Dt;
        //t = Mathf.Clamp(t, 0f, 1.0f);
        //qt.T = Vector3.Lerp(prev.Pos, next.Pos, t);
        //qt.Rot = Quaternion.Lerp(prev.Rot, next.Rot, t);

        return qt;
    }

    public override bool PathEnded()
    {
        if (mNextPt >= mPts.Count)
        {
            return true;
        }

        return mAccumulatedTime >= mPts[mNextPt].Dt;
    }

    public override Vector3 CurVeloc()
    {
        return mCurVeloc;
    }

    public override Vector3 CurPos()
    {
        return mCurPos;
    }

    public Pt GetPtByIndex(int i)
    {
        if (i >= 0 && i < mPts.Count)
        {
            return mPts[i];
        }

        return MakePt(Vector3.zero, Quaternion.identity, 0);
    }

    public void IteratorPts(System.Action<Pt> ac)
    {
        for (int i = 0; i < mPts.Count; ++i)
        {
            ac(mPts[i]);
        }
    }

    public void SetNodeNotify(System.Action<Pt> nt)
    {
        mNodeNotify = nt;
    }

    System.Action<Pt> mNodeNotify;
    List<Pt> mPts = new List<Pt>();
    Vector3 mCurVeloc;
    Vector3 mCurPos;
    Quaternion mCurRot;
    float mAccumulatedTime;         // accumulate between prev & next
    int mNextPt = 0;
}

public class CurvePath : ActorPath
{
    public override TypeEnum GetTypeEnum()
    {
        return ActorPath.TypeEnum.AutoCurvePath;
    }

    public override QuatT Start()
    {
        mElapsed = 0;
        mAccDuration = mAcceleratorLife;
        mVeloc = mInitialVeloc;
        mCurPos = mInitialQT.T;

        return mInitialQT;
    }

    public override QuatT Reset()
    {
        mElapsed = 0;
        mAccDuration = mAcceleratorLife;
        mVeloc = mInitialVeloc;
        mCurPos = mInitialQT.T;

        return mInitialQT;
    }

    public override QuatT NextPosition(float dt)
    {
        QuatT qt = QuatT.Zero;

        if (mElapsed + dt > mTimedout)
        {
            dt = mTimedout - mElapsed;
        }

        float accDt = 0;

        if (mAccDuration > 0)
        {
            if (mAccDuration < dt)
            {
                accDt = mAccDuration;
            }
            else
            {
                accDt = dt;
            }

            mAccDuration -= dt;
            if (mAccDuration <= 0)
            {
                mAccDuration = 0;
            }
        }
        else if (mAccDuration <= -1f)
        {
            accDt = dt;
        }

        mVeloc = mVeloc + mAcceleraction * accDt;

        mCurPos = mCurPos + mVeloc * dt;
        qt.T = mCurPos;

        float sqrMgr = mVeloc.sqrMagnitude;
        if (sqrMgr <= 0.000001f)
        {
            qt.Rot = Quaternion.identity;
        }
        else
        {
            qt.Rot = Quaternion.LookRotation(mVeloc.normalized, Game.Up);
        }

        return qt;
    }

    public override bool PathEnded()
    {
        return mElapsed >= mTimedout;
    }

    public override Vector3 CurVeloc()
    {
        return mVeloc;
    }

    public override Vector3 CurPos()
    {
        return mCurPos;
    }

    public static void ConfigPathWithTarget(CurvePath path, Transform initialTf, Vector3 initialVeloc, Vector3 targetLoc,
        float timedout)
    {
        float inv_t2 = 2.0f / (timedout * timedout);
        path.mAcceleraction = inv_t2 * (targetLoc - initialTf.position - (initialVeloc * timedout));
        path.mVeloc = initialVeloc;
        path.mTimedout = timedout;
        path.mInitialVeloc = initialVeloc;
        path.mInitialQT.T = initialTf.position;
        path.mInitialQT.Rot = initialTf.rotation;
        path.mAcceleratorLife = -1;
    }

    public static void ConfigPath(CurvePath path, Transform initialTf, Vector3 initialVeloc, Vector3 acc,
        float accLife)
    {
        path.mAcceleraction = acc;
        path.mVeloc = initialVeloc;
        path.mTimedout = ActorPath.NeverTimedout;
        path.mInitialVeloc = initialVeloc;
        path.mInitialQT.T = initialTf.position;
        path.mInitialQT.Rot = initialTf.rotation;
        path.mAcceleratorLife = accLife;
    }

    public static void ConfigPath(CurvePath path, Vector3 position, Quaternion rotation, Vector3 initialVeloc, Vector3 acc,
        float accLife)
    {
        path.mAcceleraction = acc;
        path.mVeloc = initialVeloc;
        path.mTimedout = ActorPath.NeverTimedout;
        path.mInitialVeloc = initialVeloc;
        path.mInitialQT.T = position;
        path.mInitialQT.Rot = rotation;
        path.mAcceleratorLife = accLife;
    }
    public Vector3 mVeloc;
    public Vector3 mAcceleraction;
    public float mTimedout;
    public float mAcceleratorLife = -1f;
    QuatT mInitialQT;
    Vector3 mInitialVeloc;
    Vector3 mCurPos;
    float mElapsed;
    float mAccDuration;
}

public class ActorPathMotor
{
    public float SpeedMultipiler
    {
        get { return mSpeedMultiplier; }

        set
        {
            mSpeedMultiplier = Mathf.Clamp(value, 0.0001f, 10000.0f);
        }
    }

    public GameObject GameObject_
    {
        get
        {
            return mObject;
        }
    }

    public void OnGameUpdate(float dt)
    {
        if (mPath != null && !mPath.PathEnded())
        {
            ActorPath.QuatT qt = mPath.NextPosition(dt * mSpeedMultiplier);

            if (mRotatePath)
            {
                mObject.transform.position = mLastQuatT.Rot * qt.T + mLastQuatT.T;
                mObject.transform.rotation = qt.Rot * mLastQuatT.Rot;
            }
            else
            {
                mObject.transform.position = qt.T + mLastQuatT.T;
                mObject.transform.rotation = qt.Rot;
            }

            if (mCallbackOnPathUpdate != null)
            {
                mCallbackOnPathUpdate(qt);
            }

            if (mPath.PathEnded() && mCallbackOnPathEnd != null)
            {
                mCallbackOnPathEnd(mObject);
            }
        }
    }

    public void SetPath(ActorPath path, ActorPath.QuatT curQt, bool rotatePath = false,
        System.Action<NPPath.Pt> nodeNT = null,
        System.Action<GameObject> onPathEnd = null,
        System.Action<ActorPath.QuatT> onPathUpdate = null)
    {
        SpeedMultipiler = 1;
        mPath = path;
        mLastQuatT = curQt;

        mNPNodeNotify = nodeNT;
        mCallbackOnPathEnd = onPathEnd;
        mCallbackOnPathUpdate = onPathUpdate;
        mRotatePath = rotatePath;

        if (mPath != null && path.GetTypeEnum() == ActorPath.TypeEnum.NodeBasedPath)
        {
            NPPath np = mPath as NPPath;
            np.SetNodeNotify(OnNodeNotify);
        }

        if (mPath != null)
        {
            ActorPath.QuatT qt = mPath.Start();

            if (rotatePath)
            {
                mObject.transform.position = mLastQuatT.Rot * qt.T + mLastQuatT.T;
                mObject.transform.rotation = qt.Rot * mLastQuatT.Rot;
            }
            else
            {
                mObject.transform.position = qt.T + mLastQuatT.T;
                mObject.transform.rotation = qt.Rot;//这里逆转了敌机的rotation
            }
        }
    }

    void OnNodeNotify(NPPath.Pt pt)
    {
        if (mNPNodeNotify != null)
        {
            mNPNodeNotify(pt);
        }
    }

    public ActorPathMotor(GameObject g)
    {
        mObject = g;
    }

    public void DebugDrawPath()
    {
        Color c = Color.red;
        c.a = 0.5f;
        Gizmos.color = c;

        if (mPath != null
            && mPath.GetTypeEnum() == ActorPath.TypeEnum.NodeBasedPath)
        {
            NPPath np = (NPPath)mPath;

            if (mRotatePath)
            {
                np.IteratorPts((pt) =>
                {
                    if (pt.KeyPt)
                    {
                        Vector3 absPos = mLastQuatT.Rot * pt.Pos + mLastQuatT.T;
                        Gizmos.DrawSphere(absPos, 0.2f);
                    }
                });
            }
            else
            {
                np.IteratorPts((pt) =>
                {
                    if (pt.KeyPt)
                    {
                        Vector3 absPos = pt.Pos + mLastQuatT.T;
                        Gizmos.DrawSphere(absPos, 0.2f);
                    }
                });
            }
        }
    }

    public ActorPath Path
    {
        get
        {
            return mPath;
        }
    }

    ActorPath mPath;
    ActorPath.QuatT mLastQuatT;
    System.Action<NPPath.Pt> mNPNodeNotify;
    System.Action<GameObject> mCallbackOnPathEnd;
    System.Action<ActorPath.QuatT> mCallbackOnPathUpdate;
    GameObject mObject;
    float mSpeedMultiplier = 1.0f;
    bool mRotatePath;
}