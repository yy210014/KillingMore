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


#if UNITY_EDITOR
    void OnGUI()
    {
        if (Game.Singleton != null && Game.Singleton.DebugStringOnScreen)
        {
            int x = 10;
            int y = 10;
            int w = 300;
            int h = 60;
            int dy = 20;

            GUI.color = new Color(1.0f, 1.0f, 1.0f);
            Rect rc = new Rect();
            rc.Set(x, y, w, h);
            GUI.Label(rc, "Elapsed " + GameScene.Singleton.Elapsed);
            rc.Set(x, y + dy, w, h);

            int i = 2;
            Game.IteraterAllDebugStrings((s) =>
                  {
                      rc.Set(x, y + i * dy, w, h);
                      GUI.Label(rc, "Debug : " + s);
                      ++i;
                  });

            x = 200;
            y = 10;
            GUI.color = new Color(1.0f, 0, 0);
            i = 0;
            Game.IteraterAllErrorStrings((s) =>
            {
                rc.Set(x, y + i * dy, w, h);
                GUI.Label(rc, "Error : " + s);
                ++i;
            });
        }
    }
#endif

}
