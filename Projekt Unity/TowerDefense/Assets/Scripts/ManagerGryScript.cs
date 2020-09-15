using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public byte czasWMinutachMiędzyFalami = 1;
    public static short iloscAktywnychWrogów = 0;

    #endregion

    #region Prywatne zmienne
    public byte aktualnaIlośćFal = 0;
    #endregion  

    void Awake()
    {

    }
    void Start()
    {
        if (aktualnaEpoka != Epoki.None)
        {
            StartCoroutine("WyzwólKolejnąFalę");
        }
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
        SpawnerHord sh = FindObjectOfType<SpawnerHord>() as SpawnerHord;
        aktualnaIlośćFal++;
        sh.GenerujSpawn(aktualnaEpoka);
        if (aktualnaIlośćFal < iloscFalWHordzie)
        {
            StartCoroutine("WyzwólKolejnąFalę");
        }
    }
    private void KoniecPoziomuZakończony(bool sukces = true)
    {
        if (sukces)
        {
            Debug.Log("Wszyscy przeciwnicy zostali pokonani");
        }
    }
}
