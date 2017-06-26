using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UScene = UnityEngine.SceneManagement.Scene;
using UnityEngine.UI;

public enum emUIType
{
    emUIType_ROOT,      //根级窗口，back到根级窗口，不能再back，show一个新的main级窗口，会关闭原来所有的其他窗口
    emUIType_Normal,    //支持导航的窗口类型
    emUIType_POP,       //弹出窗口，不支持导航，只能通过close的方式关闭窗口
}

// UI基类
public abstract class UIBase
{
    public static Dictionary<emUIWindow, string> _UI_Prefab_Dic = new Dictionary<emUIWindow, string>()
    {
        { emUIWindow.emUIWindow_Login,          "UILogin"},
        { emUIWindow.emUIWindow_Main,           "UIMain"},
        { emUIWindow.emUIWindow_Game , "UIGame" },
        { emUIWindow.emUIWindow_GamePause , "UIGamePause" },
        { emUIWindow.emUIWindow_Setting , "UI_Set" }
    };

    public static Transform FindDeepChild(Transform parent, string _childName, bool includeInactive = true)
    {
        Transform resultTrs = null;
        resultTrs = parent.transform.Find(_childName);
        if (resultTrs == null)
        {
            foreach (Transform trs in parent.transform)
            {
                //忽略未激活的
                if (!includeInactive && !trs.gameObject.activeSelf)
                    continue;

                resultTrs = FindDeepChild(trs, _childName);
                if (resultTrs != null)
                    return resultTrs;
            }
        }
        return resultTrs;
    }

    public static GameObject FindDeepChildObj(Transform parent, string strName)
    {
        if (parent == null)
        {
            return null;
        }
        Transform transChild = FindDeepChild(parent, strName);
        if (transChild == null)
            return null;
        return transChild.gameObject;
    }

    public static GameObject FindChildObj(Transform parent, string strName)
    {
        if (parent == null)
        {
            return null;
        }
        Transform transChild = parent.Find(strName);
        if (transChild == null)
        {
            Develop.LogErrorF("UIBase.FindChildObj fail! parent:{0}, child name:{1}", parent.ToString(), strName);
            return null;
        }
        return transChild.gameObject;
    }

    public static T FindComponent<T>(Transform parent, string strName) where T : Component
    {
        if (parent == null)
            return null;

        Transform target = parent.Find(strName);
        T comp = null;
        if (target != null)
        {
            comp = target.GetComponent<T>();
        }
        else
        {
            Develop.LogErrorF("UIBase.FindComponent fail! Parent:{0}, child name:{1}", parent.ToString(), strName);
        }
        return comp;
    }

    public static void SetMonoActive(MonoBehaviour go, bool bActive)
    {
        if (go != null)
        {
            go.gameObject.SetActive(bActive);
        }
    }

    public static Vector2 GetItemPosition(Transform tran, Transform canvas)
    {
        Vector2 pos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas as RectTransform,
            tran.position,
            null,
            out pos);

        RectTransform rectTran = tran.GetComponent<RectTransform>();
        float x = pos.x + (0.5f - rectTran.pivot.x) * rectTran.sizeDelta.x;
        float y = pos.y + (0.5f - rectTran.pivot.y) * rectTran.sizeDelta.y;

