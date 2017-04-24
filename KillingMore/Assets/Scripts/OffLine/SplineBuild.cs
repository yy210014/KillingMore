using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Emitter;

public class SplineBuild : MonoBehaviour
{
    public PathTester FocusTester;

    [Tooltip("以相机为中心的半径范围外剔除子弹")]
    public float ProjectileClipLife = 5.0f;
    //public bool AutoScale;
    //public float CameraHeight = 55.0f;

    List<PathTester> mTesters = new List<PathTester>();
    List<PathEditGroup> mPathObjs = new List<PathEditGroup>();

    public static SplineBuild Singleton
    {
        get
        {
            return msSingleton;
        }
    }

    static SplineBuild msSingleton = null;

    void Awake()
    {
        msSingleton = this;
    }

    void Start()
    {
        //if (FocusTester != null)
        //{
        //if (AutoScale)
        //{
        Invoke("StartTesters", 1.0f);
        //}
        //}
    }

    void StartTesters()
    {
        AssetsManager.Singleton.Initialize();
        GameScene.Singleton.OnGameInitialize();

        for (int i = 0; i < mTesters.Count; ++i)
        {
            mTesters[i].Prepare();
        }

        GameScene.Singleton.OnGameStart();
        ApplyAutoScale();
        GameScene.Singleton.SetProjectileCliper(ProjectileClip);
    }

    void ApplyAutoScale()
    {
    }

    bool ProjectileClip(Projectile p)
    {
        return p.MaxLife >= ProjectileClipLife;
    }

    public void RegisterTester(PathTester pt)
    {
        if (!mTesters.Contains(pt))
        {
            mTesters.Add(pt);
        }
    }

    public void RegisterPathObj(PathEditGroup obj)
    {
        if (!mPathObjs.Contains(obj))
        {
            mPathObjs.Add(obj);
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            Invoke("SerializeAllPaths", 0.5f);
        }

        GameScene.Singleton.OnGameUpdate();
        AssetsManager.Singleton.UpdateTasks();
    }

    public void SerializeAllPaths()
    {
        PathManager.Singleton.SavePaths(mPathObjs);

#if UNITY_EDITOR

        Game.Log("Path serialized!!");

#endif
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 20, 100, 50), "Press 'R' to reset all path-testers.");
        GUI.Label(new Rect(10, 70, 100, 50), "Press 'S' to serialize all paths.");
    }
}

public class SplineBuildMath
{
    public static List<Vector3> InterpolateCR(Vector3[] pathPts, int slices,
        float minInterpolateDist = 0.2f)
    {
        int curIndex = 0;
        List<Vector3> outputs = new List<Vector3>();
        outputs.Add(pathPts[0]);

        int stepCount = slices + 1;
        float minSqrDis = minInterpolateDist * minInterpolateDist;

        for (int i = 0; i < pathPts.Length; ++i)
        {
            Vector3 p0 = pathPts[curIndex];
            if (curIndex > 0)
            {
                p0 = pathPts[curIndex - 1];
            }

            Vector3 p1 = pathPts[curIndex];
            Vector3 p2 = p1;
            int p2Index = curIndex < pathPts.Length - 1 ? curIndex + 1 : curIndex;
            p2 = pathPts[p2Index];

            if (p2Index == curIndex)
            {
                // the same point
                outputs.Add(p1);
                ++curIndex;
                continue;
            }

            float sqrDist = Vector3.SqrMagnitude(p1 - p2);
            if (sqrDist < minSqrDis)
            {
                // too close.
                outputs.Add(p1);
                outputs.Add(p2);
                ++curIndex;
                continue;
            }

            Vector3 p3 = p2;
            if (curIndex < pathPts.Length - 2)
            {
                p3 = pathPts[curIndex + 2];
            }

            for (int step = 1; step <= stepCount; step++)
            {
                Vector3 v = CatmullRom(p0, p1, p2, p3, (float)step, (float)stepCount);
                outputs.Add(v);
            }
            //ComputeCenteriprtalCR(p0, p1, p2, p3, slices, outputs);

            ++curIndex;
        }

        return outputs;
    }

    class _PathPoint
    {
        public Vector3 Pos;
        public Quaternion Rot;
        public float Veloc;
        public bool KeyPt;
        public short Behavior;
        public float BehaviorDuration;
        public float Disturbance;

        // 用于剔除重复的关键点
        public PathEditObj Src;

        public _PathPoint()
        {
        }

        public _PathPoint(Vector3 p, Quaternion r, float v)
        {
            Pos = p;
            Rot = r;
            Veloc = v;
            Behavior = 0;
            BehaviorDuration = 0;
            Disturbance = 0;
            Src = null;
        }

        public _PathPoint(Vector3 p, Quaternion r, float v, bool k, PathEditObj src)
        {
            Pos = p;
            Rot = r;
            Veloc = v;
            KeyPt = k;
            Behavior = (short)src.Behavior;
            BehaviorDuration = src.BehaviorDuration;
            Disturbance = src.Disturbance;
            Src = src;
        }

