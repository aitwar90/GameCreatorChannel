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
    public ushort maxIlośćNaFalę = 30;
    public byte iloscPunktówSpawnu = 1;
    public static byte actualHPBars = 0;
    public byte aktualnaIloscHPBarów = 0;
    public Transform rodzicNPC = null;

    public void GenerujSpawn(Epoki e)
    {
        List<GameObject> możliwiNPC = new List<GameObject>();
        for (byte i = 0; i < wszystkieRodzajeWrogichJednostek.Length; i++)
        {
            if (wszystkieRodzajeWrogichJednostek[i].epokaNPC == e)
            {
                możliwiNPC.Add(wszystkieRodzajeWrogichJednostek[i].gameObject);
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
                    ostatniaIlośćWFali += 3;
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
            ushort npcIdx = (ushort)Random.Range(0, możliwiNPC.Count-1);
            GameObject go = Instantiate(możliwiNPC[npcIdx].gameObject, OkreślPozucjeZOffsetem(spawnPunkty[j-1].position, 2.0f), Quaternion.identity);
            ManagerGryScript.iloscAktywnychWrogów++;
            go.transform.SetParent(rodzicNPC);
            j++;
            if(j > iloscPunktówSpawnu)
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
        if(rodzicNPC == null)
        {
            GameObject go = new GameObject("rodzicNPC");
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            rodzicNPC = go.transform;
        }
    }
    void Update()
    {
        if(SpawnerHord.actualHPBars != 0)
            SpawnerHord.actualHPBars = 0;
    }

}
