using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerGryScript : MonoBehaviour
{
    [Header("Podstawowe informacje dla gracza")]
    #region Zmienne publiczne
    [Tooltip("Aktualna ilość monet")]
    public static ushort iloscCoinów = 10000;
    [Tooltip("Aktualna epoka w której gra gracz")]
    public Epoki aktualnaEpoka;
    public byte aktualnyPoziomEpoki = 1;

    [Header("Informacje o graczu")]
    [Tooltip("Czas pomięczy kolejnymi falami hordy")]
    public byte czasWMinutachMiędzyFalami = 10;
    public static short iloscAktywnychWrogów = 0;

    [Tooltip("Zaznaczony NPC")]
    public NPCClass zaznaczonyObiekt = null;
    public delegate void WywołajResetujŚcieżki(KonkretnyNPCStatyczny knpcs = null);
    public WywołajResetujŚcieżki wywołajResetŚcieżek;
    public GameObject[] bazy = new GameObject[1];
    [Tooltip("Tablica nagród jakie gracz może otrzymać")]
    public PrzedmiotScript[] ekwipunekGracza = null;
    [Tooltip("Nagrody jakie gracz poisada")]
    public EkwipunekScript ekwipunek;
    private Skrzynka[] skrzynki;
    #endregion

    #region Prywatne zmienne
    private KonkretnyNPCStatyczny knpcsBazy = null;
    private byte idxOfManagerGryScript = 0;
    private bool czyScenaZostałaZaładowana = false;
    private bool toNieOstatniaFala = true;
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
    #endregion  
    void Awake()
    {
        PomocniczeFunkcje.managerGryScript = this;
        PomocniczeFunkcje.spawnBudynki = FindObjectOfType(typeof(SpawnBudynki)) as SpawnBudynki;
        PomocniczeFunkcje.mainMenu = FindObjectOfType(typeof(MainMenu)) as MainMenu;
        skrzynki = new Skrzynka[PomocniczeFunkcje.mainMenu.buttonSkrzynki.Length];
        for (byte i = 0; i < skrzynki.Length; i++)
        {
            skrzynki[i].UstawButtonSkrzynki(PomocniczeFunkcje.mainMenu.buttonSkrzynki[i]);
        }
        PomocniczeFunkcje.ŁadujDane();
    }
    private void ŁadowanieDanych()
    {
        PomocniczeFunkcje.spawnerHord = FindObjectOfType(typeof(SpawnerHord)) as SpawnerHord;
        PomocniczeFunkcje.spawnerHord.UstawHorde(aktualnaEpoka, aktualnyPoziomEpoki);
        Terrain terr = FindObjectOfType(typeof(Terrain)) as Terrain;
        PomocniczeFunkcje.tablicaWież = new List<InformacjeDlaPolWież>[20, 20];
        PomocniczeFunkcje.aktualneGranicaTab = (ushort)((terr.terrainData.size.x - 40) / 2.0f);
        PomocniczeFunkcje.distXZ = (terr.terrainData.size.x - (PomocniczeFunkcje.aktualneGranicaTab * 2)) / 20f;
        /*
        for(byte i = 0; i < 20; i++)
        {
            for(byte j = 0; j < 20; j++)
            {
                GameObject go = new GameObject("X = "+i+" Z = "+j);
                go.transform.position = new Vector3(i*PomocniczeFunkcje.distXZ + PomocniczeFunkcje.aktualneGranicaTab, 1.0f, j*PomocniczeFunkcje.distXZ + PomocniczeFunkcje.aktualneGranicaTab);
                go.AddComponent<TestOnEnableGround>();
                TestOnEnableGround toeg = go.GetComponent<TestOnEnableGround>();
                toeg.X = i;
                toeg.Z = j;
            }
        }
        */
        PomocniczeFunkcje.ZapiszDane();
    }
    public void GenerujBaze()
    {
        if (aktualnyPoziomEpoki > PomocniczeFunkcje.odblokowanyPoziomEpoki)
        {
            Debug.Log("Poziom epoki nie został odblokowany");
            return;
        }
        else if ((byte)aktualnaEpoka > PomocniczeFunkcje.odblokowanyPoziomEpoki)
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
            GameObject baza = GameObject.Instantiate(bazy[idxEpokiBazyWTablicy], new Vector3(50.0f, 1.5f, 50.0f), Quaternion.identity);
            PomocniczeFunkcje.DodajDoDrzewaPozycji(baza.GetComponent<KonkretnyNPCStatyczny>(), ref PomocniczeFunkcje.korzeńDrzewaPozycji);
            baza.transform.SetParent(PomocniczeFunkcje.spawnBudynki.rodzicBudynkow);
            knpcsBazy = baza.GetComponent<KonkretnyNPCStatyczny>();
            StartCoroutine("WyzwólKolejnąFalę");
        }
    }
    void FixedUpdate()
    {
        /*
        Fragment kodu, który ma za zadanie zaznaczyć obiekt
        */
#if UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt);
        }
