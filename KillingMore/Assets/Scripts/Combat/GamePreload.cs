using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using Emitter;

public class GamePreload
{
    // 某些预加载物不在preload-items对象中存储，直接存在asset manager的pool中，延长其寿命以便游戏过程中索引
    public const float MaxPreloadStuffLifeInPool = 600f;
    public const int NumTaskPerBatch = 16;
    public const float Time4BulltesPreloadCalc = 2f;          // 计算多少时间内发射器发射个数
    public const int MaxPreloadBulltesPerEmmiter = 100;

    public enum eStage
    {
        None = 0,
        ObjsInList,
        Emmiters,
        Bullets,

        Ended,
    }

    public void Reset()
    {
        mTasks.Clear();
        mPreloadedItems.Clear();
        mLoadedEmmiters.Clear();
        mCurStage = eStage.None;
        mCurTaskIndex = 0;

#if UNITY_EDITOR
        mNumExecutedTasks = 0;
#endif
    }

    public void AddLoadTask(string prefabName, AssetsManager.ResType rt, System.Action<_Task, GameObject> onLoaded, bool storeInPool = false,
        string customFolder = "")
    {
        _Task tsk = new _Task();
        tsk.PrefabName = prefabName;
        tsk.CustomFolder = customFolder;
        tsk.ResTy = rt;
        tsk.OnLoaded = onLoaded;
        tsk.StoreInPool = storeInPool;
        mTasks.Add(tsk);

#if UNITY_EDITOR
        ++mNumExecutedTasks;
#endif
    }

    public GameObject FetchPreloadedItem(string prefabName, AssetsManager.ResType rt, bool loadIfNotExist)
    {
        _Item it = FetchItem(prefabName, rt);
        if (it != null)
        {
            return it.GameObj;
        }
        else if (loadIfNotExist)
        {
            return AssetsManager.Singleton.LoadGameObjectByResType(prefabName, AssetsManager.RestType2PathName(rt), rt);
        }

        return null;
    }

    public void ExecuteTasks()
    {
        mCurStage = eStage.ObjsInList;
    }

    public void ExecuteTasksBatch()
    {
        // 分批次异步加载
        AssetsManager am = AssetsManager.Singleton;
        for (int i = mCurTaskIndex; i < mCurTaskIndex + NumTaskPerBatch && i < mTasks.Count; ++i)
        {
            _Task tsk = mTasks[i];
            ++mCurTaskIndex;

            GameObject g = null;
            _Item aleadyExsit = FetchItem(tsk.PrefabName, tsk.ResTy, false);
            if (aleadyExsit != null)
            {
                g = GameObject.Instantiate(aleadyExsit.GameObj) as GameObject;
                g.name = tsk.PrefabName;
            }
            else
            {
                //对象在这里实例化 ，所以只能在这里得到boss对象保存下来
                string folder = string.IsNullOrEmpty(tsk.CustomFolder) ? AssetsManager.RestType2PathName(tsk.ResTy) : tsk.CustomFolder;
                g = am.LoadGameObjectByResType(tsk.PrefabName, folder, tsk.ResTy,
                    true, true);
            }

            if (g != null)
            {
                _Item it = new _Item();
                it.GameObj = g;
                it.PrefabName = tsk.PrefabName;
                it.ResTy = tsk.ResTy;

                if (tsk.OnLoaded != null)
                {
                    tsk.OnLoaded(tsk, g);
                }

                if (tsk.StoreInPool)
                {
                    am.DestroyGameObject(g, tsk.ResTy, tsk.PrefabName, 0, MaxPreloadStuffLifeInPool);
                    continue;
                }

                g.SetActive(false);

                List<_Item> lst = null;
                if (mPreloadedItems.TryGetValue(tsk.ResTy, out lst))
                {
                    lst.Add(it);
                }
                else
                {
                    lst = new List<_Item>();
                    lst.Add(it);
                    mPreloadedItems.Add(tsk.ResTy, lst);
                }
            }
            else
            {
                Game.ErrorString = "Failed to load " + tsk.PrefabName;
                Game.LogError(Game.ErrorString);
            }
        }

        if (mTasks.Count > 0 && mCurTaskIndex >= mTasks.Count)
        {
            mTasks.Clear();
            mCurTaskIndex = 0;

#if UNITY_EDITOR
            Game.Log("Current stage " + mCurStage + " finished " + mNumExecutedTasks + " tasks total.");
#endif

            if (mCurStage == eStage.ObjsInList)
            {
                mLoadedEmmiters.Clear();
                mCurStage = mTasks.Count > 0 ? eStage.Emmiters : eStage.Ended;
            }
            else if (mCurStage == eStage.Emmiters)
            {
                ParseNAddBulletsTasks();
                mCurStage = mTasks.Count > 0 ? eStage.Bullets : eStage.Ended;
                mLoadedEmmiters.Clear();
            }
            else if (mCurStage == eStage.Bullets || mCurStage == eStage.None)
            {
                mCurStage = eStage.Ended;
            }

#if UNITY_EDITOR
            if (mCurStage == eStage.Ended)
            {
                Game.Log("All pre-load tasks accomplished!");
            }
#endif
        }
    }

