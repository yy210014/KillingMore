using System;
using System.Collections.Generic;

public class InputAction
{
    public void Add(Action act)
    {
        mAction += act;
    }

    public void UniqueAdd(Action act)
    {
        mAction = act;
    }

    public void Delete(Action act)
    {
        mAction -= act;
    }

    public void Clear()
    {
        mAction = null;
    }

    public void Call()
    {
        if (mAction != null)
        {
            mAction.Invoke();
        }
    }

    Action mAction = null;
}
