using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using LitJson;

public class ClampedRange<T> where T : System.IComparable
{
    protected T mValue;
    protected T mMax;
    protected T mMin;

    public T Value
    {
        get
        {
            return mValue;
        }
    }

    public T Max
    {
        get
        {
            return mMax;
        }
    }

    public T Min
    {
        get
        {
            return mMin;
        }
    }

    public ClampedRange()
    {

    }

    public ClampedRange(T _min, T _max, T initialVal)
    {
        mValue = initialVal;
        mMax = _max;
        mMin = _min;
    }

    public ClampedRange(T _min, T _max)
    {
        mMax = _max;
        mMin = _min;
    }

    public ClampedRange(T _max)
    {
        mMax = _max;
    }

    public bool IsOverFlow
    {
        get
        {
            return mValue.CompareTo(mMax) >= 0;
        }
    }

    public bool IsInFlow
    {
        get
        {
            return mValue.CompareTo(mMax) < 0;
        }
    }

    public bool TouchedBottom
    {
        get
        {
            return mValue.CompareTo(mMin) <= 0;
        }
    }

    public void Reset(T v)
    {
        mValue = v;
    }

    public void Reset(T _min, T _max)
    {
        mMin = _min;
        mMax = _max;
        mValue = mMin;
    }

    public void Reset(T _min, T _max, T val)
    {
        mMin = _min;
        mMax = _max;
        mValue = val;
    }
}

public class IntClampedRange : ClampedRange<int>
{
    public void AddDt(int dt)
    {
        mValue = Mathf.Clamp(mValue + dt, mMin, mMax);
    }
}

public class Int64ClampedRange : ClampedRange<Int64>
{
    public void AddDt(int dt)
    {
        mValue += dt;
        if (mValue < mMin)
            mValue = mMin;
        else if (mValue > mMax)
            mValue = mMax;
    }
}

public class FloatClampedRange : ClampedRange<float>
{
    public void AddDt(float dt)
    {
        mValue = Mathf.Clamp(mValue + dt, mMin, mMax);
    }
}

public class DoubleClampedRange : ClampedRange<double>
{
    public void AddDt(int dt)
    {
        mValue += dt;
        if (mValue < mMin)
            mValue = mMin;
        else if (mValue > mMax)
            mValue = mMax;
    }
}

public enum StructInitial
{
    Default
}


public class Misc
{
    public const string String_Unknown = "Unknown";


    public static List<T> Permute<T>(List<T> list)
    {
        T[] t = list.ToArray();
        for (int i = 1; i < list.Count; i++)
        {
            Swap<T>(ref t, i, UnityEngine.Random.Range(0, i + 1));
        }
        return new List<T>(t);
    }

    public static void Permute<T>(T[] array)
    {
        System.Random random = new System.Random();
        for (int i = 1; i < array.Length; i++)
        {
            Swap<T>(ref array, i, random.Next(0, i));
        }
    }

    private static void Swap<T>(ref T[] array, int indexA, int indexB)
    {
        T temp = array[indexA];
        array[indexA] = array[indexB];
        array[indexB] = temp;
    }

    public static void Swap<T>(ref T lhs, ref T rhs)
    {
        T tmp = lhs;
        lhs = rhs;
        rhs = tmp;
    }

    public static int Round2Int(float f, float err = 0.001f)
    {
        float i = Mathf.Floor(f);
        float frac = f - i;

        if (frac + err >= 1.0f)
            return (int)(i + 1.0f);

        return (int)i;
    }

    public static float FracIfGreater(float v, float f)
    {
        float n = (float)((int)(v / f));
        if (n <= 0)
            n = 1;

        return v - n * f;
    }

    public static bool FlagAnd(int flag, int mask)
    {
        return (flag & mask) != 0;
    }

    public static bool FlagXOr(int flag, int mask)
    {
        return (flag ^ mask) != 0;
    }

