using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssetsManager
{
    public const float EffectPoolDuration = 60.0f;
    public const float ProjectilePoolDuration = 60.0f;
    public const float SoundEffectPoolDuration = 60.0f;

    public static AssetsManager Singleton
    {
        get
        {
            return msSingleton;
        }
    }

    public static readonly Vector3 RespawnPos = new Vector3(-100, -100, 0);

    public enum ResType
    {
        Character,
        Emitter,
        Projectile,
        Effect,
        SoundEffect,
        UIObject,
        Music,
        Unknown
    }

    public static string RestType2PathName(ResType rt)
    {
        switch (rt)
        {
            case ResType.Character:
                return "Prefabs/Characters";
            case ResType.Emitter:
                return "Prefabs/Emitters";
            case ResType.Projectile:
                return "Prefabs/Projectiles";
            case ResType.Effect:
                return "Prefabs/Effects";
            case ResType.UIObject:
                return "Prefabs/UI";
            case ResType.SoundEffect:
                return "Prefabs/Audio";
        }

        return string.Empty;
    }

    public static ResType String2ResType(string str)
    {
        switch (str)
        {
            case "Character":
                return ResType.Character;
            case "Effect":
                return ResType.Effect;
            case "Emitter":
                return ResType.Emitter;
            case "Projectile":
                return ResType.Projectile;
            case "SoundEffect":
                return ResType.SoundEffect;
            case "UI":
                return ResType.UIObject;
        }

        return ResType.Unknown;
    }

    public GameObject LoadObjectFromPath(string name, string folder, bool instantiate = true)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(folder);
        sb.Append("/");
        sb.Append(name);
        GameObject go = LoadInResourcesFolder(sb.ToString(), instantiate);
        if (go != null) go.name = name;
        return go;
    }

    public GameObject LoadObjectFromPath(string name, ResType rt, bool instantiate = true)
    {
        return LoadObjectFromPath(name, RestType2PathName(rt), instantiate);
    }

    public bool ExistInPool(string name, ResType rt)
    {
        SimplyGameObjectPool pool = null;
        mPools.TryGetValue(rt, out pool);

        if (pool != null)
        {
            return pool.Exist(name);
        }

        return false;
    }

    public GameObject FindNPopFromPool(string name, ResType rt)
    {
        GameObject g = null;

        SimplyGameObjectPool pool = null;
        mPools.TryGetValue(rt, out pool);

        if (pool != null)
        {
            g = pool.FindNPop(name);
        }

        if (g != null)
        {
            g.SetActive(true);
            g.transform.position = RespawnPos;
        }

        return g;
    }

    /// <summary>
    /// 2015-09-16 
    /// Lin新添加方法
    /// 加载resource下面某个目录所有资源
    /// </summary>
    /// <param name="name"></param>
    /// <param name="folder"></param>
    /// <returns></returns>
    public Object[] LoadObjectsFromPath(string name, string folder)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(folder).Append("/").Append(name);

        return (Object[])Resources.LoadAll(sb.ToString());
    }

    public GameObject LoadGameObjectByResType(string name, string folder, ResType rt, bool instantiate = true, bool forceCreateNewOne = false)
    {
        GameObject g = FindInPool(rt, name, !forceCreateNewOne);
        if (g != null)
        {
            if (forceCreateNewOne)
            {
                GameObject newOne = GameObject.Instantiate(g) as GameObject;
                newOne.name = name;
                newOne.SetActive(true);
                newOne.transform.position = RespawnPos;
                return newOne;
            }

            g.SetActive(true);
            g.transform.position = RespawnPos;
            return g;
        }

        return LoadObjectFromPath(name, folder, instantiate);
    }

    public T LoadObjectFromPathT<T>(string name, string folder) where T : Object
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(folder);
        sb.Append("/");
        sb.Append(name);

        return Resources.Load(sb.ToString()) as T;
    }

    GameObject LoadInResourcesFolder(string path, bool instantiate = true)
    {
        GameObject obj = Resources.Load(path, typeof(GameObject)) as GameObject;

        if (obj != null && instantiate)
        {
            GameObject inst = GameObject.Instantiate(obj) as GameObject;
            return inst;
        }

        return obj;
    }

    void MakePath(System.Text.StringBuilder sb, string name, string folder, ResType crt)
    {
        sb.Append("Prefabs/");
        sb.Append(folder);
        sb.Append("/");
        sb.Append(name);
    }

    public void PushLoadTask(string name, string folder, ResType crt, bool asyncLoad, System.Action<GameObject> onLoaded, string alia = "")
    {
        if (string.IsNullOrEmpty(name) || onLoaded == null)
        {
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        MakePath(sb, name, folder, crt);

        LoadTask task = new LoadTask();
        task.Prefab = name;
        task.ResPath = sb.ToString();
        task.ResTy = crt;
        task.OnLoadedFunc = onLoaded;
        task.Alia = alia;
        task.AsyncLoad = asyncLoad;
        mScheduledTasks.Add(task);
    }

    GameObject FindInPool(ResType rt, string prefabName, bool findNPop = true)
    {
        SimplyGameObjectPool pool = null;
        mPools.TryGetValue(rt, out pool);

        if (pool != null)
        {
            return findNPop ? pool.FindNPop(prefabName) : pool._DirectRef(prefabName);
        }

        return null;
    }

    public void Initialize()
    {
        if (mPools.Count == 0)
        {
            mPools.Add(AssetsManager.ResType.Effect, new SimplyGameObjectPool(EffectPoolDuration));
            mPools.Add(AssetsManager.ResType.SoundEffect, new SimplyGameObjectPool(SoundEffectPoolDuration));
            mPools.Add(AssetsManager.ResType.Projectile, new SimplyGameObjectPool(ProjectilePoolDuration));
        }
    }

    public void DestroyGameObject(GameObject g, ResType rt, string prefabName, float delay = 0,
        float extraLife = 0)
    {
        SimplyGameObjectPool pool = null;
        mPools.TryGetValue(rt, out pool);

        if (pool != null)
        {
            pool.Destroy(g, prefabName, delay, extraLife);
        }
        else
        {
            GameObject.Destroy(g, delay);
        }
    }

    public void UpdateTasks()
    {
        foreach (SimplyGameObjectPool pool in mPools.Values)
        {
            pool.UpdatePool();
        }

        if (mCurrentTask == null
            && mScheduledTasks.Count > 0)
        {
            mCurrentTask = mScheduledTasks[0];

            GameObject g = FindInPool(mCurrentTask.ResTy, mCurrentTask.Prefab);
            if (g != null)
            {
                mCurrentTask.AsyncLoad = false;
                mCurrentTask.NonAsyncAsset = g;
                mCurrentTask.FromPool = true;
                g.SetActive(true);
                g.transform.position = RespawnPos;
            }
            else
            {
                if (mCurrentTask.AsyncLoad)
                {
                    mCurrentTask.RequestStatus = Resources.LoadAsync(mCurrentTask.ResPath);
                }
                else
                {
                    mCurrentTask.NonAsyncAsset = Resources.Load(mCurrentTask.ResPath);
                }
            }

            mScheduledTasks.RemoveAt(0);

            // instantiating in next frame.
            return;
        }

        if (mCurrentTask != null)
        {
            if (mCurrentTask.AsyncLoad && mCurrentTask.RequestStatus.isDone)
            {
                if (mCurrentTask.RequestStatus.asset != null)
                {
                    GameObject inst = GameObject.Instantiate((GameObject)mCurrentTask.RequestStatus.asset) as GameObject;

                    if (!string.IsNullOrEmpty(mCurrentTask.Alia))
                    {
                        inst.name = mCurrentTask.Alia;
                    }

                    mCurrentTask.OnLoadedFunc(inst);
                }
                else
                {
                    Game.LogError("Async load resource " + mCurrentTask.ResPath + " failed.");
                }

                mCurrentTask = null;
            }
            else if (!mCurrentTask.AsyncLoad)
            {
                if (mCurrentTask.NonAsyncAsset != null)
                {
                    GameObject inst = mCurrentTask.FromPool ?
                        (GameObject)mCurrentTask.NonAsyncAsset : GameObject.Instantiate((GameObject)mCurrentTask.NonAsyncAsset) as GameObject;

                    if (!string.IsNullOrEmpty(mCurrentTask.Alia))
                    {
                        inst.name = mCurrentTask.Alia;
                    }

                    mCurrentTask.OnLoadedFunc(inst);
                }

                mCurrentTask = null;
            }
        }
    }

    public void ClearTasks()
    {
        mScheduledTasks.Clear();
        foreach (SimplyGameObjectPool pool in mPools.Values)
        {
            pool.ForceDestroyAll();
        }

        Resources.UnloadUnusedAssets();
    }

    public static string GetDataPath(string relFilePath)
    {
        string platformPath = Application.dataPath + "/StreamingAssets/" + relFilePath;

        RuntimePlatform platform = Application.platform;
        if (platform == RuntimePlatform.IPhonePlayer)
        {
            platformPath = Application.dataPath + "/Raw/" + relFilePath;
            return platformPath;
        }
        else if (platform == RuntimePlatform.Android)
        {
            platformPath = Application.streamingAssetsPath + "/" + relFilePath;
        }
        else if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
        {

        }

        return platformPath;
    }

    public class LoadTask
    {
        public string ResPath;
        public string Prefab;
        public ResType ResTy;
        public System.Action<GameObject> OnLoadedFunc;
        public ResourceRequest RequestStatus;
        public Object NonAsyncAsset;
        public string Alia = "";
        public bool AsyncLoad;
        public bool FromPool;
    }

    List<LoadTask> mScheduledTasks = new List<LoadTask>();
    LoadTask mCurrentTask = null;

    //struct PoolItem
    //{
    //    public GameObject EffectObj;
    //    public float Life;
    //}

    public Dictionary<ResType, SimplyGameObjectPool> mPools = new Dictionary<ResType, SimplyGameObjectPool>();
    static readonly AssetsManager msSingleton = new AssetsManager();
}

