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
    public delegate void WywołajResetujŚcieżki(KonkretnyNPCStatyczny knpcs = null);
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
        PomocniczeFunkcje.tablicaWież = new List<InformacjeDlaPolWież>[20, 20];
        PomocniczeFunkcje.aktualneGranicaTab = (ushort)((terr.terrainData.size.x - 40)/2.0f);
        PomocniczeFunkcje.distXZ = (terr.terrainData.size.x - (PomocniczeFunkcje.aktualneGranicaTab*2)) / 20f;
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
        if (Input.GetTouch(0).phase == TouchPhase.Began && Input.touchCount > 0)
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
