using admob;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdMobManager : MonoBehaviour {

    public static AdMobManager Instance;

    public Admob ad;

    // Use this for initialization
    void Awake () {
        Instance = this;
        initAdmob();
    }

    void initAdmob()
    {
        ad = Admob.Instance();
        ad.bannerEventHandler += onBannerEvent;
        ad.interstitialEventHandler += onInterstitialEvent;
        ad.rewardedVideoEventHandler += onRewardedVideoEvent;
        ad.nativeBannerEventHandler += onNativeBannerEvent;
        ad.initAdmob("ca-app-pub-3940256099942544/2934735716", "ca-app-pub-3940256099942544/4411468910");
        //ad.initAdmob("ca-app-pub-3064884938435258/3031529321", "ca-app-pub-3064884938435258/5984995722");
        ad.setTesting(true);

    }

    public void ShowBanner()
    {
        ad.showBannerRelative(AdSize.Banner, AdPosition.BOTTOM_CENTER, 0);
    }

    public void RemoveBanner()
    {
        ad.removeBanner();
    }

    void onInterstitialEvent(string eventName, string msg)
    {
        Debug.Log("handler onAdmobEvent---" + eventName + "   " + msg);
        if (eventName == AdmobEvent.onAdLoaded)
        {
            Admob.Instance().showInterstitial();
        }
    }

    void onBannerEvent(string eventName, string msg)
    {
        Debug.Log("handler onAdmobBannerEvent---" + eventName + "   " + msg);
    }
    void onRewardedVideoEvent(string eventName, string msg)
    {
        Debug.Log("handler onRewardedVideoEvent---" + eventName + "   " + msg);
    }
    void onNativeBannerEvent(string eventName, string msg)
    {
        Debug.Log("handler onAdmobNativeBannerEvent---" + eventName + "   " + msg);
    }
}