        public static _PathPoint From(PathEditObj src)
        {
            return new _PathPoint(src.transform.position, src.transform.rotation,
                src.Velocity, true, src);
        }
    }

    static void AddKeyPt(List<_PathPoint> lst, PathEditObj src)
    {
        for (int i = 0; i < lst.Count; ++i)
        {
            if (lst[i].Src != null && lst[i].Src == src)
            {
                // 重复的key point
                return;
            }
        }

        lst.Add(_PathPoint.From(src));
    }

    static List<_PathPoint> InterpolateCR(PathEditObj[] pathObjGroup, int slices,
        float minInterpolateDist = 0.2f)
    {
        int curIndex = 0;
        List<_PathPoint> outputs = new List<_PathPoint>();
        outputs.Add(_PathPoint.From(pathObjGroup[0]));

        int stepCount = slices + 1;
        float minSqrDis = minInterpolateDist * minInterpolateDist;

        for (int i = 0; i < pathObjGroup.Length; ++i)
        {
            Vector3 p0 = pathObjGroup[curIndex].transform.position;
            if (curIndex > 0)
            {
                p0 = pathObjGroup[curIndex - 1].transform.position;
            }

            Vector3 p1 = pathObjGroup[curIndex].transform.position;
            Vector3 p2 = p1;
            int p2Index = curIndex < pathObjGroup.Length - 1 ? curIndex + 1 : curIndex;
            p2 = pathObjGroup[p2Index].transform.position;

            if (p2Index == curIndex)
            {
                // the same point
                AddKeyPt(outputs, pathObjGroup[curIndex]);
                //outputs.Add(_PathPoint.From(pathObjGroup[curIndex]));

                ++curIndex;
                continue;
            }

            float sqrDist = Vector3.SqrMagnitude(p1 - p2);
            if (sqrDist < minSqrDis)
            {
                // too close.
                AddKeyPt(outputs, pathObjGroup[curIndex]);
                AddKeyPt(outputs, pathObjGroup[p2Index]);
                //outputs.Add(new _PathPoint(pathObjGroup[curIndex].transform.position,
                //    pathObjGroup[curIndex].transform.rotation, pathObjGroup[curIndex].Velocity, true));

                //outputs.Add(new _PathPoint(pathObjGroup[p2Index].transform.position,
                //    pathObjGroup[p2Index].transform.rotation, pathObjGroup[p2Index].Velocity, true));

                ++curIndex;
                continue;
            }

            Vector3 p3 = p2;
            if (curIndex < pathObjGroup.Length - 2)
            {
                p3 = pathObjGroup[curIndex + 2].transform.position;
            }

            for (int step = 1; step <= stepCount; step++)
            {
                Vector3 v = CatmullRom(p0, p1, p2, p3, (float)step, (float)stepCount);

                // 插值的还需要post process
                if (stepCount == step)
                {
                    //PathEditObj keyObj = pathObjGroup[p2Index];
                    //outputs.Add(new _PathPoint(keyObj.transform.position,
                    //    keyObj.transform.rotation, keyObj.Velocity, true,
                    //    (short)keyObj.Behavior, keyObj.BehaviorDuration));
                    AddKeyPt(outputs, pathObjGroup[p2Index]);
                }
                else
                {
                    outputs.Add(new _PathPoint(v, Quaternion.identity, 0));
                }
            }
            //ComputeCenteriprtalCR(p0, p1, p2, p3, slices, outputs);

            ++curIndex;
        }

        return outputs;
    }

    static void _PathPtsPostProcess(_PathPoint[] arr, int num, _PathPoint st, _PathPoint en)
    {
        if (num <= 0)
        {
            return;
        }

        // accumulate distance
        float totalS = 0;
        float[] accuS = new float[num];

        for (int i = 0; i < num; ++i)
        {
            if (i == 0)
            {
                totalS = Vector3.Magnitude(st.Pos - arr[i].Pos);
            }
            else
            {
                totalS += Vector3.Magnitude(arr[i].Pos - arr[i - 1].Pos);
            }

            accuS[i] = totalS;
        }

        totalS += Vector3.Magnitude(en.Pos - arr[num - 1].Pos);

        for (int i = 0; i < num; ++i)
        {
            accuS[i] /= totalS;
        }

        for (int i = 0; i < num; ++i)
        {
            arr[i].Rot = Quaternion.Lerp(st.Rot, en.Rot, accuS[i]);
            arr[i].Veloc = Mathf.Lerp(st.Veloc, en.Veloc, accuS[i]);
        }
    }

