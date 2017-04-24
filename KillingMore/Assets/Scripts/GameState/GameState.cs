using UnityEngine;
using System.Collections;

public abstract class GameState : MonoBehaviour {

    public enum States
    {
        None = 0,
        Main,
        Battle,

        //测试场景
        BulletTester
    }

    public GameEventProc EventProc
    {
        get
        {
            return mEventProc;
        }
    }

    public abstract States GetStateEnum();

    public abstract void Initialize();

    public abstract void PostInitialize();

	// Use this for initialization
	public void Start () {
        Debug.Log("-->Start GameState");
        Game.Singleton.OnGameStateStart(this);
        Initialize();

	}
	
    //// Update is called once per frame
    //public virtual void Update () {
	
    //}

    public bool OnGameUpdate()
    {
        mEventProc.DispatchAll();

        return DoGameUpdate();
    }

    public abstract bool DoGameUpdate();

    public virtual void OnDestroy()
    {
        mEventProc.Reset();

        PostUninitialize();
    }

    public abstract void Uninitialize();

    public abstract void PostUninitialize();

    protected GameEventProc mEventProc = new GameEventProc();
}
