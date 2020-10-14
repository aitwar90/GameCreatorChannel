using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOnEnableGround : MonoBehaviour
{
    public short X;
    public short Z;
    public KonkretnyNPCStatyczny[] wieżemająceZasięgNaPolu;

    public void PobierzInformacje()
    {
        List<KonkretnyNPCStatyczny> tmp = new List<KonkretnyNPCStatyczny>();
        for (ushort i = 0; i < PomocniczeFunkcje.tablicaWież[X, Z].Count; i++)
        {
            tmp.Add(PomocniczeFunkcje.tablicaWież[X, Z][i].wieża);
        }
        wieżemająceZasięgNaPolu = tmp.ToArray();
    }
}
