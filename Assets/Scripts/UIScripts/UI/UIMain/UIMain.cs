using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMain : UIBase
{
    private GameObject _start_classic_game;
    private GameObject _start_deformation_game;
    public UIMain() : base(emUIWindow.emUIWindow_Main, emUIType.emUIType_ROOT)
    {
    }

    public override bool OnInit(GameObject i_UIRoot)
    {
        if (!base.OnInit(i_UIRoot))
        {
            return false;
        }
        Transform tran = m_rootObj.transform;
        _start_classic_game = tran.FindChild("start_classic_game").gameObject;
        _start_deformation_game = tran.FindChild("start_deformation_game").gameObject;
        EventTriggerListener.Get(_start_classic_game).onClick = StartClassicGame; 
        EventTriggerListener.Get(_start_deformation_game).onClick = StartDeformationGame;
        return true;
    }

    public override void OnShow(object baseParam)
    {
        base.OnShow(baseParam);
    }

    private void StartClassicGame(GameObject obj)
    {
        UIManager.Instance.Show(emUIWindow.emUIWindow_Game,true);
        LevelManager.instance.CreateMaps(GameMode.CLASSIC);
    }

    private void StartDeformationGame(GameObject obj)
    {
        UIManager.Instance.Show(emUIWindow.emUIWindow_Game, true);
        LevelManager.instance.CreateMaps(GameMode.DEFORMATION);
    }
}
