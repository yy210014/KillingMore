using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BasicAbility
{
    public BasicAbility(Actor owner)
    {
        this.mOwner = owner;
        mOwner.AddAbility(this);
    }

    public int ID
    {
        get { return mID; }
    }

    public Actor Owner
    {
        get { return mOwner; }
    }

    public virtual string name
    {
        get
        {
            return mName;
        }
        set
        {
            value = mName;
        }
    }

    /// <summary>
    /// 注意最小值为1,最大值为MaxLevel，索引从1开始
    /// </summary>
    public int Level
    {
        get { return mLevel; }
    }

    public int MaxLevel
    {
        get { return mMaxLevel; }
    }

    public float Force
    {
        get { return mForce; }
    }

    public void SetID(int id)
    {
        mID = id;
    }

    public void SetLevel(int level)
    {
        mLevel = Mathf.Clamp(level, 1, mMaxLevel);
    }

    public void SetMaxLevel(int maxLevel)
    {
        mMaxLevel = Mathf.Clamp(maxLevel, 1, maxLevel);
    }

    public virtual void SetForce(float force)
    {
        mForce = force;
    }

    public virtual void ReplaceOwner(Actor owner)
    {
        mOwner.RemoveAbility(this);
        mOwner = owner;
        mOwner.AddAbility(this);
    }

    public virtual void OnGameUpdate(float dt)
    {
        if (mCurAction != null)
        {
            mCurAction(dt);
        }
    }

    public virtual void OwnerDie()
    {

    }

    protected System.Action<float> mCurAction;
    protected Actor mOwner;
    string mName = "";
    int mID = -1;
    int mLevel = 1;
    int mMaxLevel = 1;
    float mForce;
}
