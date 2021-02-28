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
    public static ushort iloscCoinów = 150;
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
    public bool toNieOstatniaFala = false;
    private ObsługaReklam or;
    private float timerFal;
    private short valFPS = 0;
    private byte aktualnyIndexTabFPS = 0;
    private bool poziomZakonczony = false;
    private byte bufferTimerFal = 255;
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
    public bool CzyReklamaZaładowana
    {
        get
        {
            return or.ZaładowanaReklamaJest;
        }
    }
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
        PomocniczeFunkcje.managerGryScript = this;
        PomocniczeFunkcje.spawnBudynki = FindObjectOfType(typeof(SpawnBudynki)) as SpawnBudynki;
        PomocniczeFunkcje.mainMenu = FindObjectOfType(typeof(MainMenu)) as MainMenu;
        PomocniczeFunkcje.muzyka = this.GetComponent<MuzykaScript>();
        or = FindObjectOfType(typeof(ObsługaReklam)) as ObsługaReklam;
        SpawnerHord.actualHPBars = 0;
        skrzynki = new Skrzynka[PomocniczeFunkcje.mainMenu.buttonSkrzynki.Length];
        for (byte i = 0; i < skrzynki.Length; i++)
        {
            skrzynki[i] = new Skrzynka(ref PomocniczeFunkcje.mainMenu.buttonSkrzynki[i]);
        }
        StartCoroutine(CzekajAz());
    }
    void Update()
    {
        if (!PomocniczeFunkcje.mainMenu.CzyOdpaloneMenu)
        {
            /*
            Fragment kodu, który ma za zadanie zaznaczyć obiekt
            */
#if UNITY_STANDALONE
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
#endif
#if UNITY_ANDROID || UNITY_IOS
            if (Input.mousePresent)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (PomocniczeFunkcje.spawnBudynki.aktualnyObiekt == null)
                    {
                        zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt);
                        if (zaznaczonyObiekt != null && zaznaczonyObiekt.AktualneŻycie > 0 && !PomocniczeFunkcje.CzyKliknalemUI())
                        {
                            zaznaczonyObiekt.UstawPanel(Input.mousePosition);
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
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    if (PomocniczeFunkcje.spawnBudynki.aktualnyObiekt == null)
                    {
                        zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt);
                        if (zaznaczonyObiekt != null && zaznaczonyObiekt.AktualneŻycie > 0 && !PomocniczeFunkcje.CzyKliknalemUI())
                        {
                            zaznaczonyObiekt.UstawPanel(Input.GetTouch(0).position);
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
#endif
        switch (idxOfManagerGryScript)  //Każdy idxOfManagerGryScript podzielny przez 5 bez reszty obsługuje timerFal
        {
            case 0:
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
            case 1:
                if (czyScenaZostałaZaładowana)
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
                else
                {
                    SprawdźCzyScenaZostałaZaładowana();
                }
                idxOfManagerGryScript++;
                break;
            case 5:
                for (byte i = 0; i < 4; i++)
                {
                    skrzynki[i].SprawdźCzyReuseMinęło();
                }
                idxOfManagerGryScript = 0;
                break;
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
        PomocniczeFunkcje.spawnerHord = FindObjectOfType(typeof(SpawnerHord)) as SpawnerHord;
        PomocniczeFunkcje.spawnerHord.UstawHorde(aktualnaEpoka, aktualnyPoziomEpoki);
        PomocniczeFunkcje.kameraZostalaPrzesunieta = 2;
        Terrain terr = FindObjectOfType(typeof(Terrain)) as Terrain;
        PomocniczeFunkcje.tablicaWież = new List<InformacjeDlaPolWież>[22, 22];
        PomocniczeFunkcje.aktualneGranicaTab = (ushort)((terr.terrainData.size.x - 56) / 2.0f);
        PomocniczeFunkcje.distXZ = (terr.terrainData.size.x - (PomocniczeFunkcje.aktualneGranicaTab * 2)) / PomocniczeFunkcje.tablicaWież.GetLength(0);
        PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
        PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćFal", SpawnerHord.actFala.ToString() + "/" + SpawnerHord.iloscFalNaKoncu.ToString());
        PomocniczeFunkcje.mainMenu.WłączWyłączPanel("ui_down", true);
        PomocniczeFunkcje.spawnBudynki.InicjacjaPaneluBudynków();
        PomocniczeFunkcje.mainMenu.WygenerujIPosortujTablice(); //Generuje i sortuje tablice budynków do wybudowania
        ObslTimerFal(0);
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
            //Debug.Log("Ustawiam budynek główny");
            GameObject baza = GameObject.Instantiate(bazy[idxEpokiBazyWTablicy], new Vector3(MoveCameraScript.bazowePolozenieKameryGry.x, 0.0f, MoveCameraScript.bazowePolozenieKameryGry.z - 5f), Quaternion.identity);
            knpcsBazy = baza.GetComponent<KonkretnyNPCStatyczny>();
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
        iloscAktywnychWrogów = 0;
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
                timerFal += Time.deltaTime * 5.0f;
            }
            else
            {
                timerFal = 0;
                if (aktualnyPoziomEpoki == 255 && iloscAktywnychWrogów > 0)
                    return;
                PomocniczeFunkcje.spawnerHord.GenerujSpawn(aktualnaEpoka);
                MuzykaScript.singleton.WłączWyłączClip(true, "Bitwa");
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
            if (czasMiędzyFalami - 1 == czas)
            {
                MuzykaScript.singleton.WłączWyłączClip(true, "AmbientWGrze_" + aktualnaEpoka.ToString(), false);
                SpawnerHord.actualHPBars = 0;
            }
            bufferTimerFal = czas;
            PomocniczeFunkcje.mainMenu.UstawTextUI("timer", czas.ToString());
        }
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
            for (ushort i = 0; i < fLines.Length; i++)
            {
                string[] pFrazy = fLines[i].Split('|');
                if (idx >= pFrazy.Length)
                {
                    continue;
                }
                if (pFrazy[idx] != "")
                {
                    for (ushort j = 0; j < wszystkieFrazy.Length; j++)
                    {
                        if (wszystkieFrazy[j].transform.name != "Text")
                        {
                            if (pFrazy[0] == wszystkieFrazy[j].transform.name)
                            {
                                wszystkieFrazy[j].text = pFrazy[idx];
                                if (f != null)
                                    wszystkieFrazy[j].font = f;
                                break;
                            }
                        }
                        else
                        {
                            if (pFrazy[0] == wszystkieFrazy[j].transform.parent.name)
                            {
                                wszystkieFrazy[j].text = pFrazy[idx];
                                if (f != null)
                                    wszystkieFrazy[j].font = f;
                                break;
                            }
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
    private void OdblokujKolejnaSkrzynke()
    {
        for (byte i = 0; i < skrzynki.Length; i++)
        {
            if (!skrzynki[i].ReuseTimer && !skrzynki[i].button.interactable)
            {
                skrzynki[i].RozpocznijOdliczanie();
                break;
            }
        }
    }
    public void KliknietyButtonZwiekszeniaNagrodyPoLvlu()
    {
        ushort c = (ushort)(((byte)aktualnaEpoka) * aktualnyPoziomEpoki * 15);
        or.OtwórzReklame(1, c);
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
    public void KlikniętaReklamaButtonSkrzynki(byte idx)
    {
        or.OtwórzReklame(2, idx);
    }
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
            PomocniczeFunkcje.mainMenu.rekZaWyzszaNagrode.gameObject.SetActive(CzyReklamaZaładowana);
            OdblokujKolejnaSkrzynke();
            PomocniczeFunkcje.ZapiszDane();
            iloscCoinów += (ushort)(((byte)aktualnaEpoka) * aktualnyPoziomEpoki * 15);
            PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
            PomocniczeFunkcje.mainMenu.WłączWyłączPanel("WinTXT", true);
            MuzykaScript.singleton.WłączWyłączClip(true, "Zwycięstwo");
        }
        else
        {
            PomocniczeFunkcje.mainMenu.WłączWyłączPanel("LoseTXT", true);
            MuzykaScript.singleton.WłączWyłączClip(true, "Przegrana");
        }
        PomocniczeFunkcje.mainMenu.UstawPrzyciskObrotu(false);
        PomocniczeFunkcje.mainMenu.WłączWyłączPanel("GameOver Panel", true);
        poziomZakonczony = true;
        iloscAktywnychWrogów = 0;
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
        switch (idxRozwojuBudynku)
        {
            case 1: //Max HP
                hpIdx++;
                PomocniczeFunkcje.korzeńDrzewaPozycji.ExecuteAll(1);
                break;
            case 2: //Max atak
                atkIdx++;
                PomocniczeFunkcje.korzeńDrzewaPozycji.ExecuteAll(2);
                break;
            case 3: //Max obrona
                defIdx++;
                PomocniczeFunkcje.korzeńDrzewaPozycji.ExecuteAll(3);
                break;
        }
    }
    public void KasujZapis()
    {
        PomocniczeFunkcje.KasujZapis();
    }
    public void MetodaDoOdpaleniaPoWyczekaniu()
    {
        PomocniczeFunkcje.LadujDaneOpcje();
        PomocniczeFunkcje.ŁadujDane();
        //UtworzSzablonPlikuJezykowego();
        if (blokowanieOrientacji)
        {
            Screen.orientation = ScreenOrientation.Landscape;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToPortrait = false;
        }
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
    #endregion
}
