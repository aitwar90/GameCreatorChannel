using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerGryScript : MonoBehaviour
{
    [Header("Podstawowe informacje dla gracza")]
    #region Zmienne publiczne
    [Tooltip("Aktualna ilość monet")]
    public ushort iloscCoinów = 10;
    [Tooltip("Aktualna epoka w której gra gracz")]
    public Epoki aktualnaEpoka;

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
    }
    private void ŁadowanieDanych()
    {
        PomocniczeFunkcje.spawnerHord = FindObjectOfType(typeof(SpawnerHord)) as SpawnerHord;
        Terrain terr = FindObjectOfType(typeof(Terrain)) as Terrain;
        PomocniczeFunkcje.tablicaWież = new List<InformacjeDlaPolWież>[20, 20];
        PomocniczeFunkcje.aktualneGranicaTab = (ushort)((terr.terrainData.size.x - 40) / 2.0f);
        PomocniczeFunkcje.distXZ = (terr.terrainData.size.x - (PomocniczeFunkcje.aktualneGranicaTab * 2)) / 20f;
        //Debug.Log("DistXZ = "+PomocniczeFunkcje.distXZ+" aktualnaGranicaTab = "+PomocniczeFunkcje.aktualneGranicaTab);
        /*
        GameObject go = new GameObject("Rodzic Punktów");
        for(byte x = 0; x < 20; x++)
        {
            float fx = PomocniczeFunkcje.aktualneGranicaTab+ x*PomocniczeFunkcje.distXZ;
            for(byte z = 0; z < 20; z++)
            {
                float fz = PomocniczeFunkcje.aktualneGranicaTab+ z*PomocniczeFunkcje.distXZ;
                GameObject gos = new GameObject("X="+x+" Z="+z);
                gos.transform.position = new Vector3(fx, 0.1f, fz);
                gos.transform.SetParent(go.transform);
            }
        }
        */
    }
    public void GenerujBaze()
    {
        sbyte idxEpokiBazyWTablicy = (sbyte)((sbyte)aktualnaEpoka - 1);
        if (idxEpokiBazyWTablicy < 0 || idxEpokiBazyWTablicy >= bazy.Length)
        {
            Debug.Log("idxEpokaBazyWTablicy = "+idxEpokiBazyWTablicy);
            return;
        }
        else
        {
            Debug.Log("Generuję bazę");
            ŁadowanieDanych();
            GameObject baza = GameObject.Instantiate(bazy[idxEpokiBazyWTablicy], new Vector3(50.0f, 1.5f, 50.0f), Quaternion.identity);
            PomocniczeFunkcje.DodajDoDrzewaPozycji(baza.GetComponent<KonkretnyNPCStatyczny>(), ref PomocniczeFunkcje.korzeńDrzewaPozycji);
            baza.transform.SetParent(PomocniczeFunkcje.spawnBudynki.rodzicBudynkow);
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
/*
        if (Input.GetTouch(0).phase == TouchPhase.Began && Input.touchCount > 0)
        {
            zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt);
            Debug.Log("Zaznaczony obiekt " + zaznaczonyObiekt.nazwa);
        }
        */
#endif
    }
    void Update()
    {
        if (czyScenaZostałaZaładowana)
        {
            if (aktualnaIlośćFal >= iloscFalWHordzie && iloscAktywnychWrogów <= 0)
            {
                //Lvl skończony wszystkie fale zostały pokonane
                KoniecPoziomuZakończony(true);
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
        yield return new WaitForSeconds(czasWMinutachMiędzyFalami * 15);
        aktualnaIlośćFal++;
        PomocniczeFunkcje.spawnerHord.GenerujSpawn(aktualnaEpoka);
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
            Debug.Log("Wszyscy przeciwnicy zostali pokonani");
        }
    }
}
