using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGamePause : UIBase
{
    private GameObject _close;

    private GameObject _main_panel;

    private GameObject _settings;

    private GameObject _reset;

    public UIGamePause() : base(emUIWindow.emUIWindow_GamePause, emUIType.emUIType_POP)
    {
    }

    public override bool OnInit(GameObject i_UIRoot)
    {
        if (!base.OnInit(i_UIRoot))
        {
            return false;
        }
        Transform tran = m_rootObj.transform;
        _close = tran.FindChild("Integrated/all/Close").gameObject;
        _main_panel = tran.FindChild("Integrated/all/bottom/home").gameObject;
        _settings = tran.FindChild("Integrated/all/bottom/settings").gameObject;
        _reset = tran.FindChild("Integrated/all/bottom/reset").gameObject;
        EventTriggerListener.Get(_close).onClick = PauseClose;
        EventTriggerListener.Get(_main_panel).onClick = MainPanel;
        EventTriggerListener.Get(_settings).onClick = Setting;
        EventTriggerListener.Get(_reset).onClick = Reset;
        return true;
    }

    public override void OnShow(object baseParam)
    {
        base.OnShow(baseParam);
    }

    private void PauseClose(GameObject obj)
    {
        Close();
    }

    private void MainPanel(GameObject obj)
    {
        UIManager.Instance.Back();
        Close();
        LevelManager.instance.DestroyGameAll();
    }

    private void Setting(GameObject obj)
    {
        UIManager.Instance.Show(emUIWindow.emUIWindow_Setting, false);
    }

    private void Reset(GameObject obj)
    {
        Close();
        LevelManager.instance.ResetGame();
        UIGame.HandleAddScore(0);
    }
}
