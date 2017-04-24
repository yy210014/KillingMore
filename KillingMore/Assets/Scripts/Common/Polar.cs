using UnityEngine;
using System.Collections;

[System.Serializable]
public class Polar
{
    public float dist;
    public float angle;
    public Polar(float dist, float angle)
    {
        this.dist = dist;
        this.angle = angle;
    }

    public static Vector3 PolarProjection(Vector3 source, Polar polar)
    {
        Vector3 p = Vector3.zero;
        float x = source.x + polar.dist * Mathf.Sin(polar.angle * Mathf.Deg2Rad);
        float z = source.z + polar.dist * Mathf.Cos(polar.angle * Mathf.Deg2Rad);
        p.Set(x, source.y, z);
        return p;
    }

    /// <summary>
    /// 角度转换为弧度: Mathf.Deg2Rad=Mathf.PI/180
    /// 弧度转换为角度: Mathf.Rad2Deg=?
    /// </summary>
    public static Vector3 PolarProjection(Vector3 source, float dist, float angle)
    {
        Vector3 p = Vector3.zero;
        float x = source.x + dist * Mathf.Sin(angle * Mathf.Deg2Rad);
        float z = source.z + dist * Mathf.Cos(angle * Mathf.Deg2Rad);
        p.Set(x, source.y, z);
        return p;
    }

    /// <summary>
    /// 向量的角度, 范围0~360
    /// </summary>
    /// <param name="forward"></param>
    /// <param name="upwards"></param>
    /// <returns></returns>
    public static float Angle(Vector3 from, Vector3 to)
    {
        return Vector3.Angle(from, to);
    }

    /// <summary>
    /// 向量的角度, 范围-180~180
    /// </summary>
    /// <param name="forward"></param>
    /// <param name="upwards"></param>
    /// <returns></returns>
    public static float Angle2(Vector3 forward, Vector3 upwards)
    {
        return Mathf.Atan2(forward.x, upwards.z != 0 ? forward.z : forward.y) * Mathf.Rad2Deg;
    }
}