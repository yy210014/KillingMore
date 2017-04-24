using UnityEngine;
using System.Collections;

public class PathEditGroup : MonoBehaviour
{
    public int Slice = 4;
    public float MinInterpolateDist = 0.2f;

    public NPPath BuildNPPath()
    {
        return SplineBuildMath.InterpolateCR(gameObject, Slice, MinInterpolateDist);
    }

    void Start()
    {
        SplineBuild.Singleton.RegisterPathObj(this);
    }

    void Serialize()
    {

    }

    void Deserialize()
    {

    }
}
