using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameSetting : UIBase {

    private GameObject _close;

    public UIGameSetting() : base(emUIWindow.emUIWindow_Setting, emUIType.emUIType_Normal)
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
        EventTriggerListener.Get(_close).onClick = PauseClose;
        return true;
    }

    private void PauseClose(GameObject obj)
    {
        UIManager.Instance.Back();
    }

}
