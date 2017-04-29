using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

public class BattleRegion : MonoBehaviour
{
    public bool IsBoss = false;

    void Start()
    {
        BattleEdit.Singleton.RegisterRegion(this);
    }

    public void SetRegionId(int id)
    {
        mRegionId = id;
    }

    public void Serialize(StreamWriter sw)
    {
        Vector3 groupPos = transform.position;
        foreach (Transform tf in transform)
        {
            Actor ac = tf.GetComponent<Actor>();
            JsonData jd = new JsonData();
            Vector3 offset = tf.localPosition;
            jd["Name"] = tf.gameObject.name;
            jd["ActorType"] = ac.GetActorType().ToString();
            jd["PosX"] = offset.x;
            jd["PosY"] = offset.y;
            jd["PosZ"] = offset.z;
            jd["RotX"] = tf.rotation.eulerAngles.x;
            jd["RotY"] = tf.rotation.eulerAngles.y;
            jd["RotZ"] = tf.rotation.eulerAngles.z;
            string data = jd.ToJson();
            sw.WriteLine(data);
        }
    }

    int mRegionId;
}
