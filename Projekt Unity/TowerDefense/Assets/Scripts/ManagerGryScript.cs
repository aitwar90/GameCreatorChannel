using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
    public delegate void WywołajResetujŚcieżki();
    public WywołajResetujŚcieżki wywołajResetŚcieżek;
    #endregion

    #region Prywatne zmienne
    private byte aktualnaIlośćFal = 0;
    #endregion  
    void Awake()
    {
        PomocniczeFunkcje.managerGryScript = this;
        PomocniczeFunkcje.spawnBudynki = FindObjectOfType(typeof(SpawnBudynki)) as SpawnBudynki;
        PomocniczeFunkcje.spawnerHord = FindObjectOfType(typeof(SpawnerHord)) as SpawnerHord;
        Terrain terr = FindObjectOfType(typeof(Terrain)) as Terrain;
        PomocniczeFunkcje.tablicaWież = new List<KonkretnyNPCStatyczny>[20, 20];
        PomocniczeFunkcje.aktualneGranicaTab = (ushort)((terr.terrainData.size.x - 100) / 2);
    }
    void Start()
    {
        if (aktualnaEpoka != Epoki.None)
        {
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
        if (Input.GetTouch(0) && Input.touchCout > 0)
            {
                zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt);
            }
#endif
    }
    void Update()
    {
        if (aktualnaIlośćFal >= iloscFalWHordzie && iloscAktywnychWrogów <= 0)
        {
            //Lvl skończony wszystkie fale zostały pokonane
            KoniecPoziomuZakończony(true);
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
    void OnGUI()
    {
        EditorGUI.TextField(new Rect(10, 20, 300, 20), "Zaznaczony obiekt: " + ((zaznaczonyObiekt == null) ? "null" : zaznaczonyObiekt.name));
    }
    private void KoniecPoziomuZakończony(bool sukces = true)
    {
        if (sukces)
        {
            Debug.Log("Wszyscy przeciwnicy zostali pokonani");
        }
    }
}
