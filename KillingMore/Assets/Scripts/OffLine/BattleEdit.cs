using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BattleEdit : MonoBehaviour
{
    public const string FileName = "Region.r";

    public static BattleEdit Singleton
    {
        get
        {
            return msSingleton;
        }
    }

    void Awake()
    {
        msSingleton = this;
    }

    void Start()
    {

    }

    void Update()
    {
        if (!mSerialized)
        {
            Serialize();
            mSerialized = true;
        }
    }

    public void RegisterRegion(BattleRegion br)
    {
        if (!mRegions.Contains(br))
        {
            mRegions.Add(br);
            br.SetRegionId(mRegionId);
            ++mRegionId;
        }
    }

    public void Serialize()
    {
        // crew
        StreamWriter a_sw = BinaryReadNWrite.OpenFile2WriteAsString(FileName, false);
        for (int i = 0; i < mRegions.Count; ++i)
        {
            BattleRegion br = mRegions[i];
            br.Serialize(a_sw);
        }
        BinaryReadNWrite.EndWirte(a_sw);
        Debug.Log("Region serialize finished");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 60, Screen.height / 2 - 10, 100, 20),
            "Exporting scene...");
    }
    static BattleEdit msSingleton;
    List<BattleRegion> mRegions = new List<BattleRegion>();
    bool mSerialized = false;
    int mRegionId = 1;
}
