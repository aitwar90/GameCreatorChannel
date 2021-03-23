using System.Collections;
using System.Collections.Generic;

using UnityEngine;
//using GoogleMobileAds.Api;

//using System;
//public class ObsługaReklam : MonoBehaviour
//{
    // Start is called before the first frame update
    /* UNITY_ANDROID
    private RewardBasedVideoAd bazowaReklama;
    string reklamID = "";
    public RodzajReklamy rodzajReklamy;
    private BannerView bannerView;
    private InterstitialAd inter;
    private byte status = 0;
    private ushort iloscCoinówRew = 100;
    private bool zRek = false;
    public bool ZaładowanaReklamaJest
    {
        get
        {
            return zRek;
        }
    }
    */
    /* UNITY_ANDROID
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
    }    public void OtwórzReklame(byte _status, ushort iloscCoinów = 100)
    {
        status = _status;
        iloscCoinówRew = iloscCoinów;
        ObejrzyjAD();
    }
    private void ŻądanieWideo()
    {
        zRek = false;
#if UNITY_ANDROID
        AdRequest żądanie;
        switch (rodzajReklamy)
        {
            case RodzajReklamy.Baner:
                if (bannerView != null)
                    bannerView.Destroy();
                reklamID = "ca-app-pub-3940256099942544/6300978111";    //Testowy banner
                bannerView = new BannerView(reklamID, AdSize.Banner, AdPosition.Bottom);
                żądanie = new AdRequest.Builder().Build();
                this.bazowaReklama.LoadAd(żądanie, reklamID);
                break;
            case RodzajReklamy.Interstitial:
                if (inter != null)
                    inter.Destroy();
                reklamID = "ca-app-pub-3940256099942544/1033173712";    //Testowy banner
                this.inter = new InterstitialAd(reklamID);
                żądanie = new AdRequest.Builder().Build();
                this.bazowaReklama.LoadAd(żądanie, reklamID);
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

#elif UNITY_IOS
            string adUnitId = "ca-app-pub-3940256099942544/2934735716"; //Testowy
#else
            string adUnitId = "unexpected_platform";
#endif

        // Create an empty ad request.

        // Load the rewarded video ad with the request.
    }
    private void ObejrzyjAD()
    {
        if (bazowaReklama.IsLoaded())
        {
            Debug.Log("Wyświetlam reklame");
            bazowaReklama.Show();
            //PomocniczeFunkcje.UstawTimeScale(0);
        }
        else
        {
            Debug.Log("Reklama nie została załadowana");
            StartCoroutine(CzekajNaZaladowanieReklamy());
        }
    }
    private IEnumerator CzekajNaZaladowanieReklamy()
    {
        while (!bazowaReklama.IsLoaded())
        {
            Debug.Log("Reklama wciaż niezaładowana");
            yield return null;
        }
        ObejrzyjAD();

    }
    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        zRek = true;
        Debug.Log("1) Załadowano reklamę");
        MonoBehaviour.print("1) Załadowano reklamę");
    }

    public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("2) HandleRewardBasedVideoFailedToLoad odebrano wydarzenie z informajcą: "
                             + args.Message);
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
        Debug.Log("3) HandleRewardBasedVideoStarted Reklama została odpalona");
        MonoBehaviour.print("HandleRewardBasedVideoStarted Reklama została odpalona");
    }
    */
    //public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    //{
        /*
        Ta metoda jest wywoływana, gdy reklama pełnoekranowa zostanie zamknięta po dotknięciu przez użytkownika ikony zamknięcia lub kliknięciu przycisku Wstecz. 
        Jeśli Twoja aplikacja wstrzymała wyjście audio lub pętlę gry, jest to świetne miejsce, aby ją wznowić. 
        */
        //PomocniczeFunkcje.UstawTimeScale(1);
        /*UNITY_ANDROID
        Debug.Log("4) HandleRewardBasedVideoClosed Reklama została zamknięta");
        switch (status)
        {
            case 1: //Skończony poziom
                DodajNagrodęZaPoziom(iloscCoinówRew);
                status = 0;
                break;
            case 2:
                if(iloscCoinówRew >= 0 && iloscCoinówRew < 4)
                {
                   // UNITY_ANDROID PomocniczeFunkcje.managerGryScript.SkróćCzasSkrzynki((sbyte)iloscCoinówRew);
                }
                iloscCoinówRew = 100;
                status = 0;
                break;
        }
        */
        // UNITY_ANDROID this.ŻądanieWideo();    //Rozpocznij ładowanie kolejnej reklamy
        //MonoBehaviour.print("HandleRewardBasedVideoClosed Reklama została zamknięta");
    //}
    /* UNITY_ANDROID
    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        Debug.Log("5) HandleRewardBasedVideoRewarded reklama zwróciła nagrodę "
                        + amount.ToString() + " " + type);
        MonoBehaviour.print(
            "HandleRewardBasedVideoRewarded reklama zwróciła nagrodę "
                        + amount.ToString() + " " + type);
    }
    */
    //public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    //{
        /*
        Ta metoda jest wywoływana po OnAdOpened, gdy użytkownik kliknie otwarcie innej aplikacji (takiej jak sklep Google Play), uruchamiając bieżącą aplikację w tle.
        */
        //Debug.Log("6) HandleRewardBasedVideoLeftApplication reklama opuściła aplikację?");
        //MonoBehaviour.print("HandleRewardBasedVideoLeftApplication reklama opuściła aplikację?");
    //}
    /*  UNITY_ANDROID
    private void DodajNagrodęZaPoziom(ushort bazowaWartosc)
    {
        ManagerGryScript.iloscCoinów += (ushort)(bazowaWartosc * 2);
    }
`*/
//}
/* UNITY_ANDROID
public enum RodzajReklamy
{
    None = 0,
    Baner = 1,
    Interstitial = 2,
    RewardedVideo = 3,
    [HideInInspector] NativeAdvanced = 4
}
*/
