using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
public class ManagerGryScript : MonoBehaviour, ICzekajAz
{
    [Header("Podstawowe informacje dla gracza")]
    #region Zmienne publiczne
    [Tooltip("Aktualna ilość monet")]
    public static ushort iloscCoinów = 2000;
    public static byte bonusDoObrażeń = 0;
    [Tooltip("Aktualna epoka w której gra gracz")]
    public Epoki aktualnaEpoka;
    public byte aktualnyPoziomEpoki = 1;

    [Header("Informacje o graczu")]
    [Tooltip("Czas pomięczy kolejnymi falami hordy")]
    public float czasMiędzyFalami = 1;
    public static short iloscAktywnychWrogów = 0;

    [Tooltip("Zaznaczony NPC")]
    public NPCClass zaznaczonyObiekt = null;
    public delegate void WywołajResetujŚcieżki(KonkretnyNPCStatyczny knpcs = null);
    public WywołajResetujŚcieżki wywołajResetŚcieżek;
    public GameObject[] bazy = new GameObject[1];
    [Tooltip("Tablica nagród jakie gracz może otrzymać")]
    public PrzedmiotScript[] ekwipunekGracza = null;
    [Tooltip("Nagrody jakie gracz poisada")]
    public Skrzynka[] skrzynki;
    [Tooltip("Asset o rozszerzeniu csv z tłumaczeniem")]
    public TextAsset plikJezykowy;
    [Tooltip("Wszystkie dostępne jednostki w grze, które mogą mieć nastawienie wrogie")]
    public NPCClass[] wszystkieRodzajeWrogichJednostek;
    [HideInInspector] public ushort hpIdx;
    [HideInInspector] public ushort atkIdx;
    [HideInInspector] public ushort defIdx;
    [Tooltip("Ile ma kosztować rozwój w akademii")]
    public ushort kosztRozwojuAkademii = 300;
    public static bool odpalamNaUnityRemote = false;
    #endregion
    #region Particles
    [Tooltip("Particle dla konkretnych etapów gry na Canvasie: \n [0] - Gracz wygrał")]
    public GameObject[] particleSystems;
    [Tooltip("Tablica fontów dla konkretnych jezyków")]
    public FontDlaJezyków[] fontyJezyków;
    #endregion
    #region Prywatne zmienne
    KonkretnyNPCStatyczny knpcsBazy = null;
    private byte idxOfManagerGryScript = 0;
    private bool czyScenaZostałaZaładowana = false;
    //public bool toNieOstatniaFala = false; UNITY_ANDROID
    // UNITY_ANDROID private ObsługaReklam or;
    private float timerFal;
    //private short valFPS = 0; UNITY_ANDROID
    //private byte aktualnyIndexTabFPS = 0; UNITY_ANDROID
    private bool poziomZakonczony = false;
    private byte bufferTimerFal = 255;
    private int[] wartościDlaStatystyk = { 0, 0, 0, 0, 0, 0 };
    private List<Stack<ParticleSystem>> particleStack;
    #endregion
    #region Getery i Setery
    public Skrzynka ZwróćSkrzynkeOIndeksie(byte idx)
    {
        return skrzynki[idx];
    }
    public bool CzyScenaZostałaZaładowana
    {
        get
        {
            return czyScenaZostałaZaładowana;
        }
        set
        {
            czyScenaZostałaZaładowana = value;
        }
    }
    /* UNITY_ANDROID
    public bool CzyReklamaZaładowana
    {
        get
        {
            return or.ZaładowanaReklamaJest;
        }
    }
    */
    public ref NPCClass[] PobierzTabliceWrogow
    {
        get
        {
            return ref wszystkieRodzajeWrogichJednostek;
        }
    }
    public bool blokowanieOrientacji = true;
    #endregion  
    #region Metody UNITY
    void Awake()
    {
        #if !UNITY_EDITOR
        PlayerPrefsSwitch.PlayerPrefsSwitch.Init();
        #endif
        PomocniczeFunkcje.managerGryScript = this;
        PomocniczeFunkcje.spawnBudynki = FindObjectOfType(typeof(SpawnBudynki)) as SpawnBudynki;
        PomocniczeFunkcje.mainMenu = FindObjectOfType(typeof(MainMenu)) as MainMenu;
        PomocniczeFunkcje.eSystem = FindObjectOfType(typeof(UnityEngine.EventSystems.EventSystem)) as UnityEngine.EventSystems.EventSystem;
        PomocniczeFunkcje.muzyka = this.GetComponent<MuzykaScript>();
        // UNITY_ANDROID or = FindObjectOfType(typeof(ObsługaReklam)) as ObsługaReklam;
        SpawnerHord.actualHPBars = 0;
    }
    void Start()
    {
        if (blokowanieOrientacji)
        {
            Screen.orientation = ScreenOrientation.Landscape;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToPortrait = false;
        }
        skrzynki = new Skrzynka[PomocniczeFunkcje.mainMenu.buttonSkrzynki.Length];
        for (byte i = 0; i < skrzynki.Length; i++)
        {
            skrzynki[i] = new Skrzynka(ref PomocniczeFunkcje.mainMenu.buttonSkrzynki[i]);
        }
        StartCoroutine(CzekajAz());
        //MetodaDoOdpaleniaPoWyczekaniu();
    }
    void Update()
    {
        if (!PomocniczeFunkcje.mainMenu.CzyOdpaloneMenu)
        {
            /*
            Fragment kodu, który ma za zadanie zaznaczyć obiekt
            */
            /*#if UNITY_STANDALONE
                    if (Input.GetMouseButtonDown(0))
                    {
                        zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt);
                        if(zaznaczonyObiekt != null && zaznaczonyObiekt.AktualneŻycie > 0 && !PomocniczeFunkcje.CzyKliknalemUI())
                        {
                             zaznaczonyObiekt.UstawPanel(Input.mousePosition);
                             if (zaznaczonyObiekt.szybkośćAtaku != -1)    //Nie jest to akademia
                                {
                                    PomocniczeFunkcje.mainMenu.OdpalButtonyAkademii(false);
                                }
                        }
                        else if(PomocniczeFunkcje.mainMenu.OdpalonyPanel && !PomocniczeFunkcje.CzyKliknalemUI())
                       {
                            PomocniczeFunkcje.mainMenu.UstawPanelUI("", Vector2.zero);
                            PomocniczeFunkcje.mainMenu.OdpalButtonyAkademii(false);
                        }
                    }
            //#endif
            */
            //#if UNITY_ANDROID || UNITY_IOS
            if (Input.mousePresent)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("RightStickKlik"))
                {
                    if (PomocniczeFunkcje.spawnBudynki.aktualnyObiekt == null)
                    {
                        bool czyZjoy = false;
                        if (Input.GetButtonDown("RightStickKlik"))
                            czyZjoy = true;
                        zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt, czyZjoy);
                        if (zaznaczonyObiekt != null && zaznaczonyObiekt.AktualneŻycie > 0 && !PomocniczeFunkcje.CzyKliknalemUI())
                        {
                            zaznaczonyObiekt.UstawPanel((czyZjoy) ? new Vector2(PomocniczeFunkcje.mainMenu.ZwrócPozycjeKursora[0], PomocniczeFunkcje.mainMenu.ZwrócPozycjeKursora[1]) : (Vector2)Input.mousePosition);
                            if (zaznaczonyObiekt.szybkośćAtaku != -1)    //Nie jest to akademia
                            {
                                PomocniczeFunkcje.mainMenu.OdpalButtonyAkademii(false);
                            }
                        }
                        else if (PomocniczeFunkcje.mainMenu.OdpalonyPanel && !PomocniczeFunkcje.CzyKliknalemUI())
                        {
                            PomocniczeFunkcje.mainMenu.UstawPanelUI("", Vector2.zero);
                            PomocniczeFunkcje.mainMenu.OdpalButtonyAkademii(false);
                        }
                    }
                }
            }
            else
            {
                if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetButtonDown("RightStickKlik"))
                {
                    if (PomocniczeFunkcje.spawnBudynki.aktualnyObiekt == null)
                    {
                        bool czyZjoy = false;
                        if (Input.GetButtonDown("RightStickKlik"))
                            czyZjoy = true;
                        else
                        {
                            PomocniczeFunkcje.mainMenu.UstawKursorNa(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
                        }
                        zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt, czyZjoy);
                        if (zaznaczonyObiekt != null && zaznaczonyObiekt.AktualneŻycie > 0 && !PomocniczeFunkcje.CzyKliknalemUI())
                        {
                            zaznaczonyObiekt.UstawPanel((czyZjoy) ? new Vector2(PomocniczeFunkcje.mainMenu.ZwrócPozycjeKursora[0], PomocniczeFunkcje.mainMenu.ZwrócPozycjeKursora[1]) : (Vector2)Input.GetTouch(0).position);
                            if (zaznaczonyObiekt.szybkośćAtaku != -1)    //Nie jest to akademia
                            {
                                PomocniczeFunkcje.mainMenu.OdpalButtonyAkademii(false);
                            }
                        }
                        else if (PomocniczeFunkcje.mainMenu.OdpalonyPanel && !PomocniczeFunkcje.CzyKliknalemUI())
                        {
                            PomocniczeFunkcje.mainMenu.UstawPanelUI("", Vector2.zero);
                            PomocniczeFunkcje.mainMenu.OdpalButtonyAkademii(false);
                        }
                    }
                }
            }
        }
        /* UNITY_ANDROID
        if (Input.GetKeyDown(KeyCode.Space))
        {
            sbyte t = 120;
            for (byte i = 0; i < skrzynki.Length; i++)
            {
                if (skrzynki[i].ReuseTimer && !skrzynki[i].button.interactable)
                {
                    skrzynki[i].OdejmnijCzas();
                    break;
                }
                else    //
                {
                    if (i < t && !skrzynki[i].button.interactable)
                    {
                        t = (sbyte)i;
                    }
                    if (t != 120 && i == (skrzynki.Length - 1))
                    {
                        skrzynki[t].RozpocznijOdliczanie();
                    }
                }
            }
        }
        */
        //#endif
        switch (idxOfManagerGryScript)  //Każdy idxOfManagerGryScript podzielny przez 5 bez reszty obsługuje timerFal
        {
            case 0:
                if (!czyScenaZostałaZaładowana)
                {
                    SprawdźCzyScenaZostałaZaładowana();
                    idxOfManagerGryScript = 5;
                }
                else
                {
                    idxOfManagerGryScript++;
                }
                break;
            /*  UNITY_ANDROID
        case 1:
            if (PomocniczeFunkcje.mainMenu.CzyLFPSOn)
            {
                if (aktualnyIndexTabFPS < 30)
                {
                    valFPS += (short)(1f / Time.unscaledDeltaTime);
                    aktualnyIndexTabFPS++;
                }
                else
                {
                    valFPS = (short)(valFPS / 30f);
                    PomocniczeFunkcje.mainMenu.UstawWartoscFPS(valFPS);
                    valFPS = 0;
                    aktualnyIndexTabFPS = 0;
                }
            }
            idxOfManagerGryScript++;
            break;
            */
            case 2:
                if (this.czyScenaZostałaZaładowana)
                {
                    bool czyLFala = PomocniczeFunkcje.spawnerHord.CzyOstatniaFala();
                    if (aktualnyPoziomEpoki == 255 && !poziomZakonczony && czyLFala && iloscAktywnychWrogów <= 0)
                    {
                        if (ManagerSamouczekScript.mssInstance.CzyZgadzaSięIDXGłówny(16))
                        {
                            ManagerSamouczekScript.mssInstance.ZmiennaPomocnicza = -100;
                        }
                    }
                    else if (!poziomZakonczony && czyLFala && iloscAktywnychWrogów <= 0)
                    {
                        //Lvl skończony wszystkie fale zostały pokonane
                        KoniecPoziomuZakończony(true);
                    }
                    else if (knpcsBazy.AktualneŻycie <= 0)
                    {
                        KoniecPoziomuZakończony(false);
                    }
                    else if (!czyLFala)
                    {
                        if (iloscAktywnychWrogów == 0 && aktualnyPoziomEpoki != 255)
                        {
                            ObslTimerFal();
                        }
                        ObslMryganie();
                    }
                }
                //UNITY_ANDROID idxOfManagerGryScript++;
                idxOfManagerGryScript = 0;
                break;
            /* UNITY_ANDROID
        case 5:
            if (MainMenu.singelton.CzyOdpalonyPanelReklam)
            {
                for (byte i = 0; i < 4; i++)
                {
                    skrzynki[i].SprawdźCzyReuseMinęło();
                }
            }
            idxOfManagerGryScript = 0;
            break;
            */
            default:
                idxOfManagerGryScript++;
                break;

        }
    }
    void LateUpdate()
    {
        if (PomocniczeFunkcje.poHerbacie > -1)
            PomocniczeFunkcje.ResetujDaneRaycast();
        if (PomocniczeFunkcje.czyKliknąłemUI > -1)
        {
            PomocniczeFunkcje.czyKliknąłemUI = -1;
        }
        if (PomocniczeFunkcje.kameraZostalaPrzesunieta > 0)
        {
            switch (PomocniczeFunkcje.kameraZostalaPrzesunieta)
            {
                case 1:
                    PomocniczeFunkcje.kameraZostalaPrzesunieta--;
                    break;
                case 2:
                    PomocniczeFunkcje.kameraZostalaPrzesunieta--;
                    break;
            }
        }
    }
    #endregion
    #region Metody podczas ładowania i kasowania sceny
    private void ŁadowanieDanych()
    {
        if (iloscCoinów < 50)
            iloscCoinów = 50;
        PomocniczeFunkcje.spawnerHord = FindObjectOfType(typeof(SpawnerHord)) as SpawnerHord;
        PomocniczeFunkcje.spawnerHord.UstawHorde(aktualnaEpoka, aktualnyPoziomEpoki);
        PomocniczeFunkcje.kameraZostalaPrzesunieta = 2;
        Terrain terr = FindObjectOfType(typeof(Terrain)) as Terrain;
        PomocniczeFunkcje.tablicaWież = new List<InformacjeDlaPolWież>[22, 22];
        PomocniczeFunkcje.aktualneGranicaTab = (ushort)((terr.terrainData.size.x - 56) / 2.0f);
        PomocniczeFunkcje.distXZ = (terr.terrainData.size.x - (PomocniczeFunkcje.aktualneGranicaTab * 2)) / PomocniczeFunkcje.tablicaWież.GetLength(0);
        PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
        PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćFal", SpawnerHord.actFala.ToString() + "/" + SpawnerHord.iloscFalNaKoncu.ToString());
        PomocniczeFunkcje.spawnBudynki.InicjacjaPaneluBudynków();
        PomocniczeFunkcje.mainMenu.WygenerujIPosortujTablice(); //Generuje i sortuje tablice budynków do wybudowania
        PomocniczeFunkcje.mainMenu.PrzesuńBudynki(0, true);
        PomocniczeFunkcje.mainMenu.ostatniStawianyBudynekButton.GetComponent<ObsłużPrzyciskOstatniegoStawianegoBudynku>().RestartPrzycisku();
        if (aktualnyPoziomEpoki < 255)
        {
            PomocniczeFunkcje.mainMenu.WłączWyłączPanel(new string[] { "ui_down", "UI_LicznikCzasu" }, true);
        }
        else
        {
            PomocniczeFunkcje.mainMenu.WłączWyłączPanel("ui_down", true);
        }
        for (byte i = 0; i < wartościDlaStatystyk.Length; i++)
        {
            wartościDlaStatystyk[i] = 0;
        }
        ObslTimerFal(0);
        PomocniczeFunkcje.mainMenu.OdpalKursor = true;
        /*
        Transform go = new GameObject("R").transform;
         for(byte i = 0; i < 22; i++)
        {
            for(byte j = 0; j < 22; j++)
            {
                GameObject go1 = new GameObject("X = "+i+" Z = "+j);
                go1.transform.position = new Vector3(i*PomocniczeFunkcje.distXZ + PomocniczeFunkcje.aktualneGranicaTab, 1.0f, j*PomocniczeFunkcje.distXZ + PomocniczeFunkcje.aktualneGranicaTab);

                go1.AddComponent<TestOnEnableGround>();
                TestOnEnableGround toeg = go.GetComponent<TestOnEnableGround>();
                toeg.X = i;
                toeg.Z = j;

                go1.transform.SetParent(go);
            }
        }
        */
    }
    public void GenerujBaze()
    {
        if (aktualnyPoziomEpoki > PomocniczeFunkcje.odblokowanyPoziomEpoki && aktualnyPoziomEpoki != 255)
        {
            Debug.Log("Poziom epoki nie został odblokowany");
            return;
        }
        else if ((byte)aktualnaEpoka > PomocniczeFunkcje.odblokowaneEpoki && aktualnyPoziomEpoki != 255)
        {
            Debug.Log("Epoka nie została odblokowana");
            return;
        }
        sbyte idxEpokiBazyWTablicy = (sbyte)((sbyte)aktualnaEpoka - 1);
        if (idxEpokiBazyWTablicy < 0 || idxEpokiBazyWTablicy >= bazy.Length)
        {
            Debug.Log("idxEpokaBazyWTablicy = " + idxEpokiBazyWTablicy);
            return;
        }
        else
        {
            ŁadowanieDanych();
            GameObject baza = GameObject.Instantiate(bazy[idxEpokiBazyWTablicy], new Vector3(MoveCameraScript.bazowePolozenieKameryGry.x, 0.0f, MoveCameraScript.bazowePolozenieKameryGry.z - 5f), Quaternion.identity);
            knpcsBazy = baza.GetComponent<KonkretnyNPCStatyczny>();
            if (knpcsBazy == null)
                Debug.Log("Nie załadowałem KNPCS");
            PomocniczeFunkcje.celWrogów = knpcsBazy;
            knpcsBazy.InicjacjaBudynku();
            PomocniczeFunkcje.DodajDoDrzewaPozycji(knpcsBazy, ref PomocniczeFunkcje.korzeńDrzewaPozycji);
            baza.transform.SetParent(PomocniczeFunkcje.spawnBudynki.RodzicBudynków);
        }
    }
    private void SprawdźCzyScenaZostałaZaładowana()
    {
        if (ObslugaScenScript.indeksAktualnejSceny < 0)
            return;
        Scene s = SceneManager.GetSceneByBuildIndex(ObslugaScenScript.indeksAktualnejSceny);
        if (s.isLoaded)
        {
            czyScenaZostałaZaładowana = true;
            GenerujBaze();
        }
    }
    public void ResetManagerGryScript()
    {
        czyScenaZostałaZaładowana = false;
        poziomZakonczony = false;
        idxOfManagerGryScript = 0;
        iloscAktywnychWrogów = 0;
        bonusDoObrażeń = 0;
        wartościDlaStatystyk = new int[] { 0, 0, 0, 0, 0, 0 };

    }
    #endregion
    #region Metody do obsługi UI
    private void ObslMryganie()
    {
        if (czyScenaZostałaZaładowana && PomocniczeFunkcje.celWrogów != null)
        {
            NPCClass nc = PomocniczeFunkcje.celWrogów;
            float pŻycie = nc.AktualneŻycie / (float)nc.maksymalneŻycie;
            if (pŻycie <= 0.5f)
            {
                ekwipunekGracza[1].Mrygaj();
            }
        }
    }
    public void ObslTimerFal(float setTimer = -10000)
    {
        if (setTimer == -10000)
        {
            if (timerFal < czasMiędzyFalami)
            {
                timerFal += Time.deltaTime * 3.0f;
            }
            else
            {
                timerFal = 0;
                if (aktualnyPoziomEpoki == 255 && iloscAktywnychWrogów > 0)
                    return;
                PomocniczeFunkcje.spawnerHord.GenerujSpawn(aktualnaEpoka);
                if (czasMiędzyFalami > 60)
                    czasMiędzyFalami = 60f;
                MuzykaScript.singleton.WłączWyłączClip(true, "Bitwa");
                PomocniczeFunkcje.mainMenu.WłączWyłączPanel("UI_LicznikCzasu", false);
                UstawTenDomyslnyButton.UstawDomyślnyButton(9, false);
                PomocniczeFunkcje.mainMenu.OdpalKursor = true;
            }
        }
        else
        {
            MuzykaScript.singleton.WłączWyłączClip(true, "AmbientWGrze_" + aktualnaEpoka.ToString(), false);
            timerFal = setTimer;
        }
        byte czas = (byte)(czasMiędzyFalami - timerFal);
        if (czas != bufferTimerFal)
        {
            bufferTimerFal = czas;
            PomocniczeFunkcje.mainMenu.UstawTextUI("timer", czas.ToString());
        }
    }
    ///<summary>Metoda wywoływana po tym jak fala zostanie zniszczona i czekamy na wywołanie kolejnej.</summary>
    public void RozgrywkaPoWalkaPrzełącz()
    {
        PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", iloscCoinów.ToString());
        PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćFal", SpawnerHord.actFala.ToString() + "/" + SpawnerHord.iloscFalNaKoncu.ToString());
        PomocniczeFunkcje.mainMenu.WłączWyłączPanel("ui_down", true);
        PomocniczeFunkcje.mainMenu.WłączWyłączPanel("UI_LicznikCzasu", true);
        UstawTenDomyslnyButton.UstawDomyślnyButton((aktualnyPoziomEpoki < 255) ? (sbyte)7 : (sbyte)10, false);
        MuzykaScript.singleton.WłączWyłączClip(true, "AmbientWGrze_" + PomocniczeFunkcje.managerGryScript.aktualnaEpoka.ToString(), false);
        SpawnerHord.actualHPBars = 0;
        PomocniczeFunkcje.mainMenu.OdpalKursor = true;
    }
    public void ZmianaJęzyka(byte idx)
    {
        if (plikJezykowy != null)
        {
            UnityEngine.UI.Text[] wszystkieFrazy = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text[];
            string fs = plikJezykowy.text;
            fs = fs.Replace("\n", "");
            fs = fs.Replace("\r", "");
            string[] fLines = fs.Split(';');
            idx++;
            Font f = null;
            if (fontyJezyków != null)
            {
                bool znalazlem = false;
                for (byte i = 0; i < fontyJezyków.Length; i++)
                {
                    for (byte j = 0; j < fontyJezyków[i].idxJezyka.Length; j++)
                    {
                        if (fontyJezyków[i].idxJezyka[j] == idx)
                        {
                            f = fontyJezyków[i].font;
                            znalazlem = true;
                            break;
                        }
                    }
                    if (znalazlem)
                        break;
                }
            }
            if (f != null)
            {
                PomocniczeFunkcje.PrzypiszFontyDoNiemajacychPrzypisanychTextow(ref f);
            }
            for (ushort i = 0; i < fLines.Length; i++)
            {
                string[] pFrazy = fLines[i].Split('|');
                if (idx >= pFrazy.Length)
                {
                    continue;
                }
                if (pFrazy[idx] != "")
                {
                    bool znalazlem = false;
                    for (ushort j = 0; j < wszystkieFrazy.Length; j++)
                    {
                        if (wszystkieFrazy[j].transform.name != "Text")
                        {
                            if (pFrazy[0] == wszystkieFrazy[j].transform.name)
                            {
                                wszystkieFrazy[j].text = pFrazy[idx];
                                if (f != null)
                                    wszystkieFrazy[j].font = f;
                                znalazlem = true;
                            }
                        }
                        else
                        {
                            if (pFrazy[0] == wszystkieFrazy[j].transform.parent.name)
                            {
                                wszystkieFrazy[j].text = pFrazy[idx];
                                if (f != null)
                                    wszystkieFrazy[j].font = f;
                                znalazlem = true;
                            }
                        }
                        if (znalazlem && pFrazy[0] == "Akademia=nazwa")
                        {
                            znalazlem = false;
                            break;
                        }
                    }
                    if (PomocniczeFunkcje.spawnBudynki != null)
                    {
                        KonkretnyNPCStatyczny[] knpcsT = null;
                        if (PomocniczeFunkcje.spawnBudynki.RodzicBudynków != null)
                            knpcsT = PomocniczeFunkcje.spawnBudynki.RodzicBudynków.GetComponentsInChildren<KonkretnyNPCStatyczny>();
                        byte fD = 0;
                        for (ushort j = 0; j < PomocniczeFunkcje.spawnBudynki.wszystkieBudynki.Length; j++)
                        {
                            ushort kk = 10000;
                            if (pFrazy[0] == PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[j].name + "=nazwa")
                            {
                                fD++;
                                KonkretnyNPCStatyczny knpcs = PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[j].GetComponent<KonkretnyNPCStatyczny>();
                                if (knpcsT != null)
                                {
                                    if (kk == 10000)
                                    {
                                        for (ushort k = 0; k < knpcsT.Length; k++)
                                        {
                                            if (knpcs.nazwa == knpcsT[k].nazwa)
                                            {
                                                knpcsT[k].nazwa = pFrazy[idx];
                                                kk = k;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        knpcsT[kk].UstawJezykNPC("nazwa", pFrazy[idx]);
                                    }
                                }
                                knpcs.UstawJezykNPC("nazwa", pFrazy[idx]);
                            }
                            if (pFrazy[0] == PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[j].name + "=opis")
                            {
                                fD++;
                                KonkretnyNPCStatyczny knpcs = PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[j].GetComponent<KonkretnyNPCStatyczny>();
                                if (knpcsT != null)
                                {
                                    if (kk == 10000)
                                    {
                                        for (ushort k = 0; k < knpcsT.Length; k++)
                                        {
                                            if (knpcs.opisBudynku == knpcsT[k].opisBudynku)
                                            {
                                                knpcsT[k].opisBudynku = pFrazy[idx];
                                                kk = k;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        knpcsT[kk].UstawJezykNPC("opis", pFrazy[idx]);
                                    }
                                }
                                knpcs.UstawJezykNPC("opis", pFrazy[idx]);
                            }
                            if (fD == 2)
                            {
                                break;
                            }
                        }
                    }
                    if (wszystkieRodzajeWrogichJednostek != null && wszystkieRodzajeWrogichJednostek.Length > 0)
                    {
                        for (ushort j = 0; j < wszystkieRodzajeWrogichJednostek.Length; j++)
                        {
                            if (pFrazy[0] == wszystkieRodzajeWrogichJednostek[j].name + "=nazwa")
                            {
                                wszystkieRodzajeWrogichJednostek[j].UstawJezykNPC("nazwa", pFrazy[idx]);
                            }
                        }
                    }
                    if (PomocniczeFunkcje.managerGryScript != null || pFrazy[idx] != "")
                    {
                        for (ushort j = 0; j < PomocniczeFunkcje.managerGryScript.ekwipunekGracza.Length; j++)
                        {
                            if (PomocniczeFunkcje.managerGryScript.ekwipunekGracza[j].name == pFrazy[0])
                            {
                                PomocniczeFunkcje.managerGryScript.ekwipunekGracza[j].nazwaPrzedmiotu = pFrazy[idx];
                            }
                        }
                        PomocniczeFunkcje.mainMenu.UstawDropDownEkwipunku(ref ekwipunekGracza);
                    }
                }
            }
            for (byte i = 0; i < 4; i++)
            {
                this.ekwipunekGracza[i].AktualizujNazwe();
            }
            ManagerSamouczekScript.mssInstance.ZaladujText();
        }
    }
    #endregion
    #region Nagrody, skrzynki i reklamy
    public void CudOcalenia()
    {
        //PomocniczeFunkcje.celWrogów.AktualneŻycie = (PomocniczeFunkcje.celWrogów.AktualneŻycie < PomocniczeFunkcje.celWrogów.maksymalneŻycie / 2) ? (short)(PomocniczeFunkcje.celWrogów.maksymalneŻycie / 2.0f) : PomocniczeFunkcje.celWrogów.maksymalneŻycie;
        KonkretnyNPCDynamiczny[] knpcd = FindObjectsOfType(typeof(KonkretnyNPCDynamiczny)) as KonkretnyNPCDynamiczny[];
        for (ushort i = 0; i < knpcd.Length; i++)
        {
            knpcd[i].AktualneŻycie = 0;
            knpcd[i].NieŻyję = true;
        }
        PomocniczeFunkcje.korzeńDrzewaPozycji.ExecuteAll(0);
        RozgrywkaPoWalkaPrzełącz();
        /*
        KonkretnyNPCStatyczny[] knpcs = FindObjectsOfType(typeof(KonkretnyNPCStatyczny)) as KonkretnyNPCStatyczny[];
        for (ushort i = 0; i < knpcs.Length; i++)
        {
            if (knpcs[i].AktualneŻycie > 0)
                knpcs[i].AktualneŻycie = knpcs[i].maksymalneŻycie;
        }
        PomocniczeFunkcje.mainMenu.UstawPrzyciskObrotu(false);
        PomocniczeFunkcje.mainMenu.UstawHPGłównegoPaska(1.0f);
        */
        //Fragment wyłączający courutyny
        //StopAllCoroutines();
        ObslTimerFal(0);
    }
    public void KliknietyPrzycisk() //Kliknięty przycisk potwierdzający użycie skrzynki
    {
        byte losowy = ekwipunekGracza[0].DodajNagrode();
        PomocniczeFunkcje.mainMenu.UstawButtonNagrody(losowy, ekwipunekGracza[losowy].ilośćDanejNagrody);
    }
    /* UNITY_ANDROID
    public void SkróćCzasSkrzynki(sbyte idxS = -1)
    {
        if (idxS == -1)
        {
            for (byte i = 0; i < skrzynki.Length; i++)
            {
                if (skrzynki[i].ReuseTimer && !skrzynki[i].button.interactable)
                {
                    skrzynki[i].OdejmnijCzas();
                    break;
                }
            }
        }
        else if (idxS > -1)
        {
            if (skrzynki[idxS].ReuseTimer && !skrzynki[idxS].button.interactable)
            {
                skrzynki[idxS].OdejmnijCzas();
            }
        }
    }
    */
    private void OdblokujKolejnaSkrzynke()
    {
        for (byte i = 0; i < skrzynki.Length; i++)
        {
            if (/* UNITY_ANDROID !skrzynki[i].ReuseTimer && */ !skrzynki[i].button.interactable)
            {
                /* UNITY_ANDROID
                skrzynki[i].RozpocznijOdliczanie();
                */
                skrzynki[i].button.interactable = true;
                break;
            }
        }
    }
    public void KliknietyButtonZwiekszeniaNagrodyPoLvlu()
    {
        ushort c = (ushort)(((byte)aktualnaEpoka) * aktualnyPoziomEpoki * 15);
        DodajDoWartościStatystyk(5, c);
        PomocniczeFunkcje.mainMenu.UstawDaneStatystyk(ref wartościDlaStatystyk);
        // UNITY_ANDROID or.OtwórzReklame(1, c);
        PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
    }
    public void UzyciePrzedmiotu(byte idxOfItem)
    {
        if (ekwipunekGracza[idxOfItem].ilośćDanejNagrody > 0)
        {
            ekwipunekGracza[idxOfItem].AktywujPrzedmiot();
            PomocniczeFunkcje.mainMenu.UstawButtonNagrody(idxOfItem, ekwipunekGracza[idxOfItem].ilośćDanejNagrody);
        }
    }
    /* UNITY_ANDROID
    public void KlikniętaReklamaButtonSkrzynki(byte idx)
    {
        or.OtwórzReklame(2, idx);
    }
    */
    #endregion
    #region Koniec poziomu
    public void KoniecPoziomuZakończony(bool sukces = true)
    {
        if (aktualnyPoziomEpoki == 255)  //Ukończenie poziomu samouczka
        {
            ManagerSamouczekScript.mssInstance.OpuśćSamouczek();
            aktualnyPoziomEpoki = 1;
            poziomZakonczony = true;
            iloscAktywnychWrogów = 0;
            return;
        }
        if (sukces)
        {
            if (particleSystems != null && particleSystems.Length > 0)
            {
                particleSystems[0].SetActive(true);
            }
            if (aktualnyPoziomEpoki >= PomocniczeFunkcje.odblokowanyPoziomEpoki)
            {
                if (aktualnyPoziomEpoki % 100 == 0 && (byte)aktualnaEpoka == PomocniczeFunkcje.odblokowaneEpoki)
                {
                    //Jeśli epoki są gotowe to tu są odblokowywane
                    //PomocniczeFunkcje.odblokowaneEpoki++;
                }
                else    //Ten else do usunięcia jesli zostanie dodanych więcej epok
                {
                    PomocniczeFunkcje.odblokowanyPoziomEpoki++;
                }
            }
            //  Obsługa Particle system
            PomocniczeFunkcje.mainMenu.nastepnyPoziom.interactable = true;
            PomocniczeFunkcje.mainMenu.UstawPanelUI("", Vector2.zero);
            UstawTenDomyslnyButton.ZaktualizujStan(8);
            UstawTenDomyslnyButton.UstawAktywnyButton(PomocniczeFunkcje.mainMenu.nastepnyPoziom.gameObject);
            // UNITY_ANDROID PomocniczeFunkcje.mainMenu.rekZaWyzszaNagrode.gameObject.SetActive(CzyReklamaZaładowana);
            OdblokujKolejnaSkrzynke();
            PomocniczeFunkcje.ZapiszDane();
            ushort wartośćCoinówWygrana = (ushort)((((byte)aktualnaEpoka) * aktualnyPoziomEpoki * 15) * 2);
            // UNITY_ANDROID ushort wartośćCoinówWygrana = (ushort)(((byte)aktualnaEpoka) * aktualnyPoziomEpoki * 15);
            iloscCoinów += wartośćCoinówWygrana;
            DodajDoWartościStatystyk(5, wartośćCoinówWygrana);
            PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
            PomocniczeFunkcje.mainMenu.WłączWyłączPanel("WinTXT", true);
            PomocniczeFunkcje.mainMenu.UstawDaneStatystyk(ref wartościDlaStatystyk);
            MuzykaScript.singleton.WłączWyłączClip(true, "Zwycięstwo");
        }
        else
        {
            PomocniczeFunkcje.mainMenu.WłączWyłączPanel("LoseTXT", true);
            MuzykaScript.singleton.WłączWyłączClip(true, "Przegrana");
            UstawTenDomyslnyButton.UstawDomyślnyButton(8);
        }
        PomocniczeFunkcje.mainMenu.UstawPrzyciskObrotu(false);
        PomocniczeFunkcje.mainMenu.WłączWyłączPanel("GameOver Panel", true);
        PomocniczeFunkcje.mainMenu.WłączWyłączPanel(new string[] { "ui_down", "UI_LicznikCzasu" }, false);
        poziomZakonczony = true;
        iloscAktywnychWrogów = 0;
        PomocniczeFunkcje.mainMenu.OdpalKursor = false;
    }
    public void PrzejdźNaNastepnyPoziom(bool czyNowyPoziom = true)
    {
        if (czyNowyPoziom)
        {
            if (aktualnyPoziomEpoki >= 100)
            {
                aktualnyPoziomEpoki = 0;
                byte f = (byte)aktualnaEpoka;
                aktualnaEpoka = Epoki.None;
                f++;
                aktualnaEpoka += f;
            }
            else
            {
                aktualnyPoziomEpoki++;
            }
        }
        PomocniczeFunkcje.mainMenu.nastepnyPoziom.interactable = false;
        PomocniczeFunkcje.mainMenu.WyłączPanelStatystyk();
        PomocniczeFunkcje.mainMenu.WłączWyłączPanel(new string[] { "GameOver Panel", "WinTXT", "LoseTXT" }, false);
        poziomZakonczony = false;
        PomocniczeFunkcje.mainMenu.ResetSceny((sbyte)aktualnyPoziomEpoki);
    }
    #endregion
    #region Metody ogólne
    private void UtworzSzablonPlikuJezykowego()
    {
        string sciezka = "Assets/Resources/jezyki.txt";
        if (plikJezykowy == null)
        {
            if (File.Exists(sciezka))
            {
                File.Delete(sciezka);
            }
            StreamWriter writer = File.CreateText(sciezka);
            UnityEngine.UI.Text[] wszystkieFrazy = FindObjectsOfType<UnityEngine.UI.Text>() as UnityEngine.UI.Text[];
            for (ushort i = 0; i < wszystkieFrazy.Length; i++)
            {
                string zapisywanaFraza = "";
                if (wszystkieFrazy[i].transform.name != "Text")
                {
                    zapisywanaFraza = zapisywanaFraza + wszystkieFrazy[i].transform.name + "|";
                }
                else
                {
                    zapisywanaFraza = zapisywanaFraza + wszystkieFrazy[i].transform.parent.name + "|";
                }
                zapisywanaFraza = zapisywanaFraza + wszystkieFrazy[i].text + ";";
                writer.WriteLine(zapisywanaFraza);
            }
            TextMesh[] allTe = FindObjectsOfType<TextMesh>() as TextMesh[];
            for (ushort i = 0; i < allTe.Length; i++)
            {
                string zapisywanaFraza = "";
                if (allTe[i].transform.name != "Text")
                {
                    zapisywanaFraza = zapisywanaFraza + allTe[i].transform.name + "|";
                }
                else
                {
                    zapisywanaFraza = zapisywanaFraza + allTe[i].transform.parent.name + "|";
                }
                zapisywanaFraza = zapisywanaFraza + allTe[i].text + ";";
                writer.WriteLine(zapisywanaFraza);
            }
            if (PomocniczeFunkcje.spawnBudynki != null)
            {
                for (ushort i = 0; i < PomocniczeFunkcje.spawnBudynki.wszystkieBudynki.Length; i++)
                {
                    string zapisywanaFraza = "";
                    KonkretnyNPCStatyczny knpcs = PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[i].GetComponent<KonkretnyNPCStatyczny>();
                    zapisywanaFraza = zapisywanaFraza + knpcs.gameObject.name + "=nazwa|";
                    zapisywanaFraza = zapisywanaFraza + knpcs.nazwa + ";";
                    writer.WriteLine(zapisywanaFraza);
                    zapisywanaFraza = knpcs.gameObject.name + "=opis|";
                    zapisywanaFraza = zapisywanaFraza + knpcs.opisBudynku + ";";
                    writer.WriteLine(zapisywanaFraza);
                }
            }
            else
            {
                Debug.Log("Spawn Budynki jest null");
            }
            if (wszystkieRodzajeWrogichJednostek != null && wszystkieRodzajeWrogichJednostek.Length > 0)
            {
                for (ushort i = 0; i < wszystkieRodzajeWrogichJednostek.Length; i++)
                {
                    string zapisywanaFraza = "";
                    zapisywanaFraza = zapisywanaFraza + wszystkieRodzajeWrogichJednostek[i].name + "=nazwa|";
                    zapisywanaFraza = zapisywanaFraza + wszystkieRodzajeWrogichJednostek[i].nazwa + ";";
                    writer.WriteLine(zapisywanaFraza);
                }
            }
            if (PomocniczeFunkcje.managerGryScript != null)
            {
                for (ushort i = 0; i < ekwipunekGracza.Length; i++)
                {
                    string zapisywanaFraza = "";
                    zapisywanaFraza = zapisywanaFraza + ekwipunekGracza[i].gameObject.name + "|";
                    zapisywanaFraza = zapisywanaFraza + ekwipunekGracza[i].nazwaPrzedmiotu + ";";
                    writer.WriteLine(zapisywanaFraza);
                }
            }
            writer.Close();
        }
    }
    public void RozwójBudynkow(byte idxRozwojuBudynku)
    {
        if (iloscCoinów >= kosztRozwojuAkademii)
        {
            iloscCoinów -= kosztRozwojuAkademii;
            PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", iloscCoinów.ToString());
            switch (idxRozwojuBudynku)
            {
                case 1: //Max HP
                    hpIdx++;
                    PomocniczeFunkcje.korzeńDrzewaPozycji.ExecuteAll(1);
                    DodajDoWartościStatystyk(1, -kosztRozwojuAkademii);
                    break;
                case 2: //Max atak
                    atkIdx++;
                    PomocniczeFunkcje.korzeńDrzewaPozycji.ExecuteAll(2);
                    DodajDoWartościStatystyk(1, -kosztRozwojuAkademii);
                    break;
                case 3: //Max obrona
                    defIdx++;
                    PomocniczeFunkcje.korzeńDrzewaPozycji.ExecuteAll(3);
                    DodajDoWartościStatystyk(1, -kosztRozwojuAkademii);
                    break;
            }
        }
    }
    public void KasujZapis()
    {
        PomocniczeFunkcje.KasujZapis();
    }
    public void MetodaDoOdpaleniaPoWyczekaniu()
    {
        PomocniczeFunkcje.LadujDaneOpcje();
        ZmianaJęzyka((byte)PomocniczeFunkcje.mainMenu.lastIdxJezyka);
        PomocniczeFunkcje.ŁadujDane();
        //UtworzSzablonPlikuJezykowego();
        PomocniczeFunkcje.mainMenu.UstawDropDownEkwipunku(ref ekwipunekGracza);
        PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
    }
    public IEnumerator CzekajAz()
    {
        yield return new WaitForEndOfFrame();
        MetodaDoOdpaleniaPoWyczekaniu();
    }
    public void DodajDoTablicyStackowParticli(ref ParticleSystem ps)
    {
        if (particleStack == null)
        {
            particleStack = new List<Stack<ParticleSystem>>();
        }
        bool find = false;
        sbyte idxCout0 = -1;
        for (byte i = 0; i < particleStack.Count; i++)   //Próba wyszukania particli (szukanie następuje po ustawionym maxParticles)
        {
            if (particleStack[i] != null && particleStack[i].Count > 0)
            {
                if (particleStack[i].Peek().main.maxParticles == ps.main.maxParticles)
                {
                    find = true;
                    particleStack[i].Push(ps);
                    return;
                }
            }
            else if (idxCout0 == -1 && particleStack[i] != null && particleStack.Count == 0)
            {
                idxCout0 = (sbyte)i;
            }
        }
        if (!find)   //Nie ma w liście danych particli
        {
            if (idxCout0 > -1)   //Było puste miejsce w tabliy
            {
                particleStack[idxCout0].Push(ps);
                return;
            }
            else    //Nie było pustego miejsca w tablicy
            {
                Stack<ParticleSystem> psDoDodania = new Stack<ParticleSystem>();
                psDoDodania.Push(ps);
                particleStack.Add(psDoDodania);
            }
        }
    }
    public ParticleSystem PobierzParticleSystem(ref ParticleSystem szukanyParticle)
    {
        if (particleStack == null || particleStack.Count == 0)
            return null;
        for (byte i = 0; i < particleStack.Count; i++)
        {
            if (particleStack[i] != null && particleStack[i].Count > 0)
            {
                if (particleStack[i].Peek().main.maxParticles == szukanyParticle.main.maxParticles)
                {
                    return particleStack[i].Pop();

                }
            }
        }
        return null;
    }
    ///<summary>Wartości jakie mają zostać dodane do tablicy kosztów i zysków.</summary>
    ///<param name="indeks">Do czego ma zostać dodana wartość (0-Badania, 1-Rozwój z akademii, 2-Budowa budynków, 3-Naprawa budynków, 4-Zyski z pokonanych wrogów, 5-Ilość nagrody za ukończony poziom).</param>
    ///<param name="value">Wartość jaka ma zostać dodana do indeksu.</param>
    public void DodajDoWartościStatystyk(byte indeks, int value)
    {
        this.wartościDlaStatystyk[indeks] += value;
    }
    #endregion
}
