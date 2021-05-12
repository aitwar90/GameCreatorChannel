using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using GoogleMobileAds.Api;

using System;
public class ObsługaReklam : MonoBehaviour
{
    // Start is called before the first frame update
    const string reklamID = "ca-app-pub-5582240292289857/9156662842";
    //const string AppID = "ca-app-pub-5582240292289857~8418296246";
    //const string testReklamaID = "ca-app-pub-3940256099942544/1033173712";
    //const string testAppId = "ca-app-pub-1104726610780368~6976308057";
    //const string hasło = "soidusainakra";
    private InterstitialAd inter;
    private byte status = 0;
    private ushort iloscCoinówRew = 100;
    private bool zRek = false;
    Coroutine co;
    private bool czyCoroutynaDziała = false;
    public bool ZaładowanaReklamaJest
    {
        get
        {
            return zRek;
        }
    }
    void Start()
    {
        //MobileAds.SetiOSAppPauseOnBackground(true);
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

        this.ZaładujReklamę();
        co = StartCoroutine(PrzyStarecieReklama());
        czyCoroutynaDziała = true;
    }
    public void StopCorutPrzyStarcieReklama()
    {
        if(czyCoroutynaDziała)
            StopCoroutine(co);
    }
    private IEnumerator PrzyStarecieReklama()
    {
        yield return new WaitUntil(() => zRek);
        czyCoroutynaDziała = false;
        OtwórzReklamę();
    }
    public void OtwórzReklame(byte _status, ushort iloscCoinów = 100)
    {
        status = _status;
        iloscCoinówRew = iloscCoinów;
        OtwórzReklamę();
    }
    private void ZaładujReklamę()
    {
        ResetujReklame();
        this.inter = new InterstitialAd(reklamID);

        this.inter.OnAdLoaded += HandleRewardBasedVideoLoaded;
        this.inter.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
        this.inter.OnAdOpening += HandleRewardBasedVideoOpened;
        this.inter.OnAdClosed += HandleRewardBasedVideoClosed;

        //AdRequest żądanie = new AdRequest.Builder().Build();;
        AdRequest request = new AdRequest.Builder().Build();
        this.inter.LoadAd(request);
        StartCoroutine(CzekajNaZaladowanieReklamy());
    }
    private void OtwórzReklamę()
    {
        if (zRek)
        {
            Debug.Log("Otwieram reklamę");
            zRek = false;
            PomocniczeFunkcje.UstawTimeScale(0);
            #if UNITY_ANDROID
            this.inter.Show();
            #endif
        }
        else
        {
            Debug.Log("Reklama jest niezaładowana");
        }
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
            PomocniczeFunkcje.UstawTimeScale(0);
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
        if (this.inter != null)
        {
            inter.Destroy();
        }
    }
    private IEnumerator CzekajNaZaladowanieReklamy()
    {
        yield return new WaitUntil(() => this.inter.IsLoaded());
        Debug.Log("Reklama załadowana");
        zRek = true;
        /*
        while (!this.inter.IsLoaded())
        {
            Debug.Log("Reklama wciaż niezaładowana");
            yield return null;
        }
        */
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
        PomocniczeFunkcje.UstawTimeScale((PomocniczeFunkcje.mainMenu.CzyOdpaloneMenu) ? 0 : 1);
        ZaładujReklamę();
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