    public static int[] Split2IntArray(string str)
    {
        string[] idsStr = str.Split(new char[] { '|', '.', '/', '\\', ':', ',' });

        int[] ret = new int[idsStr.Length];
        int i = 0;
        foreach (var v in idsStr)
        {
            ret[i++] = System.Convert.ToInt32(v);
        }

        return ret;
    }

    public static string[] Split(string str, params char[] separator)
    {
        return str.Split(separator);
    }

    public static void CopyString2CharArray(string s, char[] arr)
    {
        int len = Mathf.Min(s.Length + 1, arr.Length);
        for (int i = 0; i < len - 1; ++i)
        {
            arr[i] = s[i];
        }

        arr[len - 1] = (char)0;
    }

    public static void CopyString2ByteArray(string s, byte[] arr)
    {
        byte[] bf = System.Text.Encoding.UTF8.GetBytes(s);
        int len = bf.Length;
        for (int i = 0; i < len; ++i)
        {
            arr[i] = bf[i];
        }

        for (int i = len; i < arr.Length; ++i)
        {
            arr[i] = (byte)0;
        }
    }

    public static string ByteGBToUtf8(byte[] data)
    {
        return System.Text.Encoding.UTF8.GetString(data).TrimEnd('\0');
    }

    public static uint GetHWord(uint n)
    {
        return (uint)(n & 0xFFFF0000) >> 16;
    }

    public static uint GetLWord(uint n)
    {
        return (uint)(n & 0xFFFF);
    }
}

public class AlgebraHelper
{
    public static Vector3 GetQuaternionForward(Quaternion v)
    {
        Vector3 vv = Vector3.zero;
        vv.Set(2 * (v.x * v.z + v.y * v.w), 2 * (v.y * v.x - v.w * v.x), 1 - 2.0f * (v.x * v.x + v.y * v.y));
        return vv;
    }

    public static Vector3 GetQuaternionRight(Quaternion v)
    {
        Vector3 vv = Vector3.zero;
        vv.Set(1 - 2 * (v.y * v.y + v.z * v.z), 2 * (v.x * v.y + v.w * v.z), 2 * (v.x * v.z + v.w * v.y));
        return vv;
    }

    public static Vector3 GetQuaternionUp(Quaternion v)
    {
        Vector3 vv = Vector3.zero;
        vv.Set(2 * (v.x * v.y - v.w * v.z), 1 - 2 * (v.x * v.x + v.z * v.z), 2 * (v.y * v.z + v.x * v.w));
        return vv;
    }

