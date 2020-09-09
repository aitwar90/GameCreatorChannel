﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBudynki : MonoBehaviour
{
    #region Zmienne publiczne
    public GameObject[] wszystkieBudynki;
    public short wybranyBudynek = -1;
    public Transform rodzicBudynkow = null;
    #endregion
    #region Zmienne prywatne
    private Vector3 ostatniaPozycjaKursora = Vector3.zero;
    private Material materialWybranegoBudynku = null;
    private Color kolorOrginału;
    private Collider kolider = null;
    private KonkretnyNPCStatyczny knpcs = null;
    #endregion

    void Awake()
    {
        if (rodzicBudynkow == null)
        {
            GameObject go = new GameObject("Rodzic Budynków");
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            rodzicBudynkow = go.transform;
        }
    }
    void Update()
    {
        if (wybranyBudynek != -1)
        {
            if (Input.GetMouseButtonDown(2))
            {
                ResetWybranegoObiektu();
            }
            else
            {
                Vector3 posClick = PomocniczeFunkcje.OkreślPozycjęŚwiataKursora(ostatniaPozycjaKursora);
                wszystkieBudynki[wybranyBudynek].transform.position = posClick;
#if UNITY_STANDALONE
                float numer = Input.GetAxisRaw("Mouse ScrollWheel");
                if (numer != 0)
                {
                    numer = (numer < 0) ? -1 : 1;
                    Vector3 vet = wszystkieBudynki[wybranyBudynek].transform.rotation.eulerAngles;
                    vet.y += numer * 45;
                    wszystkieBudynki[wybranyBudynek].transform.rotation = Quaternion.Euler(vet);
                }
#endif
                ostatniaPozycjaKursora = posClick;
            }
        }
        if (wybranyBudynek == -1 || (wybranyBudynek != -1 && CzyMogęPostawićBudynek()))
        {
#if UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
            {
                LogikaIZatwierdzenieBudynku();
            }
#endif
#if UNITY_ANDROID
            if(Input.touchCout > 0 && wybranyBudynek != null)
            {
                LogikaIZatwierdzenieBudynku();
            }
#endif
        }
    }
    public void PostawBudynek(ref GameObject obiektDoRespawnu, Vector3 pos, Quaternion rotation)
    {
        obiektDoRespawnu = Instantiate(obiektDoRespawnu, pos, rotation);
        materialWybranegoBudynku = obiektDoRespawnu.GetComponent<Renderer>().material;
        if (materialWybranegoBudynku != null)
        {
            kolorOrginału = materialWybranegoBudynku.color;
            materialWybranegoBudynku.color = Color.red;
        }
        knpcs = obiektDoRespawnu.GetComponent<KonkretnyNPCStatyczny>();
        kolider = obiektDoRespawnu.GetComponent<Collider>();
        kolider.isTrigger = true;
    }
    private void LogikaIZatwierdzenieBudynku()
    {
        Vector3 posClick = PomocniczeFunkcje.OkreślPozycjęŚwiataKursora(ostatniaPozycjaKursora);
        if (wybranyBudynek != -1)
        {
            if (wybranyBudynek != -1)
            {
                //Sprawdź czy można postawić budynek
                wszystkieBudynki[wybranyBudynek].transform.position = posClick;
                kolider.isTrigger = false;
                kolider = null;
                materialWybranegoBudynku.color = kolorOrginału;
                materialWybranegoBudynku = null;
                knpcs = null;
                wszystkieBudynki[wybranyBudynek].transform.SetParent(rodzicBudynkow);
                wybranyBudynek = -1;
            }
        }
        else
        {
            wybranyBudynek = 0;
            PostawBudynek(ref wszystkieBudynki[wybranyBudynek], posClick, Quaternion.identity);
        }
        ostatniaPozycjaKursora = posClick;
    }
    private bool CzyMogęPostawićBudynek()
    {
        if (!knpcs.pozwalamNaBudowe)
        {
            materialWybranegoBudynku.color = Color.red;
            return false;
        }
        if (materialWybranegoBudynku != null)
            materialWybranegoBudynku.color = Color.green;
        return true;
    }
    private void ResetWybranegoObiektu()    //Resetuje ustawienie wybranego budynku
    {
        materialWybranegoBudynku = null;
        kolider = null;
        knpcs = null;
        DestroyImmediate(wszystkieBudynki[wybranyBudynek]);
        wybranyBudynek = -1;
    }
}
