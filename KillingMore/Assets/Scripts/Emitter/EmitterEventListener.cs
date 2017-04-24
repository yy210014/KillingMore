using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Emitter
{
    public class EmitterEventListener
    {
        public delegate void EmitterDelegate(Emitter_Basic emitter);
        event EmitterDelegate mCallback;
        Emitter_Basic mEmitter;
        public EmitterEventListener(Emitter_Basic emitter)
        {
            mEmitter = emitter;
        }

        public bool isEmpty
        {
            get { return mCallback == null; }
        }

        public void Call()
        {
            if (mCallback != null)
                mCallback(mEmitter);
        }

        public void Add(EmitterDelegate callback)
        {
            mCallback -= callback;
            mCallback += callback;
        }

        public void Remove(EmitterDelegate callback)
        {
            mCallback -= callback;
        }

        public void Clear()
        {
            mCallback = null;
        }
    }
}