﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnBudynki : MonoBehaviour
{
    #region Zmienne publiczne
    public GameObject[] wszystkieBudynki;
    //UI _ Canvas
    public Dropdown dropdawn;
    public Sprite lockDropdownImage;
    public Sprite enableLockDropdownImage;
    public GameObject aktualnyObiekt = null;
    public short zablokowanyBudynekIndex = -1;
    #endregion
    #region Zmienne prywatne
    private Material materialWybranegoBudynku = null;
    private Color kolorOrginału;
    private KonkretnyNPCStatyczny knpcs = null;
    private Vector3 ostatniaPozycjaKursora = Vector3.zero;
    private Vector3 posClick = Vector3.zero;
    private Transform rodzicBudynkow = null;
    private StrukturaBudynkuWTab[] czyBudynekZablokowany = null;
    private bool kliknieteUI = false;
    #endregion
    #region Getery i Setery
    public Transform RodzicBudynków
    {
        get
        {
            return rodzicBudynkow;
        }
    }
    public bool KlikUI
    {
        get
        {
            return kliknieteUI;
        }
        set
        {
            kliknieteUI = value;
        }
    }
    #endregion
    void Start()
    {
        if (rodzicBudynkow == null)
        {
            GameObject go = new GameObject("Rodzic Budynków");
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            rodzicBudynkow = go.transform;
        }
        List<string> wszystkieBudynkiList = new List<string>();
        List<StrukturaBudynkuWTab> sbwt = new List<StrukturaBudynkuWTab>();
        wszystkieBudynkiList.Add("None");
        sbyte idxActEpoki = (sbyte)PomocniczeFunkcje.managerGryScript.aktualnaEpoka;
        if (idxActEpoki > 0)
        {
            for (byte i = 0; i < wszystkieBudynki.Length; i++)
            {
                KonkretnyNPCStatyczny knpcs = wszystkieBudynki[i].GetComponent<KonkretnyNPCStatyczny>();
                byte budynekEpoki = (byte)knpcs.epokaNPC;
                if (budynekEpoki == idxActEpoki || budynekEpoki == idxActEpoki - 1)
                {
                    StrukturaBudynkuWTab tt = new StrukturaBudynkuWTab(knpcs.Zablokowany, i);
                    sbwt.Add(tt);
                    wszystkieBudynkiList.Add(knpcs.nazwa);
                }
            }
        }
        else
        {
            Debug.Log("SpawnBudynki 83: Nie ustalono epoki");
        }
        czyBudynekZablokowany = sbwt.ToArray();
        this.dropdawn.AddOptions(wszystkieBudynkiList);
        for (ushort i = 0; i < czyBudynekZablokowany.Length; i++)
        {
            if (czyBudynekZablokowany[i].czyZablokowany)
            {
                this.dropdawn.options[i + 1].image = lockDropdownImage;
                this.dropdawn.options[i + 1].text = this.dropdawn.options[i + 1].text + " LOCK";
            }
        }
        PomocniczeFunkcje.managerGryScript.ZmianaJęzyka((byte)PomocniczeFunkcje.mainMenu.lastIdxJezyka);
    }
    void Update()
    {
        if (aktualnyObiekt != null && !MainMenu.czyMenuEnable)
        {
            posClick = PomocniczeFunkcje.OkreślPozycjęŚwiataKursora(ostatniaPozycjaKursora, ref kliknieteUI);
        }
    }
    void LateUpdate()
    {
        if (aktualnyObiekt != null && !MainMenu.czyMenuEnable)
        {
            if (Input.GetMouseButton(2))
            {
                ResetWybranegoObiektu();
                return;
            }
            if (posClick != ostatniaPozycjaKursora)
            {
                aktualnyObiekt.transform.position = posClick;
                ostatniaPozycjaKursora = posClick;
            }
            if (CzyMogęPostawićBudynek(posClick))
            {
#if UNITY_STANDALONE
            ObsluzMysz();
#endif
#if UNITY_ANDROID
                if (Input.mousePresent)
                {
                    ObsluzMysz();
                }
                else
                {
                    ObsluzTouchPad();
                }
#endif
            }
        }
    }
    public void OdblokujBudynek(bool czyOdblokowywuje = false)
    {
        if (czyOdblokowywuje && zablokowanyBudynekIndex > -1)
        {
            KonkretnyNPCStatyczny statycznyBudynekDoOdbl = wszystkieBudynki[czyBudynekZablokowany[zablokowanyBudynekIndex].indexBudynku].GetComponent<KonkretnyNPCStatyczny>();
            statycznyBudynekDoOdbl.Zablokowany = false;
            ManagerGryScript.iloscCoinów -= statycznyBudynekDoOdbl.kosztBadania;
            czyBudynekZablokowany[zablokowanyBudynekIndex].czyZablokowany = false;
            this.dropdawn.options[zablokowanyBudynekIndex + 1].image = enableLockDropdownImage;
            this.dropdawn.options[zablokowanyBudynekIndex + 1].text = statycznyBudynekDoOdbl.nazwa;
            PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
        }
        zablokowanyBudynekIndex = -1;
        ResetWybranegoObiektu();
    }
    private void ObsluzMysz()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ZatwierdźBudynekWindows();
        }
        float numer = Input.GetAxisRaw("Mouse ScrollWheel");
        ObróćBudynek(numer);
    }
    private void ObsluzTouchPad()
    {
        if (Input.touchCount == 1 && CzyMogęPostawićBudynek(aktualnyObiekt.transform.position))
        {
            Touch t = Input.GetTouch(0);
            if ((t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved) && t.tapCount == 1)
            {
                PrzesuwanieAktualnegoObiektu();
            }
            else if (t.tapCount == 2)
            {
                ZatwierdźBudynekAndroid();
            }
        }
    }
    public void PostawBudynek(ref GameObject obiektDoRespawnu, Vector3 pos, Quaternion rotation)
    {
        aktualnyObiekt = Instantiate(obiektDoRespawnu, pos, rotation);
        materialWybranegoBudynku = aktualnyObiekt.GetComponent<Renderer>().material;
        if (materialWybranegoBudynku != null)
        {
            kolorOrginału = materialWybranegoBudynku.color;
            materialWybranegoBudynku.color = Color.red;
        }
        knpcs = aktualnyObiekt.GetComponent<KonkretnyNPCStatyczny>();
        if (knpcs.kosztJednostki > ManagerGryScript.iloscCoinów || knpcs.Zablokowany)
        {
            ResetWybranegoObiektu();
            return;
        }
        PomocniczeFunkcje.mainMenu.UstawPrzyciskObrotu(true);
    }
    private void ZatwierdźBudynekWindows()
    {
        if (TypBudynku.Mur == knpcs.typBudynku)
        {
            posClick = WyrównajSpawn(posClick);
        }
        if (!kliknieteUI)
        {
            aktualnyObiekt.transform.position = posClick;
            HelperZatwierdzenieBudynku();
        }
    }
    private void ZatwierdźBudynekAndroid()
    {
        HelperZatwierdzenieBudynku();
    }
    private void PrzesuwanieAktualnegoObiektu()
    {
        if (TypBudynku.Mur == knpcs.typBudynku)
        {
            posClick = WyrównajSpawn(posClick);
        }
    }
    private void HelperZatwierdzenieBudynku()
    {
        aktualnyObiekt.tag = "Budynek";
        aktualnyObiekt.transform.SetParent(rodzicBudynkow);
        //Ustawienie skryptu KonkretnyNPCStatyuczny
        knpcs.InicjacjaBudynku();
        PomocniczeFunkcje.DodajDoDrzewaPozycji(knpcs, ref PomocniczeFunkcje.korzeńDrzewaPozycji);
        //Ustawiam materiał
        materialWybranegoBudynku.color = kolorOrginału;
        //Teraz nalezy umieścić budynek w odpowiednim miejscu tablicy PomocniczeFunkcje.tablicaWież
        short[] temp = PomocniczeFunkcje.ZwrócIndeksyWTablicy(posClick);
        byte s = (byte)Mathf.CeilToInt(knpcs.zasięgAtaku / PomocniczeFunkcje.distXZ);
        int k = 0;
        while (s > 0)  //Ustawienie budynku na planszy aby wieża mogła stwierdzić że może atakować
        {
            for (short x = (short)(temp[0] - k); x <= temp[0] + k; x++)
            {
                for (short z = (short)(temp[1] - k); z <= temp[1] + k; z++)
                {
                    if (x > -1 && x < 19 && z > -1 && z < 19)
                    {
                        if (x == temp[0] - k || x == temp[0] + k ||
                        z == temp[1] - k || z == temp[1] + k)
                        {
                            if (PomocniczeFunkcje.tablicaWież[x, z] == null)
                            {
                                PomocniczeFunkcje.tablicaWież[x, z] = new List<InformacjeDlaPolWież>();
                            }
                            List<byte> tmp = new List<byte>();
                            //string ss = "("+s.ToString()+")";
                            if (s == 1)
                            {
                                if (x == temp[0] - k)       //-X
                                {
                                    //ss = ss+" 0";
                                    tmp.Add(0);
                                }
                                else if (x == temp[0] + k)  //+X
                                {
                                    //ss = ss+" 1";
                                    tmp.Add(1);
                                }
                                if (z == temp[1] - k)       //-Z
                                {
                                    //ss = ss+" 2";
                                    tmp.Add(2);
                                }
                                else if (z == temp[1] + k)  //+Z
                                {
                                    //ss = ss+" 3";
                                    tmp.Add(3);
                                }
                            }
                            PomocniczeFunkcje.tablicaWież[x, z].Add(new InformacjeDlaPolWież(s, knpcs,
                            (tmp.Count == 0) ? null : tmp.ToArray()));
                            /*
                            GameObject go = new GameObject(ss);
                            float fx = PomocniczeFunkcje.aktualneGranicaTab + (x*PomocniczeFunkcje.distXZ);
                            float fz = PomocniczeFunkcje.aktualneGranicaTab + (z*PomocniczeFunkcje.distXZ);
                            go.transform.position = new Vector3(fx, 0.0f, fz);
                            */
                        }
                    }
                }
            }
            k++;
            s--;
        }
        knpcs.maksymalneŻycie += (short)(PomocniczeFunkcje.managerGryScript.hpIdx*10);
        /*
        knpcs.modyfikatorZadawanychObrażeń += PomocniczeFunkcje.managerGryScript.atkIdx*0.1f;
        knpcs.modyfikatorOtrzymywanychObrażeń += PomocniczeFunkcje.managerGryScript.defIdx*0.1f;
        */
        knpcs.modyfikatorZadawanychObrażeń = PomocniczeFunkcje.WyliczModyfikatorObrazeń(knpcs.modyfikatorZadawanychObrażeń, PomocniczeFunkcje.managerGryScript.atkIdx);
        knpcs.modyfikatorZadawanychObrażeń = PomocniczeFunkcje.WyliczModyfikatorObrazeń(knpcs.modyfikatorOtrzymywanychObrażeń, PomocniczeFunkcje.managerGryScript.defIdx);
        if (PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek != null)
            PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek(knpcs);
        //Pobranie coinów za postawiony budynek
        ManagerGryScript.iloscCoinów -= knpcs.kosztJednostki;
        PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
        //Debug.Log("Postawiłem budynek na X = "+temp[0]+" Z = "+temp[1]);
        // Kasowanie ustawień potrzebnych do postawienia budynku
        materialWybranegoBudynku = null;
        knpcs = null;
        aktualnyObiekt = null;
        dropdawn.value = 0;
        PomocniczeFunkcje.mainMenu.UstawPrzyciskObrotu(false);
    }
    public void WybierzBudynekDoPostawienia()  //Wybór obiektu budynku do postawienia
    {
        short index = (short)(this.dropdawn.value - 1);
        if (index < 0)
        {
            ResetWybranegoObiektu();
            return;
        }
        if (index > -1 && aktualnyObiekt == null)
        {
            if (!czyBudynekZablokowany[index].czyZablokowany)
            {
                PostawBudynek(ref wszystkieBudynki[czyBudynekZablokowany[index].indexBudynku], posClick, Quaternion.identity);
                ostatniaPozycjaKursora = posClick;
            }
            else
            {
                zablokowanyBudynekIndex = index;
                Debug.Log("Dany budynek należy kupić");
                //Wyświetl przyciski kupna
                PomocniczeFunkcje.mainMenu.WłWylPrzyciskiKupna(true);
            }
        }
    }
    private bool CzyMogęPostawićBudynek(Vector3 sugerowanaPozycja)
    {
        if (knpcs.kosztJednostki > ManagerGryScript.iloscCoinów || kliknieteUI)
        {
            return false;
        }
        KonkretnyNPCStatyczny najbliższyBudynek = PomocniczeFunkcje.WyszukajWDrzewie(ref PomocniczeFunkcje.korzeńDrzewaPozycji, sugerowanaPozycja) as KonkretnyNPCStatyczny;
        if (najbliższyBudynek == null)
        {
            ResetWybranegoObiektu();
            return false;
        }
        if (Mathf.Abs(sugerowanaPozycja.x - najbliższyBudynek.transform.position.x) < najbliższyBudynek.granicaX + knpcs.granicaX &&
        Mathf.Abs(sugerowanaPozycja.z - najbliższyBudynek.transform.position.z) < najbliższyBudynek.granicaZ + knpcs.granicaZ)
        {
            if (materialWybranegoBudynku.color != Color.red)
            {
                materialWybranegoBudynku.color = Color.red;
            }
            return false;
        }
        if (materialWybranegoBudynku != null && materialWybranegoBudynku.color != Color.green)
            materialWybranegoBudynku.color = Color.green;
        return true;
    }
    private void ResetWybranegoObiektu()    //Resetuje ustawienie wybranego budynku
    {
        materialWybranegoBudynku = null;
        knpcs = null;
        Destroy(aktualnyObiekt);
        aktualnyObiekt = null;
        dropdawn.value = 0;
        PomocniczeFunkcje.mainMenu.UstawPrzyciskObrotu(false);
    }
    private Vector3 WyrównajSpawn(Vector3 sugerowanePolozenie)
    {
        sugerowanePolozenie.x = Mathf.RoundToInt(sugerowanePolozenie.x);
        sugerowanePolozenie.z = Mathf.RoundToInt(sugerowanePolozenie.z);
        return sugerowanePolozenie;
    }
    public void DestroyBuildings()
    {
        if (aktualnyObiekt != null)
        {
            ResetWybranegoObiektu();
        }
        for (int i = rodzicBudynkow.childCount - 1; i >= 0; i--)
        {
            Destroy(rodzicBudynkow.GetChild(i).gameObject);
        }
    }
    public void ZróbListęDropdownBudynków()
    {
        List<string> s = new List<string>();
        s.Add("None");
        for (byte i = 0; i < czyBudynekZablokowany.Length; i++)
        {
            if (czyBudynekZablokowany[i].czyZablokowany)
            {
                s.Add(wszystkieBudynki[czyBudynekZablokowany[i].indexBudynku].GetComponent<KonkretnyNPCStatyczny>().nazwa + " LOCK");
            }
            else
            {
                s.Add(wszystkieBudynki[czyBudynekZablokowany[i].indexBudynku].GetComponent<KonkretnyNPCStatyczny>().nazwa);
            }
        }
        this.dropdawn.ClearOptions();
        this.dropdawn.AddOptions(s);
    }
    public void ObróćBudynek(float numer = 2)   //numer to parametr skoków o ile ma obrócić się budynek
    {
        if (numer != 0)
        {
            numer = (numer < 0) ? (-1)*numer : numer;
            Vector3 vet = aktualnyObiekt.transform.rotation.eulerAngles;
            vet.y += numer * 45;
            aktualnyObiekt.transform.rotation = Quaternion.Euler(vet);
        }
    }
}