public class SimplyGameObjectPool
{
    public struct PoolItem
    {
        public GameObject Obj;
        public string PrefabName;
        public float Life;
        public float ExtraLife;
    }

    public GameObject FindNPop(string prefabName)
    {
        for (int i = 0; i < mItems.Count; ++i)
        {
            if (prefabName == mItems[i].PrefabName
                && mItems[i].Obj != null)
            {
                GameObject g = mItems[i].Obj;
                mItems.RemoveAt(i);
                return g;
            }
        }

        return null;
    }

    public GameObject _DirectRef(string prefabName)
    {
        for (int i = 0; i < mItems.Count; ++i)
        {
            if (prefabName == mItems[i].PrefabName
                && mItems[i].Obj != null)
            {
                GameObject g = mItems[i].Obj;
                return g;
            }
        }

        return null;
    }

    public bool Exist(string prefabName)
    {
        for (int i = 0; i < mItems.Count; ++i)
        {
            if (prefabName == mItems[i].PrefabName
                && mItems[i].Obj != null)
            {
                return true;
            }
        }

        return false;
    }

    public void Destroy(GameObject obj, string prefabName, float delay, float extraLife = 0)
    {
        PoolItem item;
        item.Obj = obj;
        item.PrefabName = prefabName;
        item.Life = mItemLife + extraLife;
        item.ExtraLife = extraLife;

        if (delay > 0)
        {
            item.Life = delay;
            mScheduled.Add(item);
            return;
        }

        DisableObject(obj);
        mItems.Add(item);
    }