        return new Vector2(x, y);
    }

    public GameObject gameObject
    {
        get
        {
            return m_rootObj;
        }
    }

    public enum WinStatus
    {
        Show,
        Hide,
        Close,
    }
    public WinStatus Status
    {
        get; private set;
    }


    protected emUIWindow m_eUIWindowID = emUIWindow.emUIWindow_Max;
    protected string m_strPrefabPath = "";

    protected GameObject m_rootObj = null;
    protected Transform m_rootTF = null;
    protected RectTransform m_rootRectTF = null;
    public Transform RootTransfrom { get { return m_rootRectTF.transform; } }
    protected Vector3 m_vDeactive = new Vector3(0.0f, 99999f, 0.0f);
    private emUIType m_UIType = emUIType.emUIType_Normal;
    protected bool m_bInited = false;
    protected bool m_bShow = false;
    protected bool m_bShowFromBack = false;
    public bool ShowAfterLoad = false;          //这个标志UIManager中赋值，其他地方禁止使用！！！抓到要挨揍啊！！！
    public LoadUISceneCallBack SceneLoadCallback = null;
    protected Dictionary<string, IconSprite> UsedSprite = new Dictionary<string, IconSprite>();

    private GameObject m_thisUIRootObj = null;
    private bool m_bLoadPrefab = false;
    private GameObject m_prefab = null;

    private List<UIBase> m_lstChild = null;
    private UIBase m_uiParent = null;

    public GameObject WinRoot
    {
        get { return m_thisUIRootObj; }
    }

    public bool UnloadOnClose        //关闭的时候自动卸载资源
    {
        get; protected set;
    }

    public emUIWindow WindowHide        //打开当前窗口的时候将这个窗口隐藏，当这个窗口关闭的时候，要将其打开
    {
        get; set;
    }

    public bool NeedBack { get; set; }  //该窗口是否需要返回

    protected string m_strSceneName = "";
    protected bool m_bLoadSceneFlag = false;        //是否加载场景 标记 不需要加载场景请不要赋值


    protected string SceneRelate
    {
        get { return m_strSceneName; }
        set
        {
            m_strSceneName = value;
            if (!string.IsNullOrEmpty(value))
            {
                m_bLoadSceneFlag = true;
            }
            else
            {
                m_bLoadSceneFlag = false;
            }
        }
    }

    public object ParamForShow
    {
        protected get; set;
    }

    public bool IsShowFromHide      //false 从uimanager中打开的窗口，true 走back打开的窗口主要用来在OnShow中区分
    {
        get; private set;
    }

    //false表示该窗口显示的时候关闭主城摄像机，true表示该窗口需要主城摄像机
    public bool IfDealCamera
    {
        get; private set;
    }
    private bool m_bMainCameraFlag = true;
    public bool MainCameraFlag
    {
        get { return m_bMainCameraFlag; }
        set
        {
            IfDealCamera = true;        //只有在业务层设置了MainCameraFlag，UIManager才会处理是否关闭摄像机
            m_bMainCameraFlag = value;
        }
    }

    public bool ReloadFLag      //重新登录是否需要重新加载
    {
        get; protected set;
    }

    // 构造函数
    public UIBase(emUIWindow i_eUIWindowID, emUIType windowType)
    {
        m_eUIWindowID = i_eUIWindowID;
        m_UIType = windowType;
        if (_UI_Prefab_Dic.ContainsKey(m_eUIWindowID))
        {
            m_strPrefabPath = _UI_Prefab_Dic[m_eUIWindowID];
        }
        m_bInited = false;
        Status = WinStatus.Close;
        UnloadOnClose = false;
        m_bMainCameraFlag = true;
        IfDealCamera = false;
        WindowHide = emUIWindow.emUIWindow_Invalid;
        ReloadFLag = true;
    }

    public void SetPosition(Vector3 position)
    {
        m_rootRectTF.transform.position = position;
    }

    public void SetAnchoredPosition(Vector3 position)
    {
        m_rootRectTF.anchoredPosition = position;
    }

    public void SetScale(Vector3 scale)
    {
        m_rootRectTF.transform.localScale = scale;
    }

    public void SetLocalPosition(Vector3 position)
    {
        m_rootRectTF.transform.localPosition = position;
    }

    /// <summary>
    /// 初始化一个UIPrefab 不包含加载AB过程
    /// </summary>
    /// <param name="i_UIRoot"></param>
    /// <returns></returns>
    public bool InitScene()
    {
        //如果Canvas下已经有这个节点，则不重新加载
        Transform tranUI = UIManager.UIRoot.Find(m_strPrefabPath);
        if (tranUI != null)
        {
            OnSceneLoad(tranUI.gameObject);
            return true;
        }
        return true;
    }


    private void OnSceneLoad(GameObject root)
    {
        m_prefab = root;
        if (m_prefab == null)
        {
            Develop.LogError("OnSceneLoad, ui prefab root is null !!");
            return;
        }

        Develop.LogF("UIBase OnSceneLoad, ui:{0}", m_eUIWindowID.ToString());

        m_rootObj = m_prefab;
        m_rootRectTF = m_rootObj.GetComponent<RectTransform>();
        m_rootObj.name = m_prefab.name;
        m_bLoadPrefab = true;

        m_thisUIRootObj = m_rootObj;
        m_rootTF = m_rootObj.transform;
        m_rootTF.SetParent(UIManager.UIRoot, false);

        if (m_rootObj.activeSelf)
        {
            m_rootObj.SetActive(false);
        }

        // 处理UI边框
        InitEdge();

        if (ShowAfterLoad)
        {
            ShowFromManager();
            ShowAfterLoad = false;
        }
    }


    public void Init()
    {
        try
        {
            if (m_rootObj == null)
            {
                GameObject pfb = Resources.Load<GameObject>("Prefabs/UI/" + m_strPrefabPath);
                GameObject objTarget = pfb == null ? null : GameObject.Instantiate(pfb);
                m_rootObj = objTarget;
                m_rootObj.transform.SetParent(UIManager.UIRoot, false);
                if (m_rootObj.activeSelf)
                {
                    m_rootObj.SetActive(false);
                }
                m_thisUIRootObj = m_rootObj;
                m_rootTF = m_rootObj.transform;
                m_rootRectTF = m_rootObj.GetComponent<RectTransform>();
            }
          
            OnInit(m_rootObj);
        }
        catch (System.Exception e)
        {
            Develop.LogErrorF("UI Init error. Type={0}, root={1}\nexception={2}", GetType().Name, this.m_rootObj, e);
            throw;
        }

        m_bInited = true;

        Status = WinStatus.Close;
    }

    /// <summary>
    /// 重新登录不重新加载的窗口的专有初始化UI接口，某些窗口需要特殊处理
    /// </summary>
    public virtual void NoReloadInit()
    {

    }

    // 初始化
    public virtual bool OnInit(GameObject i_UIRoot)
    {
        return true;
    }

    // 返回初始化标识
    public bool IsInited()
    {
        return m_bInited;
    }

    /// <summary>
    /// 添加一个加载完的图标，方便后面好释放
    /// </summary>
    /// <param name="icontype"></param>
    /// <param name="szSprite"></param>
    public void AddUsedSprite(emIconType icontype, string szSprite)
    {
        if (UsedSprite.ContainsKey(szSprite))
        {
            return;
        }

        IconSprite _Icon = new IconSprite();
        _Icon.IconName = szSprite;
        _Icon.IconType = icontype;
        UsedSprite[szSprite] = _Icon;
    }

    /// <summary>
    /// 释放使用了的Sprite
    /// </summary>
    public void ReleaseUsedSprite()
    {
        foreach (KeyValuePair<string, IconSprite> pair in UsedSprite)
        {
            AssetManager.UnLoadSprite(pair.Value.IconType, pair.Key);
        }

        UsedSprite.Clear();
    }


    // 处理UI边框
    private void InitEdge()
    {
        Transform tfBgWithEdge = m_rootTF.FindChild("bg_withedge");
        if (tfBgWithEdge == null)
        {
            return;
        }
        m_rootTF = tfBgWithEdge;
        m_rootObj = m_rootTF.gameObject;
    }

    public void ShowFromHide()
    {
        if (IsShow())
            return;

        IsShowFromHide = true;
        Status = WinStatus.Show;
        Show();
    }

    public void HideTemporary()
    {
        if (!IsShow())
        {
            return;
        }

        OnClose();

        HideUI();
        Status = WinStatus.Hide;
    }

    public void ShowFromManager()
    {
        if (IsShow())
        {
            Develop.LogWarningF("UIBase.ShowFromManager param, window is on show!! win:{0}", m_eUIWindowID.ToString());
        }

        IsShowFromHide = false;
        Show();
    }

    public void ShowtoFlush()
    {
        OnShow(ParamForShow);
    }

    public void Show(object baseParam)
    {
        if (IsShow())
        {
            Develop.LogWarningF("UIBase.Show param, window is on show!! win:{0}", m_eUIWindowID.ToString());
            return;
        }
        ParamForShow = baseParam;
        IsShowFromHide = false;
        Show();
    }

    private void Show()
    {
        if (!m_bInited)
        {
            Init();
        }
        /* 暂时注释 替换成 DoShow();
        //如果需要加载场景，则先加载
        if (m_bLoadSceneFlag && SceneRelate != SceneManager.Instance.CurSceneName)
        {
            SceneManager.Instance.Load(SceneRelate, DoShow, true);
        }
        else //如果不需要则直接显示
        {
            DoShow();
        }
        */
        DoShow();
    }

    private void DoShow()
    {
        ShowUI();
        //子类显示操作
        Status = WinStatus.Show;

        UnityEngine.Profiling.Profiler.BeginSample("UI DoShow-->OnShow" + m_eUIWindowID.ToString());
        GPerformance pfm = GPerformance.Start("OnShow:" + m_eUIWindowID.ToString());
        OnShow(ParamForShow);
        pfm.Stop();
        UnityEngine.Profiling.Profiler.EndSample();


        DealPOPUILayer();
    }

    // 显示UI界面 供子类重写
    public virtual void OnShow(object baseParam)
    {

    }

    //从其他窗口Back回来，也许你会有数据需要刷新
    protected virtual void OnBack()
    {
    }

    //处理弹窗的层级
    private void DealPOPUILayer()
    {
        if (m_thisUIRootObj == null)
        {
            return;
        }

        //只处理弹窗
        if (m_UIType != emUIType.emUIType_POP)
            return;

        //并且弹窗有挂canvas
        Canvas cavs = m_thisUIRootObj.GetComponent<Canvas>();
        if (cavs == null)
        {
            return;
        }

        //并且canvas是UpUI 这个id在100到899之间 一般游戏运行期间弹窗数量不会超过这个值 如果超过 要做特殊处理
        if (cavs.sortingLayerID == SortingLayer.NameToID("UpUI"))
        {
            //900以上是特殊窗口，不处理
            if (cavs.sortingOrder > 900)
            {
                return;
            }

            cavs.sortingOrder = UIManager.Instance.PopSortingLayerID++;
            ParticleSystem[] arrParticle = m_rootTF.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < arrParticle.Length; i++)
            {
                Renderer rdr = arrParticle[i].GetComponent<Renderer>();
                if (rdr != null && rdr.sortingLayerID == cavs.sortingLayerID)
                {
                    rdr.sortingOrder = cavs.sortingOrder + 1;
                }
            }
        }
    }

    //显示窗口UI
    public void ShowUI()
    {
        UnityEngine.Profiling.Profiler.BeginSample("ShowUI");
        if (!m_thisUIRootObj.activeSelf)
        {
            m_thisUIRootObj.SetActive(true);
        }
        m_thisUIRootObj.transform.SetSiblingIndex(UIManager.Instance.GetSiblindIdxOfWindow(m_eUIWindowID, m_thisUIRootObj.transform));

        if (!m_rootObj.activeSelf)
        {
            m_rootObj.SetActive(true);
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    //隐藏窗口UI
    public void HideUI()
    {
        UnityEngine.Profiling.Profiler.BeginSample("HideUI");
        if (m_thisUIRootObj.activeSelf)
        {
            m_thisUIRootObj.SetActive(false);
            /* 暂时注释
            if (m_eUIWindowID != emUIWindow.emUIWindow_Guide && GuidePart.Instance != null)
            {
                GuidePart.Instance.OnTriggerViewClose(m_thisUIRootObj.name);
            }
            */
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public virtual void Close()
    {
        if (!IsInited())
        {
            return;
        }

        //子类关闭操作
        OnClose();

        HideUI();
        Status = WinStatus.Close;

        if (m_rootObj == null)
        {
            Develop.LogF("UIBase.Close, m_rootObj is null!! win:{0}", m_eUIWindowID.ToString());
        }

        UIManager.Instance.DoClose(m_eUIWindowID);

        if (UnloadOnClose)
        {
            Unload();
        }
    }

    // 关闭UI界面 供子类重写
    public virtual void OnClose()
    {
    }

    // 返回显示标识
    public virtual bool IsShow()
    {
        return Status == WinStatus.Show;
    }

    public bool IsHide()
    {
        return Status == WinStatus.Hide;
    }

    // 卸载
    public virtual void Unload()
    {

        //卸载前通知子类
        OnUnload();

        if (m_thisUIRootObj != null)
        {
            Object.Destroy(m_thisUIRootObj);
            m_thisUIRootObj = null;
        }
        if (m_bLoadPrefab && (m_prefab != null))
        {
            Resources.UnloadUnusedAssets();
            m_bLoadPrefab = false;
            m_prefab = null;
        }
        m_rootObj = null;
        m_rootTF = null;
        m_rootRectTF = null;
        m_UIType = emUIType.emUIType_Normal;
        SceneRelate = "";
        m_bInited = false;
        Status = WinStatus.Close;
        UnloadOnClose = false;
        WindowHide = emUIWindow.emUIWindow_Invalid;
    }

    public virtual void OnUnload()
    {

    }

    // 每帧处理
    public virtual void Update()
    {
    }

    // 返回UI类型
    public virtual emUIType GetUIType()
    {
        return m_UIType;
    }

    //获取UI Window类型
    public emUIWindow GetWindowType()
    {
        return m_eUIWindowID;
    }

    // 返回UI所在场景名字
    public virtual string GetScene()
    {
        return SceneRelate;
    }

    /// <summary>
    /// 返回m_rootTF.Find(str).gameObject
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public GameObject findObjInRootTF(string str)
    {
        Transform tran = m_rootRectTF.Find(str);
        if (!tran)
        {
            Develop.LogError("findObjInRootTF 错误 rootName:" + m_rootRectTF + " prefabName:" + m_strPrefabPath + " ItemName: " + str);
            return null;
        }

        return tran.gameObject;
    }

}