    // ray = o + d * t
    // pt(u, v) = (1-u-v)*p1 + u*p2 + v*p3
    // ray = o + d * t
    public static bool RayIntersectTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 o, Vector3 d, ref float t)
    {
        Vector3 edge1 = p2 - p1;
        Vector3 edge2 = p3 - p1;
        Vector3 dxe2 = Vector3.Cross(d, edge2);
        float dxe2_d_e1 = Vector3.Dot(edge1, dxe2);

        if (dxe2_d_e1 > -Mathf.Epsilon && dxe2_d_e1 < Mathf.Epsilon)
        {
            return false;
        }

        float f = 1.0f / dxe2_d_e1;
        Vector3 diff = o - p1;
        float u = f * Vector3.Dot(diff, dxe2);
        if (u < 0.0f || u > 1.0f)
        {
            return false;
        }

        Vector3 diffxe1 = Vector3.Cross(diff, edge1);
        float v = f * Vector3.Dot(d, diffxe1);
        if (v < 0.0f || u + v > 1.0f)
        {
            return false;
        }

        t = f * Vector3.Dot(edge2, diffxe1);
        if (t > Mathf.Epsilon)
        {
            return true;
        }

        return false;
    }

    // P = A + u * (C - A) + v * (B - A)
    public static bool IsPointInsideTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        Vector3 v0 = C - A;
        Vector3 v1 = B - A;
        Vector3 v2 = P - A;

        float dot00 = Vector3.Dot(v0, v0);
        float dot01 = Vector3.Dot(v0, v1);
        float dot02 = Vector3.Dot(v0, v2);
        float dot11 = Vector3.Dot(v1, v1);
        float dot12 = Vector3.Dot(v1, v2);

        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;

        if (u < 0)
            return false;

        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (v >= 0) && (u + v < 1);
    }

    // cylinder test with sphere, any axis.
    public static bool CylinderSphereTest(Vector3 cylinderCenter, Vector3 axis, float cyr, float cyh, Vector3 cc, float cr)
    {
        // project to its cross profile, N*P = D;
        float D = Vector3.Dot(cylinderCenter, axis);
        Vector3 diff = cc - cylinderCenter;

        float t = Vector3.Dot(cc, axis) - D;
        Vector3 projOnCP = cc - axis * t;

        float testSqrR = (cyr + cr) * (cyr + cr);
        float sqrDis = Vector3.SqrMagnitude(cylinderCenter - projOnCP);

        if (testSqrR < sqrDis)
        {
            return false;
        }

        float projY = Mathf.Abs(Vector3.Dot(axis, diff));
        Vector3 fake_X = projOnCP - cylinderCenter;
        fake_X.Normalize();
        float projX = Mathf.Abs(Vector3.Dot(fake_X, diff));

        if (projX > (cyr + cr) || projY > (cyh * 0.5f + cr))
        {
            return false;
        }

        if (projX > cyr && projY > (cyh * 0.5f))
        {
            float dx = projX - cyr;
            float dy = projY - (cyh * 0.5f);

            return (dx * dx + dy * dy) <= (cyr * cyr);
        }

        return true;
    }

    // cylinder test with a sphere, Y-Axis
    public static bool YAixsCylinderSphereTest(Vector3 cylinderCenter, float cyr, float cyh, Vector3 cc, float cr)
    {
        Vector2 diff_2d = new Vector2(cylinderCenter.x - cc.x, cylinderCenter.z - cc.z);
        float sqrR = (cyr + cr) * (cyr + cr);
        float sqrDis = Vector2.Dot(diff_2d, diff_2d);
        if (sqrR < sqrDis)
        {
            return false;
        }

        Vector3 diff_3d = cc - cylinderCenter;
        float projY = Mathf.Abs(Vector3.Dot(Vector3.up, diff_3d));
        Vector3 fake_X = diff_3d;
        fake_X.y = 0;
        fake_X.Normalize();
        float projX = Mathf.Abs(Vector3.Dot(fake_X, diff_3d));

        if (projX > (cyr + cr) || projY > (cyh * 0.5f + cr))
        {
            return false;
        }

        if (projX > cyr && projY > (cyh * 0.5f))
        {
            float dx = projX - cyr;
            float dy = projY - (cyh * 0.5f);

            return (dx * dx + dy * dy) <= (cyr * cyr);
        }

        return true;
    }

    public enum SphereCastResult
    {
        None = 0,
        OnCollision,
        PreCollision
    }

    // just horizontal, ignore y & z
    public static SphereCastResult SphereCast1D(Vector3 pos1, Vector3 delta1, float r1, Vector3 pos2, Vector3 delta2, float r2, ref Vector3 contractAt)
    {
        pos1.y = pos2.y = pos1.z = pos2.z = 0;
        delta1.y = delta1.z = delta2.y = delta2.z = 0;

        return SphereCast(pos1, delta1, r1, pos2, delta2, r2, ref contractAt);
    }

    // just x-y plane
    public static SphereCastResult SphereCast2D(Vector3 pos1, Vector3 delta1, float r1, Vector3 pos2, Vector3 delta2, float r2, ref Vector3 contractAt)
    {
        pos1.y = pos2.y = 0;
        delta1.y = delta2.y = 0;

        return SphereCast(pos1, delta1, r1, pos2, delta2, r2, ref contractAt);
    }

    // 3d sphere cast
    public static SphereCastResult SphereCast(Vector3 pos1, Vector3 delta1, float r1, Vector3 pos2, Vector3 delta2, float r2, ref Vector3 contractAt)
    {
        float sqrDis = Vector3.SqrMagnitude(pos1 - pos2);
        float sqrR = r1 + r2;
        sqrR *= sqrR;

        if (sqrDis <= sqrR)
        {
            Vector3 dir = (pos2 - pos1).normalized;
            contractAt = pos1 + dir * (r1 + r2) * 0.5f;
            return SphereCastResult.OnCollision;
        }

        Vector3 a = pos1 - pos2;
        Vector3 b = delta1 - delta2;

        float b2 = Vector3.Dot(b, b);

        // 永远不会碰撞
        if (Mathf.Abs(b2) <= Mathf.Epsilon)
        {
            return SphereCastResult.None;
        }

        float ab = Vector3.Dot(a, b);
        float a2 = Vector3.Dot(a, a);


        float bSqrMinus4AC = (ab * ab) - b2 * (a2 - sqrR);

        if (bSqrMinus4AC < 0)
        {
            return SphereCastResult.None;
        }

        float t = -1.0f * (ab + Mathf.Sqrt(bSqrMinus4AC)) / b2;

        if (t >= 0 && t <= 1.0f)
        {
            contractAt = pos1 + delta1 * t;
            return SphereCastResult.PreCollision;
        }

        return SphereCastResult.None;
    }

    public static bool Inside2DBox(Vector2 pos, Vector2 boxCenter, Vector2 boxSize)
    {
        if (pos.x >= boxCenter.x - boxSize.x * 0.5f
            && pos.x <= boxCenter.x + boxSize.x * 0.5f)
        {
            return pos.y >= boxCenter.y - boxSize.y * 0.5f
                && pos.y <= boxCenter.y + boxSize.y * 0.5f;
        }

        return false;
    }

    public static bool InsideRect(Vector2 pos, float x, float y, float w, float h)
    {
        return pos.x >= x && pos.x <= x + w
            && pos.y >= y && pos.y <= y + h;
    }

    public static bool InsideCircle(Vector2 pos, Vector2 center, float radius)
    {
        return Vector2.SqrMagnitude(pos - center) <= (radius * radius);
    }
}

