using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGamePause : UIBase
{
    private GameObject _close;

    private GameObject _main_panel;
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
        _close = tran.FindChild("close").gameObject;
        _main_panel = tran.FindChild("main_panel").gameObject;
        EventTriggerListener.Get(_close).onClick = PauseClose;
        EventTriggerListener.Get(_main_panel).onClick = MainPanel;
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
        Close();
        //UIManager.Instance.Back();
    }
}
