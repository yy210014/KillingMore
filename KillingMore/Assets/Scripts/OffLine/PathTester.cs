using UnityEngine;
using System.Collections;

public class PathTester : MonoBehaviour
{
    public PathEditGroup Path;
    NPPath mNPPath = null;
    ActorPath.QuatT mLastQuatT;

    void Start()
    {
        SplineBuild.Singleton.RegisterTester(this);
    }

    public void Prepare()
    {
        if (Path != null)
        {
            mNPPath = Path.BuildNPPath();
        }

        mLastQuatT.Rot = transform.rotation;
        mLastQuatT.T = transform.position;
    }
}