    public static NPPath InterpolateCR(GameObject pathObjGroup, int slices,
        float minInterpolateDist = 0.2f)
    {
        NPPath outPath = new NPPath();

        List<PathEditObj> editObjs = new List<PathEditObj>();
        foreach (Transform tf in pathObjGroup.transform)
        {
            PathEditObj peo = tf.GetComponent<PathEditObj>();
            editObjs.Add(peo);
        }

        if (editObjs.Count == 0)
        {
            return outPath;
        }

        List<_PathPoint> pts = InterpolateCR(editObjs.ToArray(), slices, minInterpolateDist);

        // post process
        _PathPoint[] _process = new _PathPoint[slices];
        _PathPoint start = null;
        _PathPoint end = null;
        int flag = 0;
        int ppCount = 0;

        for (int i = 0; i < pts.Count; ++i)
        {
            if (pts[i].KeyPt)
            {
                if (flag == 0)
                {
                    start = pts[i];
                    ++flag;
                    ppCount = 0;
                }
                else if (flag == 1)
                {
                    end = pts[i];
                    _PathPtsPostProcess(_process, ppCount, start, end);
                    start = pts[i];
                    ppCount = 0;
                }
            }
            else
            {
                if (ppCount >= slices)
                {
                    Game.LogError("插值路径点数量大于切片数量！");
                    continue;
                }

                _process[ppCount] = pts[i];
                ++ppCount;
            }
        }

        // _PathPoints 2 path
        List<NPPath.Pt> npts = new List<NPPath.Pt>();

        // 取起始点作为相对位置开端
        Vector3 relPos = pts[0].Pos; //pathObjGroup.transform.position;
        for (int i = 0; i < pts.Count; ++i)
        {
            if (pts[i].KeyPt)
            {
                npts.Add(NPPath.MakePt(pts[i].Pos - relPos, pts[i].Rot, pts[i].Veloc,
                    pts[i].Behavior, pts[i].BehaviorDuration, true, pts[i].Disturbance));
            }
            else
            {
                npts.Add(NPPath.MakePt(pts[i].Pos - relPos, pts[i].Rot, pts[i].Veloc));
            }
        }

        outPath.ApplyPath(npts);

        return outPath;
    }

    public static Vector3 CatmullRom(Vector3 previous, Vector3 start, Vector3 end, Vector3 next,
                                float elapsedTime, float duration)
    {
        float percentComplete = elapsedTime / duration;
        float percentCompleteSquared = percentComplete * percentComplete;
        float percentCompleteCubed = percentCompleteSquared * percentComplete;

        return previous * (-0.5f * percentCompleteCubed +
                                   percentCompleteSquared -
                            0.5f * percentComplete) +
                start * (1.5f * percentCompleteCubed +
                           -2.5f * percentCompleteSquared + 1.0f) +
                end * (-1.5f * percentCompleteCubed +
                            2.0f * percentCompleteSquared +
                            0.5f * percentComplete) +
                next * (0.5f * percentCompleteCubed -
                            0.5f * percentCompleteSquared);
    }

    public class CubicPlyPhase
    {
        public float c0, c1, c2, c3;

        public float Evaluate(float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return c0 + c1 * t + c2 * t2 + c3 * t3;
        }

        public void Initialize(float x0, float x1, float x2, float x3, float dt0, float dt1, float dt2)
        {
            // compute tangents when parameterized in [t1,t2]
            float t1 = (x1 - x0) / dt0 - (x2 - x0) / (dt0 + dt1) + (x2 - x1) / dt1;
            float t2 = (x2 - x1) / dt1 - (x3 - x1) / (dt1 + dt2) + (x3 - x2) / dt2;

            // rescale tangents for parametrization in [0,1]
            t1 *= dt1;
            t2 *= dt1;

            c0 = x0;
            c1 = t1;
            c2 = -3 * x0 + 3 * x1 - 2 * t1 - t2;
            c3 = 2 * x0 - 2 * x1 + t1 + t2;
        }
    }

    public static void ComputeCenteriprtalCR(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int slices, List<Vector3> pts)
    {
        CubicPlyPhase px = new CubicPlyPhase();
        CubicPlyPhase py = new CubicPlyPhase();
        CubicPlyPhase pz = new CubicPlyPhase();

        float dt0 = Mathf.Pow(Vector3.SqrMagnitude(p0 - p1), 0.25f);
        float dt1 = Mathf.Pow(Vector3.SqrMagnitude(p1 - p2), 0.25f);
        float dt2 = Mathf.Pow(Vector3.SqrMagnitude(p2 - p3), 0.25f);

        // safety check for repeated points
        if (dt1 < 1e-4f) dt1 = 1.0f;
        if (dt0 < 1e-4f) dt0 = dt1;
        if (dt2 < 1e-4f) dt2 = dt1;

        px.Initialize(p0.x, p1.x, p2.x, p3.x, dt0, dt1, dt2);
        py.Initialize(p0.y, p1.y, p2.y, p3.y, dt0, dt1, dt2);
        pz.Initialize(p0.z, p1.z, p2.z, p3.z, dt0, dt1, dt2);

        float dt = 1.0f / (float)slices;
        float t = 0;
        for (int i = 0; i < slices; ++i)
        {
            Vector3 v = Vector3.zero;
            v.x = px.Evaluate(t);
            v.y = py.Evaluate(t);
            v.z = pz.Evaluate(t);
            t += dt;

            pts.Add(v);
        }
    }




}