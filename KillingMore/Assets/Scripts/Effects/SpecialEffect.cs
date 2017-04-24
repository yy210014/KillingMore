using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpecialEffect : MonoBehaviour
{
    public enum EffectType
    {
        Invalid = 0,

        ParticleSystem,
        SpineAnimation
    }

    [Tooltip("需要播放音效的名字")]
    public string AudioName;
    public bool MakeTest;//测试
    [Tooltip("绑定目标物体，同步目标物体位置")]
    public bool BindTarget;
    [Tooltip("动画播放完成后自动销毁！")]
    public bool AutoDestruct;
    [Tooltip("层级排序")]
    public int OrderLayer;

    public event StartEndDelegate Start;
    public event StartEndDelegate End;
    public delegate void StartEndDelegate();

    protected System.Action<float> mCurAction;
    protected List<Renderer> mRenderers = new List<Renderer>();
    protected GameObject mTarget;
    protected float mTimeDT;
    protected float mDuration;

    public GameObject Target { get { return mTarget; } }
    public float Duration { get { return mDuration; } }
    public bool isPlaying { get { return mTimeDT > 0; } }
    public string PrefabName { get; set; }

    public virtual EffectType GetActorType()
    {
        return EffectType.Invalid;
    }

    public virtual void Initialize()
    {
        ActorManage.Singleton.OnEffectSpawn(this);
        Replay();
        OrderInLayer();
    }

    public virtual void OnGameUpdate(float dt)
    {
        if (BindTarget && mTarget != null)
        {
            transform.position = mTarget.transform.position;
        }
        if (mCurAction != null)
        {
            mCurAction(dt);
        }
    }

    public virtual void Replay()
    {
        if (AudioName != null && AudioName != "")
        {
            //      SoundEffectManager.Singleton.PlaySoundEffect(AudioName);
            PlayAudio();
        }
        if (Start != null)
        {
            Start();
        }
        if (mDuration > 0)
        {
            mCurAction = EndTimer;
        }
    }

    void EndTimer(float dt)
    {
        mTimeDT += (dt / GameScene.Singleton.GameTimeScale);
        if (mTimeDT > mDuration)
        {
            //动画播放完成回调，必须实现
            mTimeDT = 0;
            mCurAction = null;
            if (End != null)
            {
                End();
            }
            if (AutoDestruct)
            {
                Destroy();
            }
        }
    }

    void OrderInLayer()
    {
        AddAllChilds<Renderer>(mRenderers, transform);
        for (int i = 0; i < mRenderers.Count; i++)
        {
            if (mRenderers[i] != null)
            {
                mRenderers[i].sortingOrder = OrderLayer;
            }
        }
    }

    void PlayAudio()
    {
        //先加载clip ，判断clip播放时间， 如果大于特效就创建马甲播放音效，否则用自身AudioSource播放
        AudioClip clip = (AudioClip)Resources.Load("Audio/" + AudioName, typeof(AudioClip));//调用Resources方法加载AudioClip资源
        if (clip != null)
        {
            AudioSource audioSource = null;
            if (clip.length >= Duration)
            {
                GameObject effectAudio = AssetsManager.Singleton.LoadGameObjectByResType(
                 EffectConstants.Audio, AssetsManager.RestType2PathName(AssetsManager.ResType.Effect),
                 AssetsManager.ResType.Effect);
                if (effectAudio != null)
                {
                    audioSource = effectAudio.GetComponent<AudioSource>();
                }
                else
                {
                    Game.LogError("找不到" + EffectConstants.Audio + "效果，请检查名字！");
                    return;
                }
                //同步音量
                if (gameObject.GetComponent<AudioSource>() != null)
                {
                    audioSource.volume = gameObject.GetComponent<AudioSource>().volume;
                }
            }
            else
            {
                if (gameObject.GetComponent<AudioSource>() == null)
                {
                    gameObject.AddComponent<AudioSource>();
                }
                audioSource = gameObject.GetComponent<AudioSource>();
            }
            audioSource.clip = clip;
            audioSource.Play();
            AssetsManager.Singleton.DestroyGameObject(
                audioSource.gameObject, AssetsManager.ResType.Effect, EffectConstants.Audio, clip.length + 0.5f);
        }
        else
        {
            Game.LogError("找不到" + AudioName);
        }
    }

    public virtual void SetSize(float size)
    {

    }

    //WE之所以能绑定特效到物体身上是因为 魔兽里面的物体死亡还有尸体存在
    //而这个框架死亡等于立即销毁 所以特效绑定不了在物体身上，只能采用同步的方法
    public virtual void SetTarget(GameObject go)
    {
        mTarget = go;
    }

    public virtual void Destroy()
    {
        Start = null;
        End = null;
        mCurAction = null;
        ActorManage.Singleton.DestroyEffect(this);
    }

    protected void AddAllChilds<T>(List<T> lists, Transform tr) where T : Component
    {
        //递归遍历子节点
        if (tr.childCount > 0)
        {
            int i;
            for (i = 0; i < tr.childCount; i++)
            {
                AddAllChilds<T>(lists, tr.GetChild(i));
            }
        }

        //存入List中
        T t = tr.GetComponent<T>();
        if (t != null)
        {
            lists.Add(t);
        }
    }
}
