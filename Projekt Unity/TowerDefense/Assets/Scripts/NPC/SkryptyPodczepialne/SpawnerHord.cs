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
    //[Tooltip("Ilość przeciwników w ostatnio wygenerowanej fali")]
    private ushort ostatniaIlośćWFali = 0;
    private ushort maxIlośćNaFalę = 1;
    public static byte actFala = 0;
    public static byte iloscFalNaKoncu = 0;
    public static byte actualHPBars = 0;
    public NPCClass cel;
    public Transform rodzicNPC = null;

    ///<summary>Metoda określa aktualną ilość spawnowanych jednostek wroga na podstawie epoki i jej poziomu.</summary>
    ///<param name="e">Epoka poziomu.</param>
    private void ObsłużFale(Epoki e)
    {
        byte aktPozEpoki = PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki;
        if (e == Epoki.EpokaKamienia)
        {
            if(aktPozEpoki == 255)  //Samouczek
            {
                ostatniaIlośćWFali = 1;
            }
            else if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 3;
            }
            else
            {
                ostatniaIlośćWFali += (ushort)(3 + (aktPozEpoki - 45));
            }
        }
        else if (e == Epoki.EpokaStarożytna)
        {
            if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 3;
            }
           else
            {
                ostatniaIlośćWFali += (ushort)(3 + (aktPozEpoki - 44));
            }
        }
        else if (e == Epoki.EpokaŚredniowiecza)
        {
            if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 3;
            }
            else
            {
               ostatniaIlośćWFali += (ushort)(3 + (aktPozEpoki - 43));
            }
        }
        else if (e == Epoki.EpokaNowożytna)
        {
            if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 3;
            }
            else
            {
                ostatniaIlośćWFali += (ushort)(3 + (aktPozEpoki - 42));
            }
        }
        else if (e == Epoki.EpokaWspołczesna)
        {
            if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 3;
            }
            else
            {
                ostatniaIlośćWFali += (ushort)(3 + (aktPozEpoki - 41));
            }
        }
        else if (e == Epoki.EpokaPrzyszła)
        {
            if (aktPozEpoki < 50)
            {
                ostatniaIlośćWFali += 3;
            }
            else
            {
                ostatniaIlośćWFali = 50;
                ostatniaIlośćWFali += (ushort)(3 + (aktPozEpoki - 40));
            }
        }
        else
        {
            Debug.Log("Nie wybrano epoki");
            ostatniaIlośćWFali = 0;
        }
    }
    ///<summary>Metoda określa warunki krańcowe dla danego poziomu oraz epoki.</summary>
    ///<param name="ep">Epoka poziomu.</param>
    ///<param name="poziomEpoki">Wewnętrzny poziom danej epoki (1-100).</param>
    public void UstawHorde(Epoki ep, byte poziomEpoki)
    {
        switch (ep)
        {
            case Epoki.EpokaKamienia:
                if(poziomEpoki == 255)  //Samouczek
                {
                    maxIlośćNaFalę = 1;
                    iloscFalNaKoncu = 1;
                }
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = (ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)));
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / 3.0f));
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / (3 + (poziomEpoki - 45))));
                }
                break;
            case Epoki.EpokaStarożytna:
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = (ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)));
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / 3.0f));
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / (3 + (poziomEpoki - 44))));
                }
                break;
            case Epoki.EpokaŚredniowiecza:
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = (ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)));
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / 3.0f));
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / (3 + (poziomEpoki - 43))));
                }
                break;
            case Epoki.EpokaNowożytna:
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = (ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)));
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / 3.0f));
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / (3 + (poziomEpoki - 42))));
                }
                break;
            case Epoki.EpokaWspołczesna:
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = (ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)));
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / 3.0f));
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / (3 + (poziomEpoki - 41))));
                }
                break;
            case Epoki.EpokaPrzyszła:
                if (poziomEpoki < 50)
                {
                    maxIlośćNaFalę = (ushort)(11 + (Mathf.CeilToInt(poziomEpoki / 2.0f)));
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / 3.0f));
                }
                else
                {
                    maxIlośćNaFalę = (ushort)(36 + poziomEpoki);
                    iloscFalNaKoncu = (byte)(Mathf.FloorToInt(maxIlośćNaFalę / (3 + (poziomEpoki - 40))));
                }
                break;
            default:
                break;
        }
    }
    ///<summary>Funkcja przetwarza i ustawia generowanie fali wrogów, oraz zwraca informację czy fala została wygenerowana.</summary>
    ///<param name="e">Epoka, której wrogowie mają zostać wygenerowani.</param>
    public bool GenerujSpawn(Epoki e)
    {
        if (ostatniaIlośćWFali >= maxIlośćNaFalę || PomocniczeFunkcje.managerGryScript == null)
        {
            return false;
        }
        else
        {
            List<KonkretnyNPCDynamiczny> możliwiNPC = new List<KonkretnyNPCDynamiczny>();
            NPCClass[] npcs = PomocniczeFunkcje.managerGryScript.PobierzTabliceWrogow;
            byte iloscPunktówSpawnu = 1;
            for (byte i = 0; i < npcs.Length; i++)
            {
                if (npcs[i].epokaNPC == e && npcs[i].poziom <= PomocniczeFunkcje.odblokowanyPoziomEpoki)
                {
                    możliwiNPC.Add((KonkretnyNPCDynamiczny)npcs[i]);
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
            actFala++;
            SpawnujMnie(ref możliwiNPC, 1);
            for (ushort i = 1, j = 1; i < ostatniaIlośćWFali; i++)
            {
                StartCoroutine(SpawnujMnieCorutine(możliwiNPC, j, Random.Range(0, 0.5f)));
                j++;
                if (j > iloscPunktówSpawnu)
                    j = 1;
            }
            PomocniczeFunkcje.mainMenu.WłączWyłączPanel("ui_down", false);
            return true;
        }
    }
    ///<summary>Funkcja zwraca pozycję wroga podczas jego generowania (nadaje jej pewną losowość).</summary>
    ///<param name="pos">Pozycja w świecie punktu spawnu.</param>
    ///<param name="offsets">Możliwe przesunięcie wględem punktu pos.</param>
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
    ///<summary>Metoda ustawia początkowe dane dla wygenerowanego lub aktywowanego wroga.</summary>
    ///<param name="knpcd">Referencja (KonkretnyNPCDynamiczny) ustawianego wroga.</param>
    ///<param name="czyPullowany">Czy wróg został wygenerowany (false), czy przeniesiony i aktywowany (true).</param>
    private void UstawWroga(KonkretnyNPCDynamiczny knpcd, bool czyPullowany = false)
    {
        knpcd.NastawienieNonPlayerCharacter = NastawienieNPC.Wrogie;
        PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek += knpcd.ResetujŚciezkę;
        if (czyPullowany)
        {
            knpcd.UstawWartościPoPoolowaniu();
        }
    }
    ///<summary>Metoda generuje lub aktuwyje jednostkę wroga po określonym czasie.</summary>
    ///<param name="możliwiNPC">Lista możliwych NPC do wygenerowania.</param>
    ///<param name="j">Indeks tablicy punktu spawnu gdzie ma zostać wygenerowany wróg.</param>
    ///<param name="_time">Czas w sec po którym ma zostać wygenerowany wróg.</param>
    private IEnumerator SpawnujMnieCorutine(List<KonkretnyNPCDynamiczny> możliwiNPC, ushort j, float _time)
    {
        yield return new WaitForSeconds(_time);
        SpawnujMnie(ref możliwiNPC, j);
    }
    ///<summary>Metoda generuje lub aktuwyje jednostkę wroga.</summary>
    ///<param name="możliwiNPC">Lista możliwych NPC do wygenerowania.</param>
    ///<param name="j">Indeks tablicy punktu spawnu gdzie ma zostać wygenerowany wróg.</param>
    private void SpawnujMnie(ref List<KonkretnyNPCDynamiczny> możliwiNPC, ushort j)
    {
        bool czyPool = true;
        ushort npcIdx = (ushort)Random.Range(0, możliwiNPC.Count);
        GameObject go = PomocniczeFunkcje.ZwróćOBiektPoolowany(możliwiNPC[npcIdx].name);
        if (go == null)
        {
            czyPool = false;
            go = Instantiate(możliwiNPC[npcIdx].gameObject);
            go.transform.position = OkreślPozucjeZOffsetem(spawnPunkty[j - 1].position, 3.0f);
            go.transform.SetParent(rodzicNPC);
        }
        else
        {
            go.transform.position = OkreślPozucjeZOffsetem(spawnPunkty[j - 1].position, 3.0f);
            go.SetActive(true);
        }
        ManagerGryScript.iloscAktywnychWrogów++;
        KonkretnyNPCDynamiczny knpcd = go.GetComponent<KonkretnyNPCDynamiczny>();
        UstawWroga(knpcd, czyPool);
    }
    ///<summary>Metoda wywołuje ponowną kalkulację ścieżek NavMesha dla jednostek znajdujących się na planszy.</summary>
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
    ///<summary>Metoda kasuje wszystkie wrogie jednostki znajdujące się na planszy.</summary>
    public void UsunWszystkieJednostki()
    {
        for (int i = rodzicNPC.childCount - 1; i >= 0; i--)
        {
            KonkretnyNPCDynamiczny knpcd = rodzicNPC.GetChild(i).GetComponent<KonkretnyNPCDynamiczny>();
            knpcd.WyczyscDaneDynamic(true);
            Destroy(rodzicNPC.GetChild(i).gameObject);
        }
        ResetSpawnedHord();
        PomocniczeFunkcje.mainMenu.WłączWyłączPanel("ui_down", true);
    }
    ///<summary>Zwróć informację o tym, czy została już wygenerowana ostatnia fala na danym poziomie.</summary>
    public bool CzyOstatniaFala()
    {
        if(actFala < iloscFalNaKoncu)
            return false;
        else
            return true;
    }
    ///<summary>Resetuj parametry podczas przeładowania sceny klasy SpawnerHord.</summary>
    public void ResetSpawnedHord()
    {
        iloscFalNaKoncu = 0;
        actFala = 0;
        actualHPBars = 0;
    }
    ///<summary>Załaduj tablicę punktów spawnu wrogów.</summary>
    public void ŁadowanieTablicy()
    {
        spawnPunkty = new Transform[this.transform.childCount];
        for(ushort i = 0; i < this.transform.childCount; i++)
        {
            spawnPunkty[i] = this.transform.GetChild(i).transform;
        }
    }
}
