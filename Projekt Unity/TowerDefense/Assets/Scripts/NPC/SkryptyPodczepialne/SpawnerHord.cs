using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Skrypt ma za zadanie wygenerować wrogów
*/
public class SpawnerHord : MonoBehaviour
{
    [Tooltip("Miejsca w których mogą zostać wygenerowani przewciwnicy")]
    public Transform[] spawnPunkty;
    [Tooltip("Wszystkie dostępne jednostki w grze, które mogą mieć nastawienie wrogie")]
    public NPCClass[] wszystkieRodzajeWrogichJednostek;
    [Tooltip("Ilość przeciwników w ostatnio wygenerowanej fali")]
    public ushort ostatniaIlośćWFali = 0;
    public ushort maxIlośćNaFalę = 1;
    public byte iloscPunktówSpawnu = 1;
    public static byte actualHPBars = 0;
    public byte aktualnaIloscHPBarów = 0;
    public NPCClass cel;
    private Transform rodzicNPC = null;

    public void GenerujSpawn(Epoki e)
    {
        if (cel != PomocniczeFunkcje.celWrogów)
            PomocniczeFunkcje.celWrogów = cel;
        List<KonkretnyNPCDynamiczny> możliwiNPC = new List<KonkretnyNPCDynamiczny>();
        for (byte i = 0; i < wszystkieRodzajeWrogichJednostek.Length; i++)
        {
            if (wszystkieRodzajeWrogichJednostek[i].epokaNPC == e)
            {
                możliwiNPC.Add((KonkretnyNPCDynamiczny)wszystkieRodzajeWrogichJednostek[i]);
            }
        }
        if (możliwiNPC.Count < 1)
        {
            Debug.Log("Niestety nie ma jednostek spełniających wymagania epoki do generowania hordy");
            return;
        }
        if (ostatniaIlośćWFali < maxIlośćNaFalę)
        {
            switch (e)
            {
                case Epoki.EpokaKamienia:
                    ostatniaIlośćWFali += 0;
                    break;
                case Epoki.EpokaStarożytna:
                    ostatniaIlośćWFali += 4;
                    break;
                case Epoki.EpokaŚredniowiecza:
                    ostatniaIlośćWFali += 5;
                    break;
                case Epoki.EpokaNowożytna:
                    ostatniaIlośćWFali += 6;
                    break;
                case Epoki.EpokaWspołczesna:
                    ostatniaIlośćWFali += 7;
                    break;
                case Epoki.EpokaPrzyszła:
                    ostatniaIlośćWFali += 8;
                    break;
                default:
                    Debug.Log("Nie podano odpowiedniej epoki do metody GenerujSpawn(Epoki e)");
                    break;
            }
            if (ostatniaIlośćWFali > maxIlośćNaFalę)
            {
                ostatniaIlośćWFali = maxIlośćNaFalę;
            }
        }
        if (ostatniaIlośćWFali < 11)
            iloscPunktówSpawnu = 1;
        else if (ostatniaIlośćWFali < 21)
            iloscPunktówSpawnu = 2;
        else if (ostatniaIlośćWFali < 31)
            iloscPunktówSpawnu = 3;
        else if (ostatniaIlośćWFali < 41)
            iloscPunktówSpawnu = 4;
        else if (ostatniaIlośćWFali < 51)
            iloscPunktówSpawnu = 5;
        else if (ostatniaIlośćWFali < 61)
            iloscPunktówSpawnu = 6;
        else if (ostatniaIlośćWFali < 71)
            iloscPunktówSpawnu = 7;
        else
            iloscPunktówSpawnu = 8;

        for (ushort i = 0, j = 1; i < ostatniaIlośćWFali; i++)
        {
            StartCoroutine(SpawnujMnie(możliwiNPC, j, Random.Range(0, 0.5f)));
            j++;
            if (j > iloscPunktówSpawnu)
                j = 1;
        }
    }
    private Vector3 OkreślPozucjeZOffsetem(Vector3 pos, float offsets)
    {
        float vx = UnityEngine.Random.Range(pos.x - offsets, pos.x + offsets);
        float vz = UnityEngine.Random.Range(pos.z - offsets, pos.z + offsets);
        return new Vector3(vx, pos.y, vz);
    }
    void Awake()
    {
        if (rodzicNPC == null)
        {
            GameObject go = new GameObject("rodzicNPC");
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            rodzicNPC = go.transform;
        }
        if (cel != null)
        {
            PomocniczeFunkcje.DodajDoDrzewaPozycji(cel, ref PomocniczeFunkcje.korzeńDrzewaPozycji);
        }
    }
    void Update()
    {
        if (SpawnerHord.actualHPBars != 0)
            SpawnerHord.actualHPBars = 0;
    }
    private void UstawWroga(KonkretnyNPCDynamiczny knpcd, bool czyPullowany = false)
    {
        knpcd.NastawienieNonPlayerCharacter = NastawienieNPC.Wrogie;
        PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek += knpcd.ResetujŚcieżki;
        if (czyPullowany)
        {
            knpcd.AktualneŻycie = knpcd.maksymalneŻycie;
            knpcd.RysujPasekŻycia = true;
            knpcd.SetGłównyIndex = -1;
            knpcd.WłWyłObj(true);
        }
    }
    private IEnumerator SpawnujMnie(List<KonkretnyNPCDynamiczny> możliwiNPC, ushort j, float _time)
    {
        yield return new WaitForSeconds(_time);
        bool czyPool = true;
        ushort npcIdx = (ushort)Random.Range(0, możliwiNPC.Count - 1);
        GameObject go = PomocniczeFunkcje.ZwróćOBiektPoolowany(możliwiNPC[npcIdx].nazwa);
        if (go == null)
        {
            czyPool = false;
            go = Instantiate(możliwiNPC[npcIdx].gameObject);
        }
        else
            go.SetActive(true);
        go.transform.position = OkreślPozucjeZOffsetem(spawnPunkty[j - 1].position, 3.0f);
        ManagerGryScript.iloscAktywnychWrogów++;
        KonkretnyNPCDynamiczny knpcd = go.GetComponent<KonkretnyNPCDynamiczny>();
        UstawWroga(knpcd, czyPool);
        go.transform.SetParent(rodzicNPC);
    }
    public void WywołajResetujŚcieżki()
    {
        if (PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek != null)
        {
            PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek();
        }
        else
        {
            Debug.Log("Delegatura jest null");
        }
    }
}
