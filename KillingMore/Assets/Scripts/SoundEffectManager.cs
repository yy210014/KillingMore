using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundEffectManager
{
    static SoundEffectManager msSingleton = new SoundEffectManager();
    
    public static SoundEffectManager Singleton
    {
        get
        {
            return msSingleton;
        }
    }

    class SoundEfcSlot
    {
        public AudioSource mSource;
        public string mPrefabName;
    }

    List<SoundEfcSlot> mSoundEfcs = new List<SoundEfcSlot>();

    public const int MaxSameEfcLimited = 10;

    public void OnGameStart()
    {
        for (int i = 0; i < mSoundEfcs.Count; ++i)
        {
            AssetsManager.Singleton.DestroyGameObject(mSoundEfcs[i].mSource.gameObject, AssetsManager.ResType.SoundEffect,
                mSoundEfcs[i].mPrefabName);
        }
        mSoundEfcs.Clear();
    }

    public void OnGameUpdate()
    {
        for (int i = 0; i < mSoundEfcs.Count; ++i)
        {
            if (!mSoundEfcs[i].mSource.isPlaying)
            {
                AssetsManager.Singleton.DestroyGameObject(mSoundEfcs[i].mSource.gameObject, AssetsManager.ResType.SoundEffect,
                    mSoundEfcs[i].mPrefabName);

                mSoundEfcs.RemoveAt(i);
                --i;
            }
        }
    }

    public void PlaySoundEffect(string prefabName)
    {
        // 检查是否已经存在限制数量个数
        int num = 0;
        for (int i = 0; i < mSoundEfcs.Count; ++i)
        {
            if (mSoundEfcs[i].mPrefabName == prefabName)
            {
                ++num;
            }
        }

        if (num >= MaxSameEfcLimited)
        {
            Game.LogWarning("加载音效 " + prefabName + " 数量超过限制");
            return;
        }

        GameObject g = AssetsManager.Singleton.FindNPopFromPool(prefabName, AssetsManager.ResType.SoundEffect);
        if (g != null)
        {
            g.SetActive(true);
            AudioSource aus = g.GetComponent<AudioSource>();
            aus.loop = false;
            aus.Play();
            SoundEfcSlot slot = new SoundEfcSlot();
            slot.mPrefabName = prefabName;
            slot.mSource = aus;
            mSoundEfcs.Add(slot);

            GameObject.DontDestroyOnLoad(g);

            return;
        }

        AudioClip ac = AssetsManager.Singleton.LoadObjectFromPathT<AudioClip>(prefabName, "Audio");
        if (ac != null)
        {
            g = new GameObject();
            g.name = "SoundEffect_" + prefabName;
            AudioSource aus = g.AddComponent<AudioSource>();
            aus.loop = false;
            aus.clip = ac;
            aus.Play();
            SoundEfcSlot slot = new SoundEfcSlot();
            slot.mPrefabName = prefabName;
            slot.mSource = aus;
            mSoundEfcs.Add(slot);

            GameObject.DontDestroyOnLoad(g);
        }
        else
        {
            Game.LogError("加载音效 " + prefabName + " 失败!");
        }
    }

    public void EndSoundEffect(string name)
    {
        for (int i = 0; i < mSoundEfcs.Count; ++i)
        {
            if (mSoundEfcs[i].mPrefabName == name)
            {
                mSoundEfcs[i].mSource.Stop();
            }
        }
    }
    
    public void OnGameEnd()
    {
        for (int i = 0; i < mSoundEfcs.Count; ++i)
        {
            AssetsManager.Singleton.DestroyGameObject(mSoundEfcs[i].mSource.gameObject, AssetsManager.ResType.SoundEffect,
                mSoundEfcs[i].mPrefabName);
        }
        mSoundEfcs.Clear();
    }
}
