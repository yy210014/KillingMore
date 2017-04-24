using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using Emitter;

public class ActorManage
{
    public const string ActorTag_Untagged = "Untagged";
    public const string ActorTag_Player = "Player";
    public const string ActorTag_Enemy = "Enemy";
    public const string ActorTag_Projectile = "Projectile";
    public const string ActorTag_Effect = "Effect";
    public static ActorManage Singleton { get { return msSingleton; } }

    public List<Actor> Aircrafts { get { return mEnemyActors; } }
    public List<Actor> PlayerActors { get { return mPlayerActors; } }
    public List<SpecialEffect> Effects { get { return mEffect; } }
    public List<Projectile> Projectiles { get { return mProjectiles; } }

    public void OnGameUpdate(float dt)
    {
        for (int i = 0; i < mPlayerActors.Count; ++i)
        {
            if (mPlayerActors[i] != null)
            {
                mPlayerActors[i].OnGameUpdate(dt);
            }
            else
            {
                Debug.LogError("-->" + mPlayerActors[i].name + "丢失！");
            }
        }

        // update actors
        for (int i = 0; i < mEnemyActors.Count; ++i)
        {
            if (mEnemyActors[i] != null)
            {
                mEnemyActors[i].OnGameUpdate(dt);
            }
            else
            {
                Debug.LogError("-->" + mEnemyActors[i].name + "丢失！");
            }
        }

        for (int i = 0; i < mProjectiles.Count; ++i)
        {
            if (mProjectiles[i] != null)
            {
                mProjectiles[i].OnGameUpdate(dt);
            }
            else
            {
                Debug.LogError("-->" + mProjectiles[i].name + "丢失！");
            }
        }

        for (int i = 0; i < mEffect.Count; ++i)
        {
            if (mEffect[i] != null)
            {
                mEffect[i].OnGameUpdate(dt);
            }
            else
            {
                Debug.LogError("-->" + mEffect[i].PrefabName + "丢失！");
            }
        }

        // 移除超过可视范围过长的对象
        DestroyInvisibleProjectiles();
    }

    public void TutorialUpdate(float dt)
    {
        for (int i = 0; i < mProjectiles.Count; ++i)
        {
            mProjectiles[i].OnGameUpdate(dt);
        }
        DestroyInvisibleProjectiles();
    }

    public void Clear()
    {
        mPlayerActors.Clear();
        mEnemyActors.Clear();
        mProjectiles.Clear();
        mEffect.Clear();
    }

    public bool OnActorSpawn(Actor a)
    {
        switch (a.GetActorType())
        {
            case Actor.ActorType.Character:
                return OnPlayerActorSpawn(a);
            case Actor.ActorType.Enemy:
                return OnEnemyActorSpawn(a);
        }
        return false;
    }

    public bool OnEnemyActorSpawn(Actor a)
    {
        if (!mEnemyActors.Contains(a))
        {
            if (a.tag == ActorManage.ActorTag_Untagged)
            {
                a.tag = ActorManage.ActorTag_Enemy;
            }
            mEnemyActors.Add(a);
            return true;
        }
        return false;
    }

    public bool OnPlayerActorSpawn(Actor a)
    {
        if (!mPlayerActors.Contains(a))
        {
            if (a.tag == ActorManage.ActorTag_Untagged)
            {
                a.tag = ActorManage.ActorTag_Enemy;
            }
            mPlayerActors.Add(a);
            return true;
        }
        return false;
    }

    public bool OnEffectSpawn(SpecialEffect effect)
    {
        if (!mEffect.Contains(effect))
        {
            effect.tag = ActorManage.ActorTag_Effect;
            mEffect.Add(effect);
            return true;
        }
        return false;
    }

    public bool OnSpawnProjectile(Projectile p)
    {
        if (!mProjectiles.Contains(p))
        {
            p.tag = ActorManage.ActorTag_Projectile;
            mProjectiles.Add(p);
            return true;
        }
        return false;
    }