    void DisableObject(GameObject g)
    {
        if (g == null)
        {
            return;
        }

        g.transform.parent = null;
        g.SetActive(false);
    }

    public void UpdatePool()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < mItems.Count; )
        {
            PoolItem pl = mItems[i];
            pl.Life -= dt;
            if (pl.Life <= 0)
            {
                if (pl.Obj != null)
                {
                    GameObject.Destroy(pl.Obj);
                }

                mItems.RemoveAt(i);
            }
            else
            {
                mItems[i] = pl;
                ++i;
            }
        }

        // schedule to items.
        for (int i = 0; i < mScheduled.Count; )
        {
            PoolItem pl = mScheduled[i];
            pl.Life -= dt;
            if (pl.Life <= 0)
            {
                mScheduled.RemoveAt(i);
                DisableObject(pl.Obj);
                pl.Life = mItemLife + pl.ExtraLife;
                mItems.Add(pl);
            }
            else
            {
                mScheduled[i] = pl;
                ++i;
            }
        }
    }

    public void ForceDestroyAll()
    {
        for (int i = 0; i < mItems.Count; ++i)
        {
            if (mItems[i].Obj != null)
            {
                GameObject.Destroy(mItems[i].Obj);
            }
        }

        mItems.Clear();

        for (int i = 0; i < mScheduled.Count; ++i)
        {
            if (mScheduled[i].Obj != null)
            {
                GameObject.Destroy(mScheduled[i].Obj);
            }
        }

        mScheduled.Clear();
    }

    public SimplyGameObjectPool(float life)
    {
        mItemLife = life;
    }

    List<PoolItem> mScheduled = new List<PoolItem>();
    List<PoolItem> mItems = new List<PoolItem>();
    float mItemLife = 10;
}
