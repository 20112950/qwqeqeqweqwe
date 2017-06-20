using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    static public GameManager instance;

    private void Awake()
    {
        instance = this;
        UIManager.Instance.Create();
        UIManager.Instance.Show(emUIWindow.emUIWindow_Main, true);
        FSoundManager.PreloadSound("drop");
        DontDestroyOnLoad(this);
    }
	
}
