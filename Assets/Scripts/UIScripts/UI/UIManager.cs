using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// 点击事件回调
/// </summary>
/// <param name="go">按钮</param>
delegate void OnClick_CallBack(GameObject go);

// UI界面类型  窗口枚举值的大小关系到Canvas下当前显示UI的层级
public enum emUIWindow
{
    emUIWindow_Invalid = -1,
    emUIWindow_Login,       //登陆界面
    emUIWindow_Main,        //主城界面
    emUIWindow_Game, // 游戏中
    emUIWindow_GamePause, // 游戏暂停
    emUIWindow_Setting,
    SwitchWindow,               //黑屏过渡
    emUIWindow_Max,
}

// UI管理器
public class UIManager
{
    private UIManager() { }

    #region Property
    public static UIManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new UIManager();
            }
            return m_Instance;
        }
    }

    //UI Canvas的Transform
    public static Transform UIRoot
    {
        get; private set;
    }

    public static Canvas canvas { get; private set; }

    #endregion

    #region Field

    static private UIManager m_Instance;
    private UIBase[] m_UIList;            // 所有UI界面
    private Dictionary<emUIWindow, int> m_dicShowWindow = new Dictionary<emUIWindow, int>();
    private Stack<UIBase> m_stkNavigateUI;    // 导航栈
    private Dictionary<emUIWindow, int> m_dicOtherWindow = new Dictionary<emUIWindow, int>();
    private UIBase m_CurUI;               // 当前打开的NormalUI窗口
    private CanvasScaler m_CanvasScaler;

    static public List<string> LoginRes = new List<string>();
    static public List<string> MainRes = new List<string>();
    static public List<string> IconRes = new List<string>();

    private FadeBackground m_FadeBackground;  // 渐变背景,控制渐变效果

    private GameObject m_mainCamera;        //主城摄像机
    public GameObject MainCamera
    {
        get
        {
            if (m_mainCamera == null)
            {
                m_mainCamera = GameObject.Find("MainCamera");
            }
            return m_mainCamera;
        }
        private set
        {
            m_mainCamera = value;
        }
    }

    public int PopSortingLayerID
    {
        get; set;
    }

    #endregion

    #region Method

    // 初始化UI模块
    public void Create()
    {
        PopSortingLayerID = 100;
        // 获得UI根节点
        GameObject objCanvas = GameObject.Find("Canvas");
        if (objCanvas == null)
        {
            return;
        }
        UIRoot = objCanvas.transform.FindChild("UIRoot");
        canvas = objCanvas.GetComponent<Canvas>();

        m_CanvasScaler = objCanvas.GetComponent<CanvasScaler>();
        if ((m_CanvasScaler == null) || (m_CanvasScaler.transform == null))
        {
            return;
        }
        Transform fadeBackgroundTF = m_CanvasScaler.transform.FindChild("UIRoot/BlackImage");
        if ((fadeBackgroundTF == null) || (fadeBackgroundTF.gameObject == null))
        {
            return;
        }
        m_FadeBackground = fadeBackgroundTF.gameObject.GetComponent<FadeBackground>();

        m_UIList = new UIBase[(int)emUIWindow.emUIWindow_Max];

        m_UIList[(int)emUIWindow.emUIWindow_Main] = new UIMain();
        m_UIList[(int)emUIWindow.emUIWindow_Game] = new UIGame();
        m_UIList[(int)emUIWindow.emUIWindow_GamePause] = new UIGamePause();
        m_UIList[(int)emUIWindow.emUIWindow_Setting] = new UIGameSetting(); 
         //m_UIList[(int)emUIWindow.emUIWindow_Two] = new UITwo();
         //m_UIList[(int)emUIWindow.emUIWindow_UIPopOne] = new UIPopOne();
         //m_UIList[(int)emUIWindow.SwitchWindow] = new UISwitchWindow();

         m_stkNavigateUI = new Stack<UIBase>();
    }

    public UIBase InitUI(emUIWindow wnd)
    {
        UIBase uiWnd = m_UIList[(int)wnd];
        try
        {
            m_UIList[(int)wnd].InitScene();
        }
        catch (System.Exception e)
        {
            try
            {
                m_UIList[(int)wnd].Close();
            }
            catch (System.Exception e2)
            {
                Develop.LogError(e2);
            }

            Develop.LogErrorF("UIManager.InitUI Exception! emUIWindow:{0}, Exception:{1}", wnd, e.StackTrace);
        }
        return uiWnd;
    }


    public void Update()
    {
        if (m_UIList == null)
        {
            return;
        }

        for (int i = 0; i < (int)emUIWindow.emUIWindow_Max; i++)
        {
            if (i < m_UIList.Count() && m_UIList[i] != null)
            {
                if (m_UIList[i].IsShow())
                {
                    m_UIList[i].Update();
                }
            }
        }
    }

    //获取当前UI 
    public emUIWindow GetCurWnd()
    {
        return m_CurUI != null ? m_CurUI.GetWindowType() : emUIWindow.emUIWindow_Invalid;
    }

    //获取UI对象
    public UIBase GetUI(emUIWindow wnd)
    {
        if (wnd < 0 || wnd >= emUIWindow.emUIWindow_Max)
        {
            return null;
        }
        return m_UIList[(int)wnd];
    }

    public void BlackImage(bool isShow)
    {
        m_FadeBackground.BlackImage(isShow);
    }

    /// <summary>
    /// 隐藏当前窗口
    /// </summary>
    public void HideCurr()
    {
        if (m_CurUI != null)
        {
            m_CurUI.HideTemporary();
        }
    }

    private void CloseUI(UIBase ui)
    {
        if (ui == null)
        {
            return;
        }
        ui.Close();
    }
    private void HideUI(UIBase ui)
    {
        if (ui == null)
        {
            return;
        }
        ui.HideTemporary();
    }

    /// <summary>
    /// 显示UI
    /// </summary>
    /// <param wnd="UI窗口类型"></param>
    /// <param bCloseCur="是否隐藏当前UI, POP窗口打开的时候会隐藏当前Normal UI，关闭的时候会自动打开当前UI"></param>
    /// <param needBack="是否需要返回  只有是normal类型的窗口才有效 如果是false 则按照pop类型窗口处理，自己负责关闭，UIManager不管理"></param>
    public UIBase Show(emUIWindow i_eUIWindow, bool bHideCur, object baseParam = null, bool needBack = true)
    {
        if ((i_eUIWindow < 0) || (i_eUIWindow >= emUIWindow.emUIWindow_Max))
        {
            return null;
        }

        //当前窗口就是要显示的 不处理
        emUIWindow currUIID = m_CurUI != null ? m_CurUI.GetWindowType() : emUIWindow.emUIWindow_Invalid;
        if (currUIID == i_eUIWindow)
        {
            if (!m_CurUI.IsShow())
            {
                m_CurUI.ShowFromManager();
            }
            return m_CurUI;
        }

        Develop.LogF("---------UIManager.Show window:{0}, currWin:{1}, ifHideCUrr:{2}, ifNeedBack:{3}", i_eUIWindow, currUIID, bHideCur, needBack);

        // 设置当前最前面的UI
        UIBase newUI = m_UIList[(int)i_eUIWindow];
        if (newUI == null)
        {
            return null;
        }
        emUIType newUIType = newUI.GetUIType();

        if (m_CurUI != null && newUIType == emUIType.emUIType_Normal && !m_CurUI.NeedBack)   //如果是普通窗口不需要返回 则关闭
        {
            CloseUI(m_CurUI);
            m_CurUI = null;
        }
        else if (bHideCur && m_CurUI != null && m_CurUI != newUI && m_CurUI.IsShow())
        {
            HideUI(m_CurUI);
            newUI.WindowHide = currUIID;
        }
        else if (!bHideCur && m_CurUI != null)
        {
            newUI.WindowHide = emUIWindow.emUIWindow_Invalid;
        }

        if (newUIType != emUIType.emUIType_POP)
        {
            m_CurUI = newUI;
            newUI.NeedBack = needBack;
        }

        //Main窗口以及，支持返回的Normal窗口
        if (newUIType == emUIType.emUIType_ROOT || (needBack && newUIType == emUIType.emUIType_Normal))
        {
            Develop.Log("Current UI:" + m_CurUI.GetWindowType());

            //当前打开的是主窗口的时候 清除导航栈里的所有UI
            if (newUIType == emUIType.emUIType_ROOT)
            {
                CloseAllNavigate();     //这里会把Main窗口也pop出来，所以下一步需要重新push回去
            }

            m_stkNavigateUI.Push(newUI);
        }
        else
        {
            m_dicOtherWindow[i_eUIWindow] = 1;
        }


        m_dicShowWindow[i_eUIWindow] = 1;
        m_FadeBackground.m_IsBlack = true;
        if (m_FadeBackground.m_IsBlack)
        {
            m_FadeBackground.FadeOut();
        }

        newUI.ParamForShow = baseParam;

        // 打开新的UI
        newUI.ShowFromManager();
        return newUI;
    }

    /// <summary>
    /// 关闭当前导航UI栈里的所有UI
    /// </summary>
    public void CloseAllNavigate()
    {
        while (m_stkNavigateUI.Count > 0)
        {
            UIBase ui = m_stkNavigateUI.Pop();

            if (ui == null)
            {
                continue;
            }

            if (m_CurUI == null || ui.GetWindowType() != m_CurUI.GetWindowType())
            {
                ui.Close();
            }
        }
    }


    public int GetSiblindIdxOfWindow(emUIWindow wnd, Transform transWnd)
    {
        if (transWnd == null)
        {
            return 0;
        }

        int iIdx = UIRoot.childCount;       //默认是最后一个节点
        foreach (KeyValuePair<emUIWindow, int> keyval in m_dicShowWindow)
        {
            UIBase uiWnd = m_UIList[(int)keyval.Key];
            if (uiWnd == null || uiWnd.gameObject == null || (int)wnd >= (int)keyval.Key)
            {
                continue;
            }

            int thisIdx = uiWnd.RootTransfrom.GetSiblingIndex();
            if (iIdx > thisIdx)
            {
                iIdx = thisIdx;
            }
        }
        if (transWnd.GetSiblingIndex() < iIdx)
        {
            iIdx--;
        }

        return iIdx;
    }


    /// <summary>
    /// 外部可以根据UIManager的这个接口关闭窗口，也可以直接调用UIBase的关闭窗口
    /// </summary>
    /// <param name="windowID"></param>
    public void Close(emUIWindow windowID)
    {
        UIBase uibase = GetUI(windowID);
        if (uibase != null)
        {
            uibase.Close();
        }
    }

    /// <summary>
    /// 这个接口只能UIBase调用！！ 
    /// </summary>
    /// <param name="windowID"></param>
    public void DoClose(emUIWindow windowID)
    {
        UIBase uiClosed = m_UIList[(int)windowID];
        if (uiClosed == null)
        {
            return;
        }

        if (m_dicShowWindow.ContainsKey(windowID))
        {
            m_dicShowWindow.Remove(windowID);
        }
        else
        {
            Develop.LogWarningF("Trying to close a window have not open!!windowID:{0}", windowID.ToString());
        }

        if (m_dicOtherWindow.ContainsKey(windowID))
        {
            m_dicOtherWindow.Remove(windowID);
        }

        if (m_CurUI != null && windowID == m_CurUI.GetWindowType())
        {
            if (m_stkNavigateUI.Count > 0)
            {
                m_CurUI = m_stkNavigateUI.Peek();
            }
            else
            {
                m_CurUI = null;
            }
        }
        else if (m_CurUI != null && uiClosed.WindowHide == m_CurUI.GetWindowType() && uiClosed.GetUIType() == emUIType.emUIType_POP)        //只有pop窗口在关闭的时候做这个操作，其他窗口走导航
        {
            if (m_CurUI.IsHide())   //如果窗口在Hide之后被其他地方关闭了，则不再打开
            {
                m_CurUI.ShowFromHide();
            }

            uiClosed.WindowHide = emUIWindow.emUIWindow_Invalid;
        }
    }

    public void CloseAll()
    {
        List<emUIWindow> lstWin = m_dicShowWindow.Keys.ToList();
        for (int i = lstWin.Count - 1; i >= 0; i--)
        {
            emUIWindow em = lstWin[i];
            Develop.Log(em);
            m_UIList[(int)em].Close();
        }
        m_dicShowWindow.Clear();
        m_dicOtherWindow.Clear();
        m_stkNavigateUI.Clear();
        m_CurUI = null;
    }


    // 卸载UI
    public void Unload(emUIWindow i_eUIWindow)
    {
        UIBase uiWnd = GetUI(i_eUIWindow);
        if (uiWnd != null)
        {
            uiWnd.Unload();
        }


    }

    //返回
    public void Back()
    {
        Develop.Log("------------UI BACK---------------");
        emUIWindow curWndID = emUIWindow.emUIWindow_Invalid;

        if (m_CurUI != null)
        {
            curWndID = m_CurUI.GetWindowType();

            //主窗口不能关闭
            if (m_CurUI.GetUIType() == emUIType.emUIType_ROOT)
            {
                Develop.LogWarningF("UIManager.Back, curr window:{0}, is a main window can't close by back!!", curWndID.ToString());
                return;
            }

            UIBase uiTop = m_stkNavigateUI.Peek();
            if (uiTop == m_CurUI)        //这里做一个判断是因为，normal类型的窗口可能not needback
            {
                m_stkNavigateUI.Pop();
            }
        }
        ShowWndAtTopOfNgt();
    }

    /// <summary>
    /// 显示栈顶的窗口
    /// </summary>
    private void ShowWndAtTopOfNgt()
    {
        if (m_stkNavigateUI.Count == 0)
        {
            return;
        }

        UIBase newUI = m_stkNavigateUI.Peek();

        //这里用while循环是为了防止有些窗口在导航栈里，但是提早被关掉并且被释放
        while (true && m_stkNavigateUI.Count > 0)
        {
            if (newUI != null && newUI.IsInited())
            {
                if (m_CurUI != null)
                {
                    //Back的时候才会调用当前窗口的Close
                    CloseUI(m_CurUI);
                    m_CurUI = null;
                }

                if (newUI.Status == UIBase.WinStatus.Hide)
                {
                   
                    newUI.ShowFromHide();
                    /*
                                        m_CurUI.ShowUI();       //因为这个窗口入栈的时候就是走的Hide
                                        m_CurUI.Back();*/
                }
                else if (newUI.GetWindowType() == emUIWindow.emUIWindow_Main) //主界面就算没关闭也要刷新
                {
                    newUI.ShowtoFlush();
                }

                m_CurUI = newUI;
                return;
            }

            newUI = m_stkNavigateUI.Pop();
        }

    }

    public Vector3 WorldToScreen(Vector3 vWorldPos)
    {
        float offect = (Screen.width / m_CanvasScaler.referenceResolution.x) * (1 - m_CanvasScaler.matchWidthOrHeight) + (Screen.height / m_CanvasScaler.referenceResolution.y) * m_CanvasScaler.matchWidthOrHeight;
        Vector2 a = RectTransformUtility.WorldToScreenPoint(Camera.main, vWorldPos);
        return new Vector3(a.x / offect, a.y / offect, 0);
    }

    public Vector2 TouchToScreen(Vector2 vTouchPos)
    {
        float offect = (Screen.width / m_CanvasScaler.referenceResolution.x) * (1 - m_CanvasScaler.matchWidthOrHeight) + (Screen.height / m_CanvasScaler.referenceResolution.y) * m_CanvasScaler.matchWidthOrHeight;

        return new Vector2(vTouchPos.x / offect, vTouchPos.y / offect);
    }

    public Vector3 ScreenToWorld(UIBase ui, Camera cam, Vector2 vScreenPos)
    {
        Vector3 a = Vector3.zero;
        RectTransform canvasRt = ui.RootTransfrom as RectTransform;
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRt, vScreenPos, cam, out a))
        {
            Develop.LogError("RectTransformUtility Hit Failed!!!");
        }

        return a;
    }

    public void FadeIn()
    {
        m_FadeBackground.FadeIn();
    }
    #endregion

    //重新初始化
    public void ReInit()
    {
        PopSortingLayerID = 100;

        if (m_UIList == null)
            return;

        UIBase uicurr = null;
        for (int i = 0; i < m_UIList.Length; i++)
        {
            if (m_UIList == null)
                continue;

            uicurr = m_UIList[i];
            if (uicurr == null)
                continue;

            if (!uicurr.IsInited())
            {
                continue;
            }
            if (!uicurr.ReloadFLag)
            {
                uicurr.NoReloadInit();  //虽然不需要重新加载 但是有些东西要重新初始化
                continue;
            }

            GameObject.Destroy(uicurr.WinRoot);
            Type type = uicurr.GetType();
            uicurr = Activator.CreateInstance(type) as UIBase;
            if (uicurr != null)
            {
                m_UIList[i] = uicurr;
            }
        }
    }
}