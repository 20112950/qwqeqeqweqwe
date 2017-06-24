using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGame : UIBase
{
    private GameObject _trash;

    private GameObject _undo;

    private GameObject _hammer;

    private GameObject _pause;
    public UIGame() : base(emUIWindow.emUIWindow_Game, emUIType.emUIType_Normal)
    {
    }

    public override bool OnInit(GameObject i_UIRoot)
    {
        if (!base.OnInit(i_UIRoot))
        {
            return false;
        }
        Transform tran = m_rootObj.transform;
        _trash = tran.FindChild("trash").gameObject;
        _undo = tran.FindChild("undo").gameObject;
        _hammer = tran.FindChild("hammer").gameObject;
        _pause = tran.FindChild("pause").gameObject;
        EventTriggerListener.Get(_trash).onClick = Trash;
        EventTriggerListener.Get(_undo).onClick = Undo;
        EventTriggerListener.Get(_hammer).onClick = Hammer;
        EventTriggerListener.Get(_pause).onClick = Pause;
        return true;
    }

    public override void OnShow(object baseParam)
    {
        base.OnShow(baseParam);
    }

    public override void OnClose()
    {
        base.OnClose();
        AdMobManager.Instance.RemoveBanner();
    }

    private void Undo(GameObject obj)
    {
       
    }

    private void Trash(GameObject obj)
    {
        LevelManager.instance.TrashItemStruct(0.5f);
    }

    private void Hammer(GameObject obj)
    {

    }

    private void Pause(GameObject obj)
    {
        UIManager.Instance.Show(emUIWindow.emUIWindow_GamePause, false);
    }
}
