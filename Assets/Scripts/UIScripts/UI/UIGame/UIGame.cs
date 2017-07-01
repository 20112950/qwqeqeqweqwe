using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGame : UIBase
{
    private GameObject _trash;

    private GameObject _undo;

    private GameObject _hammer;

    private GameObject _pause;

    private Text _score;

    private Text _coin;

    private Text _best_score;

    public delegate void AddScoreDelegate(int score);

    public static AddScoreDelegate AddScoreEvent;

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
        _score = tran.FindChild("Score/Text").gameObject.GetComponent<Text>();
        _coin = tran.FindChild("Gold/Gold/gold").gameObject.GetComponent<Text>();
        _best_score = tran.FindChild("Gold/Highest/highest").gameObject.GetComponent<Text>();
        EventTriggerListener.Get(_trash).onClick = Trash;
        EventTriggerListener.Get(_undo).onClick = Undo;
        EventTriggerListener.Get(_hammer).onClick = Hammer;
        EventTriggerListener.Get(_pause).onClick = Pause;       
        return true;
    }

    public override void OnShow(object baseParam)
    {
        base.OnShow(baseParam);
        AddScoreEvent += AddScore;
        _score.text = LevelManager.instance.score+"";
    }

    public override void OnClose()
    {
        base.OnClose();
        AddScoreEvent -= AddScore;
        AdMobManager.Instance.RemoveBanner();
    }

    private void Undo(GameObject obj)
    {
        if (LevelManager.game_handle_state == GameHandleState.NULL)
        {
            LevelManager.game_handle_state = GameHandleState.Undo;
            LevelManager.instance.Undo();
        }
            
    }

    private void Trash(GameObject obj)
    {
        LevelManager.instance.TrashItemStruct(0.5f);
    }

    public void Hammer(GameObject obj)
    {
        if (LevelManager.game_handle_state == GameHandleState.NULL || LevelManager.game_handle_state == GameHandleState.Hammer )
        {
            if(LevelManager.game_handle_state == GameHandleState.Hammer)
            {
                LevelManager.instance.ShowHammers(false);
                LevelManager.game_handle_state = GameHandleState.NULL;
            }else if (LevelManager.game_handle_state == GameHandleState.NULL)
            {
                if (LevelManager.instance.ShowHammers(true))
                {
                    LevelManager.game_handle_state = GameHandleState.Hammer;
                }
            }
        }
      
        
    }

    private void Pause(GameObject obj)
    {
        UIManager.Instance.Show(emUIWindow.emUIWindow_GamePause, false);
    }

    public static void HandleAddScore(int socre)
    {
        if (AddScoreEvent != null)
        {
            AddScoreEvent(socre);
        }
    }
    private void AddScore(int socre)
    {
        this._score.text = LevelManager.instance.score + socre+"";
        LevelManager.instance.score += socre;
    }
}
