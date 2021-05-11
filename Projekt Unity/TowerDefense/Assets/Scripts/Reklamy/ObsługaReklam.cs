using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using GoogleMobileAds.Api;

using System;
public class ObsługaReklam : MonoBehaviour
{
    // Start is called before the first frame update
    string reklamID = "";
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
    void Start()
    {
        MobileAds.SetiOSAppPauseOnBackground(true);
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

        //MobileAds.Initialize(initStatus => { });
        //this.inter = RewardBasedVideoAd.Instance;

        // Called when an ad request has successfully loaded.

        this.ŻądanieWideo();
    }
    public void OtwórzReklame(byte _status, ushort iloscCoinów = 100)
    {
        status = _status;
        iloscCoinówRew = iloscCoinów;
        ObejrzyjAD();
    }
    private void ŻądanieWideo()
    {
        zRek = false;
#if UNITY_ANDROID
        ResetujReklame();
        reklamID = "ca-app-pub-5582240292289857/2771750725";    //Reklama pełno ekranowa
        this.inter = new InterstitialAd(reklamID);

        this.inter.OnAdLoaded += (sender, args) => OnAdLoadedEvent.Invoke();
        this.inter.OnAdFailedToLoad += (sender, args) => OnAdFailedToLoadEvent.Invoke();
        this.inter.OnAdOpening += (sender, args) => OnAdOpeningEvent.Invoke();
        this.inter.OnAdClosed += (sender, args) => OnAdClosedEvent.Invoke();

        //AdRequest żądanie = new AdRequest.Builder().Build();;
        Debug.Log("Ładuję reklamę");
        this.inter.LoadAd(CreateAdRequest());
        if (this.inter.IsLoaded())
            Debug.Log("Reklama została załadowana");
        else
        {
            Debug.Log("Reklama nie została załadowana");
            StartCoroutine(CzekajNaZaladowanieReklamy());
        }

#elif UNITY_IOS
            string adUnitId = "ca-app-pub-3940256099942544/2934735716"; //Testowy
#else
            string adUnitId = "unexpected_platform";
#endif

        // Create an empty ad request.

        // Load the rewarded video ad with the request.
    }
    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
            .AddKeyword("unity-admob-sample")
            .Build();
    }
    private void ObejrzyjAD()
    {
        if (this.inter.IsLoaded())
        {
            Debug.Log("Wyświetlam reklame");
            this.inter.Show();
            //PomocniczeFunkcje.UstawTimeScale(0);
        }
        else
        {
            Debug.Log("Reklama nie została załadowana");
            StartCoroutine(CzekajNaZaladowanieReklamy());
        }
    }
    public void ResetujReklame()
    {
        Debug.Log("Próbuje resetować reklamę.");
        if (this.inter != null && this.inter.IsLoaded())
        {
            inter.Destroy();
        }
    }
    private IEnumerator CzekajNaZaladowanieReklamy()
    {
        while (!this.inter.IsLoaded())
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

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        /*
        Ta metoda jest wywoływana, gdy reklama pełnoekranowa zostanie zamknięta po dotknięciu przez użytkownika ikony zamknięcia lub kliknięciu przycisku Wstecz. 
        Jeśli Twoja aplikacja wstrzymała wyjście audio lub pętlę gry, jest to świetne miejsce, aby ją wznowić. 
        */
        //PomocniczeFunkcje.UstawTimeScale(1);
        Debug.Log("4) HandleRewardBasedVideoClosed Reklama została zamknięta");
        switch (status)
        {
            case 1: //Skończony poziom
                DodajNagrodęZaPoziom(iloscCoinówRew);
                status = 0;
                break;
            case 2:
                if (iloscCoinówRew >= 0 && iloscCoinówRew < 4)
                {
                    PomocniczeFunkcje.managerGryScript.SkróćCzasSkrzynki((sbyte)iloscCoinówRew);
                }
                iloscCoinówRew = 100;
                status = 0;
                break;
        }
        ResetujReklame();
        this.ŻądanieWideo();    //Rozpocznij ładowanie kolejnej reklamy
        MonoBehaviour.print("HandleRewardBasedVideoClosed Reklama została zamknięta");
    }

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
    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
        /*
        Ta metoda jest wywoływana po OnAdOpened, gdy użytkownik kliknie otwarcie innej aplikacji (takiej jak sklep Google Play), uruchamiając bieżącą aplikację w tle.
        */
        Debug.Log("6) HandleRewardBasedVideoLeftApplication reklama opuściła aplikację?");
        MonoBehaviour.print("HandleRewardBasedVideoLeftApplication reklama opuściła aplikację?");
    }
    private void DodajNagrodęZaPoziom(ushort bazowaWartosc)
    {
        PomocniczeFunkcje.managerGryScript.ZmodyfikujIlośćCoinów((short)(bazowaWartosc * 2));
        //ManagerGryScript.iloscCoinów += (ushort)(bazowaWartosc * 2);
    }

}
public enum RodzajReklamy
{
    None = 0,
    Baner = 1,
    Interstitial = 2,
    RewardedVideo = 3,
    [HideInInspector] NativeAdvanced = 4
}
