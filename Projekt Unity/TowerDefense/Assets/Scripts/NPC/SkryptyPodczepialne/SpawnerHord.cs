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
    public Transform rodzicNPC = null;

    private void ObsłużFale(Epoki e)
    {
        byte aktPozEpoki = PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki;
        if (e == Epoki.EpokaKamienia)
        {
            if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 1;
            }
            else
            {
                ostatniaIlośćWFali = 50;
                ostatniaIlośćWFali += (ushort)Mathf.FloorToInt(ostatniaIlośćWFali * 0.1f * (aktPozEpoki - 50));
            }
        }
        else if (e == Epoki.EpokaStarożytna)
        {
            if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 1;
            }
            else
            {
                ostatniaIlośćWFali = 50;
                ostatniaIlośćWFali += (ushort)Mathf.FloorToInt(ostatniaIlośćWFali * 0.1f * (aktPozEpoki - 50));
            }
        }
        else if (e == Epoki.EpokaŚredniowiecza)
        {
            if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 1;
            }
            else
            {
                ostatniaIlośćWFali = 50;
                ostatniaIlośćWFali += (ushort)Mathf.FloorToInt(ostatniaIlośćWFali * 0.1f * (aktPozEpoki - 50));
            }
        }
        else if (e == Epoki.EpokaNowożytna)
        {
            if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 1;
            }
            else
            {
                ostatniaIlośćWFali = 50;
                ostatniaIlośćWFali += (ushort)Mathf.FloorToInt(ostatniaIlośćWFali * 0.1f * (aktPozEpoki - 50));
            }
        }
        else if (e == Epoki.EpokaWspołczesna)
        {
            if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 1;
            }
            else
            {
                ostatniaIlośćWFali = 50;
                ostatniaIlośćWFali += (ushort)Mathf.FloorToInt(ostatniaIlośćWFali * 0.1f * (aktPozEpoki - 50));
            }
        }
        else if (e == Epoki.EpokaPrzyszła)
        {
            if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 1;
            }
            else
            {
                ostatniaIlośćWFali = 50;
                ostatniaIlośćWFali += (ushort)Mathf.FloorToInt(ostatniaIlośćWFali * 0.1f * (aktPozEpoki - 50));
            }
        }
        else
        {
            Debug.Log("Nie wybrano epoki");
            ostatniaIlośćWFali = 0;
        }
    }
    public void UstawHorde(Epoki ep, byte poziomEpoki)
    {
        switch (ep)
        {
            case Epoki.EpokaKamienia:
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = 2/*(ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)))*/;
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                }
                break;
            case Epoki.EpokaStarożytna:
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = (ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)));
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                }
                break;
            case Epoki.EpokaŚredniowiecza:
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = (ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)));
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                }
                break;
            case Epoki.EpokaNowożytna:
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = (ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)));
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                }
                break;
            case Epoki.EpokaWspołczesna:
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = (ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)));
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                }
                break;
            case Epoki.EpokaPrzyszła:
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = (ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)));
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                }
                break;
            default:
                break;
        }
    }
    public bool GenerujSpawn(Epoki e)
    {
        if (ostatniaIlośćWFali >= maxIlośćNaFalę)
        {
            return false;
        }
        else
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
                return false;
            }
            if (ostatniaIlośćWFali < maxIlośćNaFalę)
            {
                ObsłużFale(e);
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
            SpawnujMnie(ref możliwiNPC, 1);
            for (ushort i = 1, j = 1; i < ostatniaIlośćWFali; i++)
            {
                StartCoroutine(SpawnujMnieCorutine(możliwiNPC, j, Random.Range(0, 0.5f)));
                j++;
                if (j > iloscPunktówSpawnu)
                    j = 1;
            }
            return true;
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
        PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek += knpcd.ResetujŚciezkę;
        if (czyPullowany)
        {
            knpcd.AktualneŻycie = knpcd.maksymalneŻycie;
            knpcd.RysujPasekŻycia = true;
            knpcd.NieŻyję = false;
            knpcd.SetGłównyIndex = -1;
            knpcd.WłWyłObj(true);
        }
    }
    private IEnumerator SpawnujMnieCorutine(List<KonkretnyNPCDynamiczny> możliwiNPC, ushort j, float _time)
    {
        yield return new WaitForSeconds(_time);
        SpawnujMnie(ref możliwiNPC, j);
    }
    private void SpawnujMnie(ref List<KonkretnyNPCDynamiczny> możliwiNPC, ushort j)
    {
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
    public void UsunWszystkieJednostki()
    {
        for (int i = rodzicNPC.childCount - 1; i >= 0; i--)
        {
            Destroy(rodzicNPC.GetChild(i).gameObject);
        }
    }
}