public static class UtilExtensions
{
    public static Color SetAlpha(this Color c, float alpha)
    {
        Color b = c;
        b.a = alpha;
        return b;
    }

    static public bool IsActive(this GameObject go)
    {
        return go && go.activeInHierarchy;
    }

    static public T ForceGetComponent<T>(this GameObject g) where T : Component
    {
        T c = g.GetComponent<T>();
        if (c == null)
        {
            c = g.AddComponent<T>();
        }
        return c;
    }

    static public List<T> GetCompoentHierarchy<T>(this GameObject g) where T : Component
    {
        List<T> list = new List<T>();
        T[] c = g.GetComponents<T>();
        list.AddRange(c); ;
        T[] cs = g.GetComponentsInChildren<T>();
        list.AddRange(cs);

        return list;
    }
}

public class Schedule
{
    float mTime = 0.0001f;
    System.Action<Schedule> mDelegate;
    bool mCanceled = false;
    bool mUseWorldTimeScale = true;

    public bool Canceled
    {
        get
        {
            return mCanceled;
        }
    }

    public float Time
    {
        get { return mTime; }
    }

    public bool UsingWorldTimeScale
    {
        get { return mUseWorldTimeScale; }
    }

    public System.Action<Schedule> Callback
    {
        get { return mDelegate; }
    }

    public Schedule()
    {
    }

    public Schedule(float t, System.Action<Schedule> callback)
    {
        mTime = t;
        mDelegate = callback;
    }

    public Schedule(float t, bool worldTimeScale, System.Action<Schedule> callback)
    {
        mTime = t;
        mUseWorldTimeScale = worldTimeScale;
        mDelegate = callback;
    }

    public void Cancel()
    {
        mCanceled = true;
    }

