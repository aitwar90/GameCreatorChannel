﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using GoogleMobileAds.Api;

using System;
public class ObsługaReklam : MonoBehaviour
{
    // Start is called before the first frame update
    private RewardBasedVideoAd bazowaReklama;
    string reklamID = "";
    public RodzajReklamy rodzajReklamy;
    private BannerView bannerView;
    private InterstitialAd inter;
    void Start()
    {
        /*
        List<string> deviceIds = new List<string>();
        deviceIds.Add("");
        RequestConfiguration requestConfiguration = new RequestConfiguration
            .Builder()
            .SetTestDeviceIds(deviceIds)
            .build();
        MobileAds.SetRequestConfiguration(requestConfiguration);
        */
        MobileAds.Initialize(initStatus => { });
        this.bazowaReklama = RewardBasedVideoAd.Instance;

         // Called when an ad request has successfully loaded.
        bazowaReklama.OnAdLoaded += HandleRewardBasedVideoLoaded;
        // Called when an ad request failed to load.
        bazowaReklama.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        // Called when an ad is shown.
        bazowaReklama.OnAdOpening += HandleRewardBasedVideoOpened;
        // Called when the ad starts to play.
        bazowaReklama.OnAdStarted += HandleRewardBasedVideoStarted;
        // Called when the user should be rewarded for watching a video.
        bazowaReklama.OnAdRewarded += HandleRewardBasedVideoRewarded;
        // Called when the ad is closed.
        bazowaReklama.OnAdClosed += HandleRewardBasedVideoClosed;
        // Called when the ad click caused the user to leave the application.
        bazowaReklama.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

        this.ŻądanieWideo();
        ObejrzyjAD();
    }

    private void ŻądanieWideo()
    {
        
        #if UNITY_ANDROID
        AdRequest żądanie;
        switch(rodzajReklamy)
        {
            case RodzajReklamy.Baner:
            reklamID = "ca-app-pub-3940256099942544/6300978111";    //Testowy banner
            bannerView = new BannerView(reklamID, AdSize.Banner, AdPosition.Bottom);
            żądanie = new AdRequest.Builder().Build();
            bannerView.LoadAd(żądanie);
            break;
            case RodzajReklamy.Interstitial:
            reklamID = "ca-app-pub-3940256099942544/1033173712";    //Testowy banner
            this.inter = new InterstitialAd(reklamID);
            żądanie = new AdRequest.Builder().Build();
            this.inter.LoadAd(żądanie);
            break;
            case RodzajReklamy.RewardedVideo:
            reklamID = "ca-app-pub-3940256099942544/5224354917";    //Testowe wideo
            żądanie = new AdRequest.Builder().Build();
            this.bazowaReklama.LoadAd(żądanie, reklamID);
            break;
            case RodzajReklamy.NativeAdvanced:  //Native nie jest wspierane przez unity
            reklamID = "ca-app-pub-3940256099942544/2247696110";    //Testowe wideo
            break;
        }
            
        #elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/2934735716"; //Testowy
        #else
            string adUnitId = "unexpected_platform";
        #endif

        // Create an empty ad request.
        
        // Load the rewarded video ad with the request.
    }
    private void ObejrzyjAD()
    {
        if(bazowaReklama.IsLoaded())
        {
            Debug.Log("Wyświetlam reklame");
            bazowaReklama.Show();
        }
        else
        {
            Debug.Log("Reklama nie została załadowana");
            StartCoroutine(CzekajNaZaladowanieReklamy());
        }
    }
    private IEnumerator CzekajNaZaladowanieReklamy()
    {
        while(!bazowaReklama.IsLoaded())
        {
            yield return null;
        }
        ObejrzyjAD();

    }
    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("1) Załadowano reklamę");
    }

    public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print(
            "2) HandleRewardBasedVideoFailedToLoad odebrano wydarzenie z informajcą: "
                             + args.Message);
    }

    public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoOpened Otwarto reklamę");
    }

    public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoStarted Reklama została odpalona");
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        this.ŻądanieWideo();
        MonoBehaviour.print("HandleRewardBasedVideoClosed Reklama została zamknięta");
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        MonoBehaviour.print(
            "HandleRewardBasedVideoRewarded reklama zwróciła nagrodę "
                        + amount.ToString() + " " + type);
    }
    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLeftApplication reklama opuściła aplikację?");
    }

}
public enum RodzajReklamy
{
    None = 0,
    Baner = 1,
    Interstitial = 2,
    RewardedVideo = 3,
    NativeAdvanced = 4
}
