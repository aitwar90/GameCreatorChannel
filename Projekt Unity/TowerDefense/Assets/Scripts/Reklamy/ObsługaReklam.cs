using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class ObsługaReklam : MonoBehaviour
{
    // Start is called before the first frame update
    private RewardBasedVideoAd bazowaReklama;
    void Start()
    {
        MobileAds.Initialize(initStatus => { });
        this.bazowaReklama = RewardBasedVideoAd.Instance;
        this.ŻądanieWideo();
    }

    private void ŻądanieWideo()
    {
        #if UNITY_ANDROID
            string adUnitId = "ca-app-pub-3940256099942544/5224354917";
        #elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/1712485313";
        #else
            string adUnitId = "unexpected_platform";
        #endif

        // Create an empty ad request.
        AdRequest żądanie = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        this.bazowaReklama.LoadAd(żądanie, adUnitId);
    }
    private void ObejrzyjAD()
    {
        if(bazowaReklama.IsLoaded())
        {
            bazowaReklama.Show();
        }
    }
}
