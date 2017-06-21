using System.Collections;
using System.Collections.Generic;
using Umeng;
using UnityEngine;

public class UMManager : MonoBehaviour {

    /* 在info post 增加如下字段 ,这个文件应该在unity生成的ios项目中
     * < key>NSAppTransportSecurity< /key>
    < dict>
        < key>NSAllowsArbitraryLoads< /key>
        < ture />
    < /dict>
     * */
    void Start()
    {

        //请到 http://www.umeng.com/analytics 获取app key 
        GA.StartWithAppKeyAndChannelId("your app key", "App Store");
        
        //触发统计事件 开始关卡
        GA.StartLevel("your level ID");


    }
}
