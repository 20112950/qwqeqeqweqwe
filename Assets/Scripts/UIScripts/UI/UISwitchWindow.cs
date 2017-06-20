using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//此窗口用来处理黑屏过渡，全程不关闭
public class UISwitchWindow : UIBase {
    private float m_fDelayTime = 0.8f;
    private GameObject m_objBlock = null;
    public float DelayTime
    {
        get { return m_fDelayTime; }
    }

    public UISwitchWindow() : base(emUIWindow.SwitchWindow, emUIType.emUIType_POP)
    {
    }

    public override bool OnInit(GameObject i_UIRoot)
    {
        base.OnInit(i_UIRoot);
        m_fDelayTime = Time.fixedDeltaTime * 14;

        m_objBlock = UIBase.FindChildObj(m_rootTF.parent, "Blocker");
        return true;
    }

    public override void OnShow(object baseParam)
    {
        base.OnShow(baseParam);
        m_rootObj.SetActive(false);
    }

    public void Switch()
    {
        if (m_rootObj == null)
            return;
        if (!IsInited())
            return;

        m_rootObj.SetActive(false);
        m_rootObj.SetActive(true);

        //显示block
        ShowBlock(null);
        DelayFunc.CallFuncDelay(HideBlock, DelayTime);
    }

    public void ShowBlock(EventTriggerListener.VoidDelegate onClickBlock)
    {
        EventTriggerListener.Get(m_objBlock).onClick = onClickBlock;
        m_objBlock.SetActive(true);
    }

    public void HideBlock()
    {
        EventTriggerListener.Get(m_objBlock).onClick = null;
        m_objBlock.SetActive(false);
    }
}
