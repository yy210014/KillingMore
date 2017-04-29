using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Emitter;
using LitJson;

public class GameScene
{
    public enum SpawnPoint
    {
        TopLeft = 0,
        TopCenter,
        TopRight,
        LeftHigh,
        LeftMid,
        LeftLow,
        RightHigh,
        RightMid,
        RightLow,

        NumSpawnPoints
    }
    public enum SpawnCond
    {
        Time = 0,
        Wave,
        TimeOrWave,
        TimeAndWave
    }
    public static GameScene Singleton { get { return msSingleton; } }
    public float GameTimeScale { get { return mGameTimeScale; } }
    public float TimeDelta { get { return Time.deltaTime * mGameTimeScale; } }
    public float Elapsed { get { return mElapsed; } }
    public System.Predicate<Projectile> ProjClipFunc { get { return mProjClipFunc; } }
    public GamePreload PreloadStuff { get { return mPreloadStuff; } }

    public void OnGameInitialize()
    {
        mElapsed = 0;
        mGameTimeScale = 1.0f;
        mProjClipFunc = ClipProjectile;
        CalcViewExtPoints();
        CalcCenterPos();
        mSpawnPts = new Vector3[(int)SpawnPoint.NumSpawnPoints];
        for (int i = 0; i < (int)SpawnPoint.NumSpawnPoints; ++i)
        {
            mSpawnPts[i] = CalcSpawnPoint((SpawnPoint)i, Vector3.zero, Vector3.zero);
        }

        mSpawnerRots = new Quaternion[(int)SpawnPoint.NumSpawnPoints];
        for (int i = 0; i < (int)SpawnPoint.NumSpawnPoints; ++i)
        {
            Vector3 dir = (Vector3.zero - mSpawnPts[i]).normalized;
            mSpawnerRots[i] = Quaternion.LookRotation(dir, Vector3.up);
        }
        ReconstructStage();
        mPreloadStuff.ExecuteTasks();
    }

    public void OnGameStart()
    {
        if (Game.Singleton != null
            && Game.Singleton.CurrentState != null)
        {
            mElapsed = 0;
            mGameTimeScale = 1.0f;
            Game.Singleton.RegisterEventHandler(GameEvent.ActorCollideWithProjectile, OnActorCollideWithProj);
            Game.Singleton.RegisterEventHandler(GameEvent.ActorCollideWithLaser, OnActorCollideWithLaser);
            Game.Singleton.RegisterEventHandler(GameEvent.ActorDie, OnActorDieEvent);
            CreateCharacter(1000);
        }
        SpawnRegion();
        mGameUpdateProc = InGameUpdate;
    }

