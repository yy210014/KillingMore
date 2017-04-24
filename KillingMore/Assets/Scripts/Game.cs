using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

public class Game : MonoBehaviour
{
    public float StandardAspectW = 1280;
    public float StandardAspectH = 720;
    public float GameAreaW = 640;
    public float GameAreaH = 960;

    [Tooltip("出生点在屏幕上的归一化坐标, top_left")]
    public Vector2 SpawnPoint_TopLeft = new Vector2(0.25f, 0f);
    [Tooltip("出生点在屏幕上的归一化坐标, top_mid")]
    public Vector2 SpawnPoint_TopMid = new Vector2(0.5f, 0f);
    [Tooltip("出生点在屏幕上的归一化坐标, top_right")]
    public Vector2 SpawnPoint_TopRight = new Vector2(0.75f, 0f);
    [Tooltip("出生点在屏幕上的归一化坐标, left_high")]
    public Vector2 SpawnPoint_LeftHigh = new Vector2(0f, 0.25f);
    [Tooltip("出生点在屏幕上的归一化坐标, left_mid")]
    public Vector2 SpawnPoint_LeftMid = new Vector2(0f, 0.5f);
    [Tooltip("出生点在屏幕上的归一化坐标, left_low")]
    public Vector2 SpawnPoint_LeftLow = new Vector2(0f, 0.75f);
    [Tooltip("出生点在屏幕上的归一化坐标, right_high")]
    public Vector2 SpawnPoint_RightHigh = new Vector2(1.0f, 0.25f);
    [Tooltip("出生点在屏幕上的归一化坐标, right_mid")]
    public Vector2 SpawnPoint_RightMid = new Vector2(1.0f, 0.5f);
    [Tooltip("出生点在屏幕上的归一化坐标, right_low")]
    public Vector2 SpawnPoint_RightLow = new Vector2(1.0f, 0.75f);

    public bool DebugStringOnScreen = true;
    public const int MaxDebugStringOnScreen = 10;

#if UNITY_EDITOR

    public float SpecifyValidRcW = 100.0f;
    public float SpecifyValidRcH = 100.0f;

#endif

    public static Game Singleton
    {
        get
        {
            return msSingleton;
        }
    }

    public static string OfflineTestLevel
    {
        get;
        set;
    }

    public static AsyncOperation Asy
    {
        get;
        set;
    }

    public static Vector2 SceneZero
    {
        get
        {
            Vector3 zero = Camera.main.ScreenToWorldPoint(Vector3.zero);
            return new Vector2(zero.x, zero.z); ;
        }
    }

    public static float SceneHeight
    {
        get
        {
            Vector3 height = Camera.main.ScreenToWorldPoint(Vector3.up * Screen.height);
            return height.z;
        }
    }

    public static float SceneWidth
    {
        get
        {
            Vector3 width = Camera.main.ScreenToWorldPoint(Vector3.right * Screen.width);
            return width.x;
        }
    }

    public static float SceneMagnitude
    {
        get
        {
            return (Mathf.Abs(Game.SceneHeight) + Mathf.Abs(Game.SceneZero.y));
        }
    }

    public static Vector3 Up = new Vector3(0, 1, 0);
    public static string DebugString
    {
        get
        {
            return msDebugStrings.Count > 0 ? msDebugStrings[msDebugStrings.Count - 1] : "";
        }

        set
        {
            if (msDebugStrings.Count < MaxDebugStringOnScreen)
            {
                msDebugStrings.Add(value);
            }
            else
            {
                msDebugStrings.Insert(0, value);
                msDebugStrings.RemoveAt(msDebugStrings.Count - 1);
            }
        }
    }

    public static string ErrorString
    {
        get
        {
            return msErrorStrings.Count > 0 ? msErrorStrings[msErrorStrings.Count - 1] : "";
        }

        set
        {
            if (msErrorStrings.Count < MaxDebugStringOnScreen)
            {
                msErrorStrings.Add(value);
            }
            else
            {
                msErrorStrings.Insert(0, value);
                msErrorStrings.RemoveAt(msErrorStrings.Count - 1);
            }
        }
    }

    static List<string> msDebugStrings = new List<string>();
    static List<string> msErrorStrings = new List<string>();

    public static void IteraterAllDebugStrings(System.Action<string> ac)
    {
        for (int i = 0; i < msDebugStrings.Count; ++i)
        {
            ac(msDebugStrings[i]);
        }
    }

    public static void IteraterAllErrorStrings(System.Action<string> ac)
    {
        for (int i = 0; i < msErrorStrings.Count; ++i)
        {
            ac(msErrorStrings[i]);
        }
    }

    public float RealtimeDelta
    {
        get
        {
            return mRealtimeDelta;
        }
    }

    public GameState CurrentState
    {
        get
        {
            return mCurrentState;
        }
    }

    void Awake()
    {
        if (msSingleton != null)
        {
            return;
        }
        msSingleton = this;
        Application.targetFrameRate = 60;
        GameObject.DontDestroyOnLoad(this);
    }

