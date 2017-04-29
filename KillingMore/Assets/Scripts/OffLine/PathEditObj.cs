using UnityEngine;
using System.Collections;

public class PathEditObj : MonoBehaviour
{
    public float Velocity = 1;
    public PathBehavior Behavior = PathBehavior.Move;

    [Tooltip("节点行为持续时间，仅对非move类型有效，且超过到达下一节点时间时会被自动校正")]
    public float BehaviorDuration = 0;

    [Tooltip("仅用于定点攻击时，进行随机移动的半径")]
    public float Disturbance = 0;

    GameObject mGroupParent = null;

    void Start()
    {
        mGroupParent = transform.parent.gameObject;
    }

    void Serialize()
    {

    }
}