    public bool DestroyEnemyActor(Actor ac)
    {
        if (!mEnemyActors.Contains(ac))
        {
            return false;
        }
        AssetsManager.Singleton.DestroyGameObject(ac.gameObject, AssetsManager.ResType.Character, ac.name);
        mEnemyActors.Remove(ac);
        return true;
    }

    public bool DestroyPlayerActor(Actor ac)
    {
        if (!mPlayerActors.Contains(ac))
        {
            return false;
        }
        AssetsManager.Singleton.DestroyGameObject(ac.gameObject,
            AssetsManager.ResType.Character, ac.name);
        mPlayerActors.Remove(ac);
        return true;
    }

    public bool DestroyEffect(SpecialEffect se)
    {
        if (!mEffect.Contains(se))
        {
            return false;
        }
        AssetsManager.Singleton.DestroyGameObject(se.gameObject,
            AssetsManager.ResType.Effect, se.PrefabName);
        mEffect.Remove(se);
        return true;
    }

    public bool DestroyProjectile(Projectile pj)
    {
        if (!mProjectiles.Contains(pj))
        {
            return false;
        }
        AssetsManager.Singleton.DestroyGameObject(pj.gameObject,
            AssetsManager.ResType.Projectile, pj.name);
        mProjectiles.Remove(pj);
        return true;
    }

    public void DestroyAllEnemyActor()
    {
        for (int i = 0; i < mEnemyActors.Count; )
        {
            if (mEnemyActors[i] != null)
            {
                AssetsManager.Singleton.DestroyGameObject(mEnemyActors[i].gameObject, AssetsManager.ResType.Character,
                    mEnemyActors[i].name);
                mEnemyActors.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }

    public void DestroyAllPlayerActor()
    {
        for (int i = 0; i < mPlayerActors.Count; )
        {
            if (mPlayerActors[i] != null)
            {
                AssetsManager.Singleton.DestroyGameObject(mPlayerActors[i].gameObject,
                    AssetsManager.ResType.Character,
                    mPlayerActors[i].name);
                mPlayerActors.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }

    public void DestroyAllEffect()
    {
        for (int i = 0; i < mEffect.Count; )
        {
            if (mEffect[i] != null)
            {
                AssetsManager.Singleton.DestroyGameObject(mEffect[i].gameObject,
                    AssetsManager.ResType.Effect,
                    mEffect[i].name);
                mEffect.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }

    public void DestroyAllProjectile()
    {
        for (int i = 0; i < mProjectiles.Count; )
        {
            if (mProjectiles[i] != null)
            {
                AssetsManager.Singleton.DestroyGameObject(mProjectiles[i].gameObject,
                    AssetsManager.ResType.Projectile,
                    mProjectiles[i].PrefabName);
                mProjectiles.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }

    public void DestroyFactionProjectile(int Faction)
    {
        for (int i = 0; i < mProjectiles.Count; )
        {
            if (mProjectiles[i] != null && mProjectiles[i].FactionId == Faction)
            {
                AssetsManager.Singleton.DestroyGameObject(mProjectiles[i].gameObject,
                    AssetsManager.ResType.Projectile,
                    mProjectiles[i].PrefabName);
                mProjectiles.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }

    void DestroyInvisibleProjectiles()
    {
        for (int i = 0; i < mProjectiles.Count; )
        {
            if (GameScene.Singleton.ProjClipFunc(mProjectiles[i]))
            {
                AssetsManager.Singleton.DestroyGameObject(mProjectiles[i].gameObject,
                    AssetsManager.ResType.Projectile,
                    mProjectiles[i].PrefabName);
                mProjectiles.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }

    static readonly ActorManage msSingleton = new ActorManage();
    List<Actor> mEnemyActors = new List<Actor>();
    List<Actor> mPlayerActors = new List<Actor>();
    List<SpecialEffect> mEffect = new List<SpecialEffect>();
    List<Projectile> mProjectiles = new List<Projectile>();
}
