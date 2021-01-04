using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObslugaScenScript : MonoBehaviour
{
    [Header("Sceny muszą mieć nazwę Scena_x_y1_y2_y3 gdzie x oznacza indeks sceny danej epoki a y epoki do której scena ma przynależeć")]
    public byte indeksAktualnejSceny = 0;
    public byte ZwróćIndeksScenyPoEpoce(Epoki e)
    {
        string doPorowniania = e.ToString()+".unity";
        List<int> wszystkieSceny = new List<int>();
        for (byte i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string ścieżka = SceneUtility.GetScenePathByBuildIndex(i);
            Debug.Log("Ścieżka = "+ścieżka);
            string[] nSceny = ścieżka.Split('_');
            for (byte j = 2; j < nSceny.Length; j++)
            {
                if (nSceny[j] == doPorowniania)
                {
                    wszystkieSceny.Add(i);
                }
            }
        }
        sbyte t = (sbyte)Random.Range(0, wszystkieSceny.Count);
        indeksAktualnejSceny = (byte)wszystkieSceny[t];
        return indeksAktualnejSceny;
    }
}
