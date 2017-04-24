using UnityEngine;
using System.Collections;


public class AbilityEventListener
{
    public delegate void AbilityDelegate(Ability ability);
    event AbilityDelegate mCallback;
    Ability mAbility;
    public AbilityEventListener(Ability ability)
    {
        mAbility = ability;
    }

    public bool isEmpty
    {
        get { return mCallback == null; }
    }

    public void Call()
    {
        if (mCallback != null)
            mCallback(mAbility);
    }

    public void Add(AbilityDelegate callback)
    {
        mCallback -= callback;
        mCallback += callback;
    }

    public void Remove(AbilityDelegate callback)
    {
        mCallback -= callback;
    }

    public void Clear()
    {
        mCallback = null;
    }
}