    void OnLoadedEmitter(_Task tsk, GameObject g)
    {
        Emitter_Basic em = g.GetComponent<Emitter_Basic>();
        if (em != null)
        {
            mLoadedEmmiters.Add(em);
        }
    }

    int DistillNumberInString(string s)
    {
        int index = 0;

        while (index < s.Length)
        {
            if (s[index] >= '0' && s[index] <= '9')
            {
                break;
            }

            ++index;
        }

        if (index > s.Length - 1)
        {
            return 0;
        }

        string sub = s.Substring(index);
        return System.Convert.ToInt32(sub);
    }

    void ParseNAddBulletsTasks()
    {
        for (int i = 0; i < mLoadedEmmiters.Count; ++i)
        {
            Emitter_Basic em = mLoadedEmmiters[i];
            RecuAddBulletTasks(em);
        }
    }

    void RecuAddBulletTasks(Emitter_Basic em)
    {
        if (em.ProjectilePrefab != null && em.GetEmitterType() == EmitterType.Bullet)
        {
            // 计算子弹个数
            Emitter_Bullet bm = em as Emitter_Bullet;
            float bps = bm.CalcBPS();
            int num = Mathf.FloorToInt(bps * Time4BulltesPreloadCalc);
            num = Mathf.Max(1, num);
            num = Mathf.Min(MaxPreloadBulltesPerEmmiter, num);

            for (int i = 0; i < num; ++i)
            {
                AddLoadTask(em.ProjectilePrefab.name, AssetsManager.ResType.Emitter, null, true, "Prefabs/Projectiles/Enemy");
            }
        }

        foreach (Transform tf in em.transform)
        {
            Emitter_Basic subEm = tf.GetComponent<Emitter_Basic>();
            if (subEm != null)
            {
                RecuAddBulletTasks(subEm);
            }
        }
    }

    _Item FetchItem(string prefabName, AssetsManager.ResType rt, bool popItem = true)
    {
        List<_Item> lst = null;
        //根据AssetsManager.ResType 获取对应的对象池
        if (mPreloadedItems.TryGetValue(rt, out lst))
        {
            for (int i = 0; i < lst.Count; ++i)
            {
                if (lst[i].PrefabName == prefabName)
                {
                    _Item it = lst[i];

                    if (popItem)
                    {
                        lst.RemoveAt(i);
                    }

                    return it;
                }
            }
        }
        else
        {
            //    KXDebug.Log("FetchItem: 对象池没有发现对象 " + prefabName + " 类型 " + rt);
        }

        return null;
    }

    public void UpdateTasks()
    {
        // TODO
    }

    public bool AllTaskFinished
    {
        get
        {
            return mCurStage == eStage.Ended;
        }
    }

    public class _Task
    {
        public string PrefabName;
        public string CustomFolder;                                 // 如果这个有值，则不使用资源类型自动生成完整路径
        public AssetsManager.ResType ResTy;
        public System.Action<_Task, GameObject> OnLoaded;
        public bool StoreInPool;
    }

    class _Item
    {
        public string PrefabName;
        public AssetsManager.ResType ResTy;
        public GameObject GameObj;
    }

    List<_Task> mTasks = new List<_Task>();
    List<Emitter_Basic> mLoadedEmmiters = new List<Emitter_Basic>();
    int mCurTaskIndex = 0;

#if UNITY_EDITOR
    int mNumExecutedTasks = 0;
#endif

    eStage mCurStage = eStage.None;
    Dictionary<AssetsManager.ResType, List<_Item>> mPreloadedItems = new Dictionary<AssetsManager.ResType, List<_Item>>();
}
