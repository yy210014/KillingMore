using UnityEngine;
using System.Collections;

public enum ResourceCatelog
{
    Character,
}

public enum ResourceDetail
{
    Prefab,
}

public class ResourceNameUtil
{
    private const string linkString = "_";

    public static string BuildResourceName(int id, ResourceCatelog catelog, ResourceDetail detail)
    {
        return catelog.ToString() + linkString + detail.ToString() + linkString + id.ToString();
    }
}
