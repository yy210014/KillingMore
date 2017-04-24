using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : GameState
{
    #region 父类方法
    public override States GetStateEnum()
    {
        return GameState.States.Battle;
    }

    public override void Initialize()
    {
        GameScene.Singleton.OnGameInitialize();
    }

    public override void PostInitialize()
    {
        GameScene.Singleton.OnGameStart();
    }

    public override bool DoGameUpdate()
    {
        GameScene.Singleton.OnGameUpdate();
        return true;
    }

    public override void Uninitialize()
    {
        GameScene.Singleton.OnGameEnd();
    }

    public override void PostUninitialize()
    {
        GameScene.Singleton.OnGameUninitialize();
        SoundEffectManager.Singleton.OnGameEnd();
    }
    #endregion
}