#endif
#if UNITY_ANDROID
        if (Input.mousePresent)
        {
            if (Input.GetMouseButtonDown(0))
            {
                zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt);
            }
        }
        else
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt);
            }
        }
#endif
        if (zaznaczonyObiekt != null)
        {
            PomocniczeFunkcje.spawnBudynki.teksAktualnegoObiektu.text = "Altualnie zaznaczony obiekt: " + zaznaczonyObiekt.nazwa;
        }
    }
    void Update()
    {
        switch (idxOfManagerGryScript)
        {
            case 255:
                for (byte i = 0; i < 4; i++)
                {
                    skrzynki[i].SprawdźCzyReuseMinęło();
                }
                idxOfManagerGryScript++;
                break;
            default:
                idxOfManagerGryScript++;
                break;

        }
        if (czyScenaZostałaZaładowana)
        {
            if (!toNieOstatniaFala && iloscAktywnychWrogów <= 0)
            {
                //Lvl skończony wszystkie fale zostały pokonane
                KoniecPoziomuZakończony(true);
            }
            else if (knpcsBazy.AktualneŻycie <= 0)
            {
                KoniecPoziomuZakończony(false);
            }
        }
        else
        {
            SprawdźCzyScenaZostałaZaładowana();
        }
    }
    private void SprawdźCzyScenaZostałaZaładowana()
    {
        Scene s = SceneManager.GetSceneByBuildIndex((byte)aktualnaEpoka);
        if (s.isLoaded)
        {
            czyScenaZostałaZaładowana = true;
            GenerujBaze();
        }
    }
    private IEnumerator WyzwólKolejnąFalę()
    {
        yield return new WaitForSeconds(czasWMinutachMiędzyFalami * 60);
        toNieOstatniaFala = PomocniczeFunkcje.spawnerHord.GenerujSpawn(aktualnaEpoka);
        if (toNieOstatniaFala)
        {
            StartCoroutine("WyzwólKolejnąFalę");
        }
    }
    private void KoniecPoziomuZakończony(bool sukces = true)
    {
        if (sukces)
        {
            if (aktualnyPoziomEpoki == PomocniczeFunkcje.odblokowanyPoziomEpoki)
            {
                if (aktualnyPoziomEpoki == 100 && (byte)aktualnaEpoka < PomocniczeFunkcje.odblokowanyPoziomEpoki)
                {
                    PomocniczeFunkcje.odblokowaneEpoki++;
                }
                PomocniczeFunkcje.odblokowanyPoziomEpoki++;
            }
            for (byte i = 0; i < 4; i++)
            {
                if (!skrzynki[i].button.enabled && !skrzynki[i].ReuseTimer)
                {
                    skrzynki[i].RozpocznijOdliczanie();
                    break;
                }
            }
            PomocniczeFunkcje.ZapiszDane();
            PomocniczeFunkcje.mainMenu.nastepnyPoziom.gameObject.SetActive(true);
            PomocniczeFunkcje.mainMenu.powtorzPoziom.gameObject.SetActive(true);
            //Tu reset sceny jak kliknie button
        }
        else
        {
            PomocniczeFunkcje.mainMenu.powtorzPoziom.gameObject.SetActive(true);
        }
        toNieOstatniaFala = true;
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
        PomocniczeFunkcje.mainMenu.nastepnyPoziom.gameObject.SetActive(false);
        PomocniczeFunkcje.mainMenu.powtorzPoziom.gameObject.SetActive(false);
        PomocniczeFunkcje.mainMenu.ResetSceny();
    }
    public void CudOcalenia()
    {
        PomocniczeFunkcje.celWrogów.AktualneŻycie = (short)(PomocniczeFunkcje.celWrogów.maksymalneŻycie/ 2.0f);
        KonkretnyNPCDynamiczny[] knpcd = FindObjectsOfType(typeof(KonkretnyNPCDynamiczny)) as KonkretnyNPCDynamiczny[];
        for(ushort i = 0; i < knpcd.Length; i++)
        {
            knpcd[i].AktualneŻycie = 0;
        }
        KonkretnyNPCStatyczny[] knpcs = FindObjectsOfType(typeof(KonkretnyNPCStatyczny)) as KonkretnyNPCStatyczny[];
        for(ushort i = 0; i < knpcs.Length; i++)
        {
            if(knpcs[i].AktualneŻycie > 0)
                knpcs[i].AktualneŻycie = knpcs[i].maksymalneŻycie;
        }
    }
    public void KliknietyPrzycisk(byte idx)
    {
        skrzynki[idx].button.enabled = false;
        if(ekwipunek == null)
            ekwipunek = new EkwipunekScript(null);
        ekwipunek.LosujNagrode();
    }
}