    void Start()
    {
        mRealtimeStamp = Time.realtimeSinceStartup;
        AssetsManager.Singleton.Initialize();
        PathManager.Singleton.Initialize();
    }

    public void PauseAudioListener(bool pause)
    {
        AudioListener.pause = pause;
    }

    public bool ChangeAudio()
    {
        AudioListener.volume = AudioListener.volume == 0 ? 0.5f : 0;
        return AudioListener.volume == 0 ? true : false;
    }

    float mLoopDT, mWaitLoopTime = 2;
    public void SetBGM(string name)
    {
        if (GetComponent<AudioSource>() != null && GetComponent<AudioSource>().clip != null && GetComponent<AudioSource>().clip.name == name)
        {
            return;
        }
        if (GetComponent<AudioSource>() == null)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = 0.5f;
            audioSource.playOnAwake = false;
        }
        mLoopDT = 0;

        GameObject go = AssetsManager.Singleton.LoadGameObjectByResType(name, AssetsManager.RestType2PathName(AssetsManager.ResType.Music), AssetsManager.ResType.Music);
        if (go != null)
        {
            GetComponent<AudioSource>().clip = go.GetComponent<AudioSource>().clip;
        }
        else
        {
            AudioClip clip = (AudioClip)Resources.Load("Audio/" + name, typeof(AudioClip));//调用Resources方法加载AudioClip资源
            GetComponent<AudioSource>().clip = clip;
        }
        GetComponent<AudioSource>().Play();
    }

    public void PlayMainBGM()
    {
        SetBGM("BGM_Theme");
    }

    public void OnGameStateStart(GameState gs)
    {
        mStateQueued = gs;
        GameState.States state = gs.GetStateEnum();
        if (state == GameState.States.Main)
        {
            SetBGM("BGM_Theme");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //2015 12 30 小戴添加
        ThreadUpdate();
        if (this != msSingleton)
        {
            return;
        }
        if (mStateQueued != null)
        {
            mCurrentState = mStateQueued;
            mCurrentState.PostInitialize();
            mStateQueued = null;
            return;
        }

        float curT = Time.realtimeSinceStartup;
        mRealtimeDelta = curT - mRealtimeStamp;
        mRealtimeStamp = curT;

        Timer.Singleton.Update();
        AssetsManager.Singleton.UpdateTasks();

        if (mCurrentState != null)
        {
            if (!mCurrentState.OnGameUpdate())
            {
                mCurrentState.Uninitialize();
                mCurrentState = null;
            }

            SoundEffectManager.Singleton.OnGameUpdate();
        }

        if (GetComponent<AudioSource>() != null && GetComponent<AudioSource>().clip != null)
        {
            mLoopDT += Time.deltaTime;
            if (mLoopDT >= GetComponent<AudioSource>().clip.length + mWaitLoopTime)
            {
                mLoopDT = 0;
                GetComponent<AudioSource>().Play();
            }
        }
    }

    public bool IsInCurrentState(GameState.States st)
    {
        return mCurrentState != null && st == mCurrentState.GetStateEnum();
    }

    public void RegisterEventHandler(int eventType, GameEventDel del)
    {
        if (mCurrentState != null)
        {
            mCurrentState.EventProc.RegisterHandler(eventType, del);
        }
    }

    public void UnregisterEventHandler(int eventType, GameEventDel del)
    {
        if (mCurrentState != null)
        {
            mCurrentState.EventProc.UnregisterHandler(eventType, del);
        }
    }

    public void SendEvent(GameEvent ge, object sender, float delay)
    {
        if (mCurrentState != null)
        {
            mCurrentState.EventProc.SendEvent(ge, sender, delay);
        }
    }

    public void SwitchScene(string levelName)
    {
        if (mCurrentState != null)
        {
            SwitchSceneEvent evt = new SwitchSceneEvent();
            mCurrentState.EventProc.SendEvent(evt, this, 0);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
        AssetsManager.Singleton.ClearTasks();

    }

    public void SwitchSceneAsync(string levelName, bool gcCollect = false, System.Action<string> cb = null)
    {
        if (mCurrentState != null)
        {
            SwitchSceneEvent evt = new SwitchSceneEvent();
            mCurrentState.EventProc.SendEvent(evt, this, 0);
        }

        StartCoroutine(SyncLoadLevel(levelName, gcCollect, cb));
    }

    public void SwitchSceneAsyncWaitSignal(string levelName, System.Action<string> startSignal,
        System.Predicate<string> waitSignal, bool gcCollect = false)
    {
        if (mCurrentState != null)
        {
            SwitchSceneEvent evt = new SwitchSceneEvent();
            mCurrentState.EventProc.SendEvent(evt, this, 0);
        }

        StartCoroutine(SyncLoadLevel(levelName, gcCollect, startSignal, waitSignal));
    }

    const float MinLoadingTime = 2.8f;
    float mLoadingCountdown = 0;

    public IEnumerator SyncLoadLevel(string levelName, bool gcCollect, System.Action<string> cb = null)
    {
        GameObject loadPanel = AssetsManager.Singleton.LoadGameObjectByResType("LoadingRoot",
            AssetsManager.RestType2PathName(AssetsManager.ResType.UIObject),
            AssetsManager.ResType.UIObject);
        Vector3 pos = Camera.main.transform.position;
        pos.Set(0, 1000, 0);
        Camera.main.transform.position = pos;

        yield return new WaitForSeconds(0.2f);

        AssetsManager.Singleton.ClearTasks();

        Asy = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(levelName);
        yield return Asy;

        AssetsManager.Singleton.ClearTasks();

        if (gcCollect)
        {
            System.GC.Collect();
        }

        yield return new WaitForSeconds(0.2f);

        GameObject.DestroyImmediate(loadPanel);
        Asy = null;

        if (cb != null)
        {
            cb("Scene Loaded");
        }
    }

    public IEnumerator SyncLoadLevel(string levelName, bool gcCollect, System.Action<string> startSignal,
        System.Predicate<string> waitSignal)
    {
        startSignal("signal");
        /*    GameObject loadPanel = AssetsManager.Singleton.LoadGameObjectByResType("LoadingRoot",
                AssetsManager.RestType2PathName(AssetsManager.ResType.UIObject),
                AssetsManager.ResType.UIObject);
            GameObject.DontDestroyOnLoad(loadPanel);*/

        //Camera.main.enabled = false;
        if (Camera.main != null)
        {
            Vector3 pos = Camera.main.transform.position;
            pos.Set(0, 1000, 0);
            Camera.main.transform.position = pos;
        }
        yield return new WaitForSeconds(0.2f);

        AssetsManager.Singleton.ClearTasks();

        Asy = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(levelName);
        yield return Asy;

        AssetsManager.Singleton.ClearTasks();

        if (gcCollect)
        {
            System.GC.Collect();
        }

        while (!waitSignal("signal"))
        {
            yield return new WaitForSeconds(Time.deltaTime);
        }

        //      GameObject.DestroyImmediate(loadPanel);
        Asy = null;
    }

    float mRealtimeStamp;
    float mRealtimeDelta;
    GameState mStateQueued;
    GameState mCurrentState;

    static Game msSingleton = null;
    private bool isCountDown = false;
    public int StaminaTime = 0;
    public int ExtremeTime = 30 * 60;
    ///===================================================================================
    public struct DelayedQueueItem
    {
        public float time;
        public System.Action action;
    }

    public int MaxThreads = 8;
    public int NumThreads;
    private List<Action> mActions = new List<Action>();
    private List<DelayedQueueItem> mDelayed = new List<DelayedQueueItem>();
    private List<DelayedQueueItem> mCurrentDelayed = new List<DelayedQueueItem>();
    private List<Action> mCurrentActions = new List<Action>();


    public void QueueOnMainThread(Action action)
    {
        QueueOnMainThread(action, 0);
    }

    public void QueueOnMainThread(Action action, float time)
    {
        if (time != 0)
        {
            lock (mDelayed)
            {
                mDelayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
            }
        }
        else
        {
            lock (mActions)
            {
                mActions.Add(action);
            }
        }
    }

    public Thread RunAsync(Action a)
    {
        while (NumThreads >= MaxThreads)
        {
            Thread.Sleep(1);
        }
        Interlocked.Increment(ref NumThreads);
        ThreadPool.QueueUserWorkItem(RunAction, a);
        return null;
    }

    private void RunAction(object action)
    {
        try
        {
            ((Action)action)();
        }
        catch (Exception e)
        {
            Game.LogError("线程池RunAction异常 : " + e.ToString());
        }
        finally
        {
            Interlocked.Decrement(ref NumThreads);
        }

    }

    private void ThreadUpdate()
    {
        lock (mActions)
        {
            mCurrentActions.Clear();
            mCurrentActions.AddRange(mActions);
            mActions.Clear();
        }
        foreach (var a in mCurrentActions)
        {
            a();
        }
        lock (mDelayed)
        {
            mCurrentDelayed.Clear();
            mCurrentDelayed.AddRange(mDelayed.Where(d => d.time <= Time.time));
            foreach (var item in mCurrentDelayed)
                mDelayed.Remove(item);
        }
        foreach (var delayed in mCurrentDelayed)
        {
            delayed.action();
        }

    }

    #region Log
    static public void Log(params object[] objs)
    {
#if UNITY_EDITOR
        string text = getString(objs);
        Debug.Log(text);
#endif
    }

    static public void LogWarning(params object[] objs)
    {
#if UNITY_EDITOR
        string text = getString(objs);
        Debug.LogWarning(text);
#endif
    }

    static public void LogError(params object[] objs)
    {
        string text = getString(objs);

        Debug.LogError(text);
    }

    static string getString(params object[] objs)
    {
        string text = "";

        for (int i = 0; i < objs.Length; ++i)
        {
            if (i == 0)
            {
                text += objs[i].ToString();
            }
            else
            {
                text += ", " + objs[i].ToString();
            }
        }

        return text;
    }
    #endregion
}