    public void ResetTime(float t)
    {
        mTime = t;
    }
}

public class Timer
{
    public static Timer Singleton
    {
        get { return msSingleton; }
    }

    static readonly Timer msSingleton = new Timer();

    public void SetSchedule(float time, System.Action<Schedule> func)
    {
        Schedule sc = new Schedule(time, func);
        mFront.Add(sc);
    }

    public void SetSchedule(float time, bool usingWorldTimeScale, System.Action<Schedule> func)
    {
        Schedule sc = new Schedule(time, usingWorldTimeScale, func);
        mFront.Add(sc);
    }

    public void SetSchedule(Schedule sc)
    {
        if (mBack.Contains(sc))
        {
            return;
        }

        if (!mFront.Contains(sc))
        {
            mFront.Add(sc);
        }
    }

    public void Update()
    {
        if (mFront.Count > 0)
        {
            mBack.AddRange(mFront);//添加到 List 的末尾
            mFront.Clear();
        }

        float dt = Time.deltaTime;
        foreach (Schedule sc in mBack)
        {
            if (sc.Canceled)
            {
                mGarbrage.Add(sc);
                continue;
            }

            float tmpTime = sc.Time;
            // time scale.
            tmpTime -= sc.UsingWorldTimeScale ? (dt * GameScene.Singleton.GameTimeScale) : dt;
            //     tmpTime -= dt;
            sc.ResetTime(tmpTime);
            if (tmpTime <= 0)
            {
                if (sc.Callback != null)
                {
                    sc.Callback(sc);
                }

                // 如果重置了时间，不要放进去
                if (sc.Time <= 0)
                {
                    mGarbrage.Add(sc);
                }
            }
        }

        foreach (Schedule sc in mGarbrage)
        {
            mBack.Remove(sc);
        }

        mGarbrage.Clear();
    }

    // 双缓冲，防止计时器回调时更改计时器队列
    List<Schedule> mFront = new List<Schedule>();
    List<Schedule> mBack = new List<Schedule>();
    List<Schedule> mGarbrage = new List<Schedule>();
}

public class RawInput
{
    static Vector2 msLastFingerPos = Vector2.zero;
    static Vector2 msInvalidLocation = new Vector2(-10000, -10000);

    public static Vector2 InvalidLocation
    {
        get
        {
            return msInvalidLocation;
        }
    }

    public static Vector2 GetFingerPos()
    {
        // FIXME more platform.
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount > 0)
                msLastFingerPos = Input.touches[0].position;
        }
        else
        {
            msLastFingerPos = Input.mousePosition;
        }

        return msLastFingerPos;
    }

    public static int FingerTouchCount
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return Input.touchCount;
            }

            return 1;
        }
    }

    public static Vector2 GetFingerPosByIndex(int i)
    {
        if (i < FingerTouchCount)
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return Input.touches[i].position;
            }
            else
            {
                Vector2 pos = msInvalidLocation;
                if (i == 0)
                {
                    pos.x = Input.mousePosition.x;
                    pos.y = Input.mousePosition.y;
                }

                return pos;
            }
        }

        return msInvalidLocation;
    }

    public static bool IsPressing()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return Input.touchCount > 0;
        }

        return Input.GetMouseButton(0);
    }

    public static bool IsPressed()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount > 0)
            {
                return Input.GetTouch(0).phase == TouchPhase.Began;
            }
        }

        return Input.GetMouseButtonDown(0);
    }

    public static bool IsPressReleased()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount > 0)
            {
                return Input.GetTouch(0).phase == TouchPhase.Ended;
            }
        }

        return Input.GetMouseButtonUp(0);
    }

    public static Vector4 TransformScreenRc(Vector4 imageSpRc, float imageScH)
    {
        Vector4 rc = imageSpRc;
        rc.y = imageSpRc.y * Screen.height / imageScH;
        rc.y = Screen.height - rc.y;

        return rc;
    }

}