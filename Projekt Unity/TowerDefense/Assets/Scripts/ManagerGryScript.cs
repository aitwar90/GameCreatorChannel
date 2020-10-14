using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ManagerGryScript : MonoBehaviour
{
    [Header("Podstawowe informacje dla gracza")]
    #region Zmienne publiczne
    [Tooltip("Aktualna ilość monet")]
    public static ushort iloscCoinów = 10;
    [Tooltip("Aktualna epoka w której gra gracz")]
    public Epoki aktualnaEpoka;
    public byte aktualnyPoziomEpoki = 1;

    [Header("Informacje o graczu")]
    [Tooltip("Ilość fal w hordzie")]
    public byte iloscFalWHordzie = 5;
    [Tooltip("Czas pomięczy kolejnymi falami hordy")]
    public byte czasWMinutachMiędzyFalami = 10;
    public static short iloscAktywnychWrogów = 0;

    [Tooltip("Zaznaczony NPC")]
    public NPCClass zaznaczonyObiekt = null;
    public delegate void WywołajResetujŚcieżki(KonkretnyNPCStatyczny knpcs = null);
    public WywołajResetujŚcieżki wywołajResetŚcieżek;
    public GameObject[] bazy = new GameObject[1];
    [Tooltip("Lista nagrod ze skrzynek dla gracza")]
    //    public List<EkwipunekScript> ekwipunekGracza = new List<EkwipunekScript>();
    public Skrzynka[] skrzynki = new Skrzynka[4];
    private KonkretnyNPCStatyczny knpcsBazy = null;
    private byte idxOfManagerGryScript = 0;
    #endregion

    #region Prywatne zmienne
    private bool czyScenaZostałaZaładowana = false;
    private byte aktualnaIlośćFal = 0;
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
        PomocniczeFunkcje.ŁadujDane();
    }
    private void ŁadowanieDanych()
    {
        PomocniczeFunkcje.spawnerHord = FindObjectOfType(typeof(SpawnerHord)) as SpawnerHord;
        Terrain terr = FindObjectOfType(typeof(Terrain)) as Terrain;
        PomocniczeFunkcje.tablicaWież = new List<InformacjeDlaPolWież>[20, 20];
        PomocniczeFunkcje.aktualneGranicaTab = (ushort)((terr.terrainData.size.x - 40) / 2.0f);
        PomocniczeFunkcje.distXZ = (terr.terrainData.size.x - (PomocniczeFunkcje.aktualneGranicaTab * 2)) / 20f;
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (byte i = 0; i < 4; i++)
            {
                skrzynki[i].OdejmnijCzas();
            }
        }
        if (czyScenaZostałaZaładowana)
        {
            if (aktualnaIlośćFal >= iloscFalWHordzie && iloscAktywnychWrogów <= 0)
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
        PomocniczeFunkcje.spawnerHord.GenerujSpawn(aktualnaEpoka);
        aktualnaIlośćFal++;
        if (aktualnaIlośćFal < iloscFalWHordzie)
        {
            StartCoroutine("WyzwólKolejnąFalę");
        }
    }
#if UNITY_STANDALONE
    void OnGUI()
    {
        EditorGUI.TextField(new Rect(10, 20, 300, 20), "Zaznaczony obiekt: " + ((zaznaczonyObiekt == null) ? "null" : zaznaczonyObiekt.name));
    }
#endif
    private void KoniecPoziomuZakończony(bool sukces = true)
    {
        if (sukces)
        {
            if (aktualnyPoziomEpoki == PomocniczeFunkcje.odblokowanyPoziomEpoki)
            {
                if (aktualnyPoziomEpoki == 100 && (byte)aktualnaEpoka < PomocniczeFunkcje.odblokowanyPoziomEpoki)
                {
                    PomocniczeFunkcje.odblokowanyPoziomEpoki++;
                }
                PomocniczeFunkcje.odblokowanyPoziomEpoki++;
            }
            for (byte i = 0; i < 4; i++)
            {
                if (!skrzynki[i].button.enabled && !skrzynki[i].ReuseTImer)
                {
                    skrzynki[i].RozpocznijOdliczanie();
                    break;
                }
            }
            Debug.Log("Maksi Kaz rusza na łowy");
            PomocniczeFunkcje.ZapiszDane();
        }
        else
        {
            Debug.Log("Porażka, dwa kieliszki i flaszka");
        }
    }
}
