using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : GameState
{

    #region 父类方法
    public override States GetStateEnum()
    {
        return GameState.States.Main;
    }

    public override void Initialize()
    {
    }

    public override void PostInitialize()
    {

    }

    public override bool DoGameUpdate()
    {
        // TODO
        return false;
    }

    public override void Uninitialize()
    {
    }

    public override void PostUninitialize()
    {

    }
    #endregion

    #region ButtonOnClick
    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        Game.Singleton.SwitchScene("Battle");
    }

    #endregion

}