    public void ReconstructStage()
    {
        string fileName = BattleEdit.FileName;
        string context = BinaryReadNWrite.ReadAsString(fileName);
        if (context != null)
        {
            string[] strs = context.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strs.Length; ++i)
            {
                JsonData jd = JsonMapper.ToObject(strs[i]);
                ActorSpawnSchedule ass = new ActorSpawnSchedule();
                ass.mPrefabName = (string)jd["Name"];
                ass.mActorType = (Actor.ActorType)System.Enum.Parse(typeof(Actor.ActorType), (string)jd["ActorType"]);
                float posx = (float)((double)jd["PosX"]);
                float posy = (float)((double)jd["PosY"]);
                float posz = (float)((double)jd["PosZ"]);
                Vector3 relPos = Vector3.zero;
                relPos.Set(posx, posy, posz);
                ass.mRelPos = relPos;
                float rotX = (float)((double)jd["RotX"]);
                float rotY = (float)((double)jd["RotY"]);
                float rotZ = (float)((double)jd["RotZ"]);
                ass.mRot = Quaternion.Euler(rotX, rotY, rotZ);
                mRegionSchedule.Add(ass);
            }
        }
    }

    void SpawnRegion()
    {
        for (int i = 0; i < mRegionSchedule.Count; ++i)
        {
            ActorSpawnSchedule ass = mRegionSchedule[i];
            GameObject g = mPreloadStuff.FetchPreloadedItem(ass.mPrefabName, AssetsManager.String2ResType(ass.mActorType.ToString()), true);
            if (g != null)
            {
                g.SetActive(true);
                g.transform.position = ass.mRelPos;
                g.transform.rotation = ass.mRot;//ass.mRot * groupQt.Rot;
            }
        }
    }

    void CreateCharacter(int id)
    {
        string name = ResourceNameUtil.BuildResourceName(id, ResourceCatelog.Character, ResourceDetail.Prefab);
        GameObject go = AssetsManager.Singleton.LoadGameObjectByResType(name, AssetsManager.RestType2PathName(AssetsManager.ResType.Character),
                    AssetsManager.ResType.Character);
        if (go == null)
        {
            Game.LogError("创建玩家: " + name + "失败！");
            return;
        }
        Character character = go.GetComponent<Character>();
        if (character != null)
        {
            character.transform.position = Vector3.zero;
            character.OnGameInitialize();
            character.SetFaction(Actor.PlayerTeamFactionId);
            character.Hp.Value = 100;
            character.SwitchBehaviorFSM(CharacterBehaviors.MOVE_ATTACK);
            ActorManage.Singleton.OnPlayerActorSpawn(character);

            //添加发射器
            GameObject Emitter_go = AssetsManager.Singleton.LoadGameObjectByResType(
                           "LinearEmitter", AssetsManager.RestType2PathName(AssetsManager.ResType.Emitter),
                           AssetsManager.ResType.Emitter);
            if (Emitter_go != null)
            {
                Emitter_Basic emitter = Emitter_go.GetComponent<Emitter_Basic>();
                Emitter_go.SetActive(false);
                if (emitter != null)
                {
                    emitter.SetOwner(character);
                    emitter.Initialize();
                    AttackStateSchedule ass = new AttackStateSchedule();
                    ass.Emitter = emitter;
                    character.InitialAddEmitter(emitter, Vector3.zero, Quaternion.identity);
                    character.AddAttackStateSchedule(ass);
                //    character.SwitchBulletEmitte(0);
                }
            }
        }
        else
        {
            Game.LogError("创建玩家: " + name + "失败！");
        }

    }

    public void OnGameUpdate()
    {
        float dt = TimeDelta;
        mElapsed += dt;

        if (mGameUpdateProc != null)
        {
            mGameUpdateProc(dt);
        }
    }

    public void OnGameEnd()
    {

    }

    void InGameUpdate(float dt)
    {
        ActorManage.Singleton.OnGameUpdate(dt);
    }

    public void OnGameUninitialize()
    {
        ActorManage.Singleton.Clear();
        mPreloadStuff.Reset();
    }

    void OnActorCollideWithProj(GameEvent ge)
    {
        // TODO
        ActorCollideWithProj acj = ge as ActorCollideWithProj;
        if (acj.mActor == null || acj.mActor.IsDead || acj.mProj.IsDead)
        {
            return;
        }
    }

    void OnActorCollideWithLaser(GameEvent ge)
    {
        ActorCollideWithLaser acl = ge as ActorCollideWithLaser;
        if (acl.mActor == null || acl.mActor.IsDead || acl.mProj.IsDead)
        {
            return;
        }
    }

    void OnActorDieEvent(GameEvent ge)
    {
        ActorDieEvent ade = ge as ActorDieEvent;
        if (ade == null)
        {
            Game.LogError("ActorDieEvent为null!");
            return;
        }
    }

    public void ResetSignal(string s)
    {
        mGameInitialized = false;
    }

    // 在异步加载场景时会被频繁调用
    public bool SignalCheck(string s)
    {
        if (!mPreloadStuff.AllTaskFinished)
        {
            mPreloadStuff.ExecuteTasksBatch();
            return false;
        }
        Game.Log("预加载结束!!");
        mGameInitialized = true;
        return mGameInitialized;
    }

    public void SetGameTimeScale(float s)
    {
        mGameTimeScale = s;
    }

    public void SetProjectileCliper(System.Predicate<Projectile> func)
    {
        if (func != null)
        {
            mProjClipFunc = func;
        }
        else
        {
            mProjClipFunc = ClipProjectile;
        }
    }
    Vector3 CalcCenterPos()
    {
        Vector3 smidPos = Vector3.zero;
        smidPos.x = (float)Screen.width * 0.5f;
        smidPos.y = (float)Screen.height * 0.5f;
        Ray r0 = Camera.main.ScreenPointToRay(smidPos);
        float t0 = -1.0f * Vector3.Dot(Vector3.up, r0.origin) / Vector3.Dot(Vector3.up, r0.direction);
        return r0.origin + r0.direction * t0;
    }

    Vector3 CalcSpawnPoint(SpawnPoint sp, Vector3 centerPos, Vector3 topHeight)
    {
        Vector3 pos = Vector3.zero;
        Vector3 spos = Vector3.zero;
        switch (sp)
        {
            case SpawnPoint.TopLeft:
                {
                    spos = Game.Singleton.SpawnPoint_TopLeft;
                }
                break;
            case SpawnPoint.TopCenter:
                {
                    spos = Game.Singleton.SpawnPoint_TopMid;
                }
                break;
            case SpawnPoint.TopRight:
                {
                    spos = Game.Singleton.SpawnPoint_TopRight;
                }
                break;
            case SpawnPoint.LeftHigh:
                {
                    spos = Game.Singleton.SpawnPoint_LeftHigh;
                }
                break;
            case SpawnPoint.LeftMid:
                {
                    spos = Game.Singleton.SpawnPoint_LeftMid;
                }
                break;
            case SpawnPoint.LeftLow:
                {
                    spos = Game.Singleton.SpawnPoint_LeftLow;
                }
                break;
            case SpawnPoint.RightHigh:
                {
                    spos = Game.Singleton.SpawnPoint_RightHigh;
                }
                break;
            case SpawnPoint.RightMid:
                {
                    spos = Game.Singleton.SpawnPoint_RightMid;
                }
                break;
            case SpawnPoint.RightLow:
                {
                    spos = Game.Singleton.SpawnPoint_RightLow;
                }
                break;
        }
        spos += Vector3.up * topHeight.z;
        spos.x *= Screen.width;
        spos.y = 1.0f - spos.y;
        spos.y *= Screen.height;
        Ray r = Camera.main.ScreenPointToRay(spos);
        float t = -1.0f * Vector3.Dot(Vector3.up, r.origin) / Vector3.Dot(Vector3.up, r.direction);
        pos = r.origin + r.direction * t;
        return pos;
    }

    Vector3 CalcViewExtPoints()
    {
        Vector3 spos = Vector3.zero;
        Ray r = Camera.main.ScreenPointToRay(spos);
        float t = -1.0f * Vector3.Dot(Vector3.up, r.origin) / Vector3.Dot(Vector3.up, r.direction);
        Vector3 pos = r.origin + r.direction * t;
        Vector4 output = Vector4.zero;
        output.x = pos.x;
        output.y = pos.z;
        spos.x = (float)Screen.width;
        spos.y = (float)Screen.height;
        r = Camera.main.ScreenPointToRay(spos);
        t = -1.0f * Vector3.Dot(Vector3.up, r.origin) / Vector3.Dot(Vector3.up, r.direction);
        pos = r.origin + r.direction * t;

        output.z = pos.x;
        output.w = pos.z;
        Vector3 zero = Vector3.zero;
        Vector3 width = Vector3.right * Screen.width;
        Vector3 height = Camera.main.ScreenToWorldPoint(Vector3.up * Screen.height);
        zero = Camera.main.ScreenToWorldPoint(zero);
        width = Camera.main.ScreenToWorldPoint(width);

        //剔除子弹用的边界
        mMinViewX = Mathf.Min(zero.x, width.x) - 2;
        mMaxViewX = Mathf.Max(zero.x, width.x) + 2;
        mMinViewZ = Mathf.Min(output.y, output.w) - 3.0f;
        mMaxViewZ = Mathf.Max(output.y, output.w) + 2.0f;
        return height / (Mathf.Abs(height.z) + Mathf.Abs(zero.z));
    }

    bool ClipProjectile(Projectile p)
    {
        Vector3 pos = p.transform.position;
        return pos.x < mMinViewX || pos.x > mMaxViewX || pos.z < mMinViewZ || pos.z > mMaxViewZ;
    }

    public bool invisible(Vector3 pos)
    {
        return pos.x < mMinViewX || pos.x > mMaxViewX || pos.z < mMinViewZ || pos.z > mMaxViewZ;
    }

#if UNITY_EDITOR
    // DEBUG
    public void DrawSpawnPoints()
    {
        if (mSpawnPts != null)
        {
            for (int i = 0; i < mSpawnPts.Length; ++i)
            {
                Gizmos.DrawSphere(mSpawnPts[i], 1.0f);
            }
        }
    }
#endif

    static readonly GameScene msSingleton = new GameScene();
    System.Action<float> mGameUpdateProc = null;
    System.Predicate<Projectile> mProjClipFunc;
    GamePreload mPreloadStuff = new GamePreload();
    List<ActorSpawnSchedule> mRegionSchedule = new List<ActorSpawnSchedule>();
    bool mGameInitialized = false;
    float mGameTimeScale = 1.0f;
    float mElapsed = 0;
    Vector3[] mSpawnPts;
    Quaternion[] mSpawnerRots;
    int mCurrentWaveIndex;
    float mDelay2Push = 0;
    int mWave_Delay2Push = -1;
    // view clip
    float mMinViewX;
    float mMaxViewX;
    float mMinViewZ;
    float mMaxViewZ;
}

public class ActorSpawnSchedule
{
    public string mPrefabName;
    public Actor.ActorType mActorType;
    public Vector3 mRelPos;
    public Quaternion mRot;
}