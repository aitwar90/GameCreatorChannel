using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class SpawnBudynki : MonoBehaviour
{
    #region Zmienne publiczne
    [Header("Uwaga automatyczne ładowanie obiektów do tablicy wymaga: \n aby był dodany obrazek do budynku")]
    public GameObject[] wszystkieBudynki;
    public GameObject aktualnyObiekt = null;
    public short zablokowanyBudynekIndex = -1;
    #endregion
    #region Zmienne prywatne
    private MaterialyZKolorami[] materialWybranegoBudynku = null;
    private Color kolorOrginału;
    private KonkretnyNPCStatyczny knpcs = null;
    private Vector3 ostatniaPozycjaKursora = Vector3.zero;
    private Vector3 posClick = Vector3.zero;
    private Transform rodzicBudynkow = null;
    public StrukturaBudynkuWTab[] czyBudynekZablokowany = null;
    public short aktualnieWybranyIndeksObiektuTabZablokowany = -1;
    private bool kliknieteUI = false;
    public Vector2 posCursor;
    private byte aktualnyStanKoloru = 255;  //255 domyślny, 0 - czerwony, 1 - zielony
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
    public short AktIdxBudZab
    {
        get
        {
            return aktualnieWybranyIndeksObiektuTabZablokowany;
        }
        set
        {
            aktualnieWybranyIndeksObiektuTabZablokowany = value;
        }
    }
    public ref StrukturaBudynkuWTab[] ZablokowaneBudynki
    {
        get
        {
            return ref czyBudynekZablokowany;
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
    }
    public void InicjacjaPaneluBudynków()
    {
        sbyte idxActEpoki = (sbyte)PomocniczeFunkcje.managerGryScript.aktualnaEpoka;
        byte ape = PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki;
        //Reset paneli

        //Stwórz listy
        List<StrukturaBudynkuWTab> allsbwt = new List<StrukturaBudynkuWTab>();
        for (ushort i = 0; i < wszystkieBudynki.Length; i++)
        {
            KonkretnyNPCStatyczny knpcs = wszystkieBudynki[i].GetComponent<KonkretnyNPCStatyczny>();
            byte budynekEpoki = (byte)knpcs.epokaNPC;
            if (budynekEpoki == idxActEpoki || budynekEpoki == idxActEpoki - 1)
            {
                StrukturaBudynkuWTab tt = new StrukturaBudynkuWTab((ape != 255) ? knpcs.Zablokowany : knpcs.blokowany, i);
                allsbwt.Add(tt);
            }
        }
        czyBudynekZablokowany = allsbwt.ToArray();
    }
    void Update()
    {
        if (aktualnyObiekt != null && PomocniczeFunkcje.mainMenu.CzyMogePrzesuwaćKamere())
        {
            posClick = PomocniczeFunkcje.OkreślPozycjęŚwiataKursora(ostatniaPozycjaKursora, ref kliknieteUI, ref posCursor);
        }
    }
    void LateUpdate()
    {
        if (aktualnyObiekt != null && PomocniczeFunkcje.mainMenu.CzyMogePrzesuwaćKamere())
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
#if UNITY_ANDROID || UNITY_IOS
                if (Input.mousePresent && !ManagerGryScript.odpalamNaUnityRemote)
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
        if (czyOdblokowywuje && aktualnieWybranyIndeksObiektuTabZablokowany > -1)
        {
            KonkretnyNPCStatyczny statycznyBudynekDoOdbl = wszystkieBudynki[czyBudynekZablokowany[aktualnieWybranyIndeksObiektuTabZablokowany].indexBudynku].GetComponent<KonkretnyNPCStatyczny>();
            statycznyBudynekDoOdbl.Zablokowany = false;
            ManagerGryScript.iloscCoinów -= statycznyBudynekDoOdbl.kosztBadania;
            PomocniczeFunkcje.managerGryScript.DodajDoWartościStatystyk(0, -statycznyBudynekDoOdbl.kosztBadania);
            czyBudynekZablokowany[aktualnieWybranyIndeksObiektuTabZablokowany].czyZablokowany = false;
            PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
        }
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
        else if (Input.touchCount == 2)  //Jeśli jest aktualny budynek do postawienia to kliknięcie 2 przycisków na ekran spowoduje jego reset
        {
            ResetWybranegoObiektu();
        }
    }
    public bool KliknietyBudynekWPanelu(short tabWTablicy)
    {
        bool f = false;
        aktualnieWybranyIndeksObiektuTabZablokowany = tabWTablicy;
        if (czyBudynekZablokowany[aktualnieWybranyIndeksObiektuTabZablokowany].czyZablokowany)
        {
            if (PomocniczeFunkcje.mainMenu.stawiajBudynek.interactable)
                PomocniczeFunkcje.mainMenu.stawiajBudynek.interactable = false;
            //Odpal przycisk kupna
            if (ManagerGryScript.iloscCoinów >= wszystkieBudynki[czyBudynekZablokowany[aktualnieWybranyIndeksObiektuTabZablokowany].indexBudynku].GetComponent<KonkretnyNPCStatyczny>().kosztBadania)
            {
                PomocniczeFunkcje.mainMenu.kup.interactable = true;
            }
        }
        else
        {
            PomocniczeFunkcje.mainMenu.kup.interactable = false;
            //Odpal przycisk budowy
            int koszt = wszystkieBudynki[czyBudynekZablokowany[aktualnieWybranyIndeksObiektuTabZablokowany].indexBudynku].GetComponent<KonkretnyNPCStatyczny>().kosztJednostki;
            if (ManagerGryScript.iloscCoinów >= koszt)
            {
                PomocniczeFunkcje.mainMenu.stawiajBudynek.interactable = true;
                if(ManagerGryScript.iloscCoinów - koszt - koszt > koszt)
                    f = true;
            }
            else
            {
                PomocniczeFunkcje.mainMenu.stawiajBudynek.interactable = false;
            }
        }
        KonkretnyNPCStatyczny knpcs = wszystkieBudynki[czyBudynekZablokowany[aktualnieWybranyIndeksObiektuTabZablokowany].indexBudynku].GetComponent<KonkretnyNPCStatyczny>();
        knpcs.UstawPanel(Vector2.negativeInfinity);
        return f;
    }
    public void PostawBudynek(ref GameObject obiektDoRespawnu, Vector3 pos, Quaternion rotation)
    {
        aktualnyObiekt = Instantiate(obiektDoRespawnu, pos, rotation);
        Renderer[] mats = aktualnyObiekt.GetComponentsInChildren<Renderer>();
        materialWybranegoBudynku = new MaterialyZKolorami[mats.Length];
        for (byte i = 0; i < mats.Length; i++)
        {
            materialWybranegoBudynku[i] = new MaterialyZKolorami(mats[i].material, mats[i].material.color);
        }
        if (materialWybranegoBudynku != null)
        {
            PodmieńNaCzerwony();
        }
        knpcs = aktualnyObiekt.GetComponent<KonkretnyNPCStatyczny>();

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
    public void ZatwierdźBudynekAndroid()
    {
        HelperZatwierdzenieBudynku();
    }
    private void PrzesuwanieAktualnegoObiektu()
    {
        if (TypBudynku.Mur == knpcs.typBudynku)
        {
            posClick = WyrównajSpawn(posClick);
        }
        PomocniczeFunkcje.mainMenu.UstawPosPanelBudowyBudynków(posCursor);
        Debug.Log("Ustawiam posClick na "+posCursor);
    }
    private void HelperZatwierdzenieBudynku()
    {
        aktualnyObiekt.tag = "Budynek";
        aktualnyObiekt.transform.SetParent(rodzicBudynkow);
        //Ustawienie skryptu KonkretnyNPCStatyuczny
        knpcs.InicjacjaBudynku();
        PomocniczeFunkcje.DodajDoDrzewaPozycji(knpcs, ref PomocniczeFunkcje.korzeńDrzewaPozycji);
        //Ustawiam materiał
        PodmieńNaOrginalny();
        //Teraz nalezy umieścić budynek w odpowiednim miejscu tablicy PomocniczeFunkcje.tablicaWież
        short[] temp = PomocniczeFunkcje.ZwrócIndeksyWTablicy(posClick.x, posClick.z);
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
        if (PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek != null)
            PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek(knpcs);
        //Pobranie coinów za postawiony budynek
        ManagerGryScript.iloscCoinów -= knpcs.kosztJednostki;
        PomocniczeFunkcje.managerGryScript.DodajDoWartościStatystyk(2, -knpcs.kosztJednostki);
        PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
        //Debug.Log("Postawiłem budynek na X = "+temp[0]+" Z = "+temp[1]);
        // Kasowanie ustawień potrzebnych do postawienia budynku
        materialWybranegoBudynku = null;
        aktualnieWybranyIndeksObiektuTabZablokowany = -1;
        knpcs = null;
        aktualnyObiekt.GetComponent<Collider>().enabled = true;
        aktualnyObiekt = null;
        PomocniczeFunkcje.mainMenu.UstawPrzyciskObrotu(false);
        if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki == 255)
        {
            if (ManagerSamouczekScript.mssInstance.CzyZgadzaSięIDXGłówny(6) ||
            ManagerSamouczekScript.mssInstance.CzyZgadzaSięIDXGłówny(10))
            {
                ManagerSamouczekScript.mssInstance.ZmiennaPomocnicza = -3;
            }
        }
    }
    public void WybierzBudynekDoPostawienia()  //Wybór obiektu budynku do postawienia
    {
        if (aktualnieWybranyIndeksObiektuTabZablokowany < 0)
        {
            ResetWybranegoObiektu();
            return;
        }
        if (aktualnieWybranyIndeksObiektuTabZablokowany > -1 && aktualnyObiekt == null)
        {
            if (!czyBudynekZablokowany[aktualnieWybranyIndeksObiektuTabZablokowany].czyZablokowany)
            {
                PostawBudynek(ref wszystkieBudynki[czyBudynekZablokowany[aktualnieWybranyIndeksObiektuTabZablokowany].indexBudynku], posClick, Quaternion.identity);
                ostatniaPozycjaKursora = posClick;
            }
            else
            {
                zablokowanyBudynekIndex = (short)aktualnieWybranyIndeksObiektuTabZablokowany;
                Debug.Log("Dany budynek należy kupić");
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
            PodmieńNaCzerwony();
            return false;
        }
        PodmieńNaZielony();
        return true;
    }
    public void ResetWybranegoObiektu()    //Resetuje ustawienie wybranego budynku
    {
        materialWybranegoBudynku = null;
        knpcs = null;
        Destroy(aktualnyObiekt);
        aktualnyObiekt = null;
        aktualnieWybranyIndeksObiektuTabZablokowany = -1;
        PomocniczeFunkcje.mainMenu.UstawPrzyciskObrotu(false);
        PomocniczeFunkcje.mainMenu.DeaktywujPanelBudowyBudynków();
    }
    private Vector3 WyrównajSpawn(Vector3 sugerowanePolozenie)
    {
        //NOWE
        /*
        float t = sugerowanePolozenie.x - (byte)sugerowanePolozenie.x;
        if(t < .25f)
            sugerowanePolozenie.x = (byte)sugerowanePolozenie.x;
        else if(t < .75f)
            sugerowanePolozenie.x = (byte)sugerowanePolozenie.x + 0.5f;
        else
            sugerowanePolozenie.x = (byte)sugerowanePolozenie.x + 1;
        t = sugerowanePolozenie.z - (byte)sugerowanePolozenie.z;
        if(t < .25f)
            sugerowanePolozenie.z = (byte)sugerowanePolozenie.z;
        else if(t < .75f)
            sugerowanePolozenie.z = (byte)sugerowanePolozenie.z + 0.5f;
        else
            sugerowanePolozenie.z = (byte)sugerowanePolozenie.z + 1;
        */
        //STARE
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
    public void ObróćBudynek(float numer = 2)   //numer to parametr skoków o ile ma obrócić się budynek
    {
        if (numer != 0)
        {
            numer = (numer < 0) ? (-1) * numer : numer;
            Vector3 vet = aktualnyObiekt.transform.rotation.eulerAngles;
            vet.y += numer * 45;
            if (numer != 2)
            {
                ushort actY = (ushort)(vet.y / 45);
                if (actY % 2 == 1 && actY != (ushort)(vet.y / 45))
                {
                    float tmpGranicaX = knpcs.granicaX;
                    knpcs.granicaX = knpcs.granicaZ;
                    knpcs.granicaZ = tmpGranicaX;
                }
            }
            else
            {
                float tmpGranicaX = knpcs.granicaX;
                knpcs.granicaX = knpcs.granicaZ;
                knpcs.granicaZ = tmpGranicaX;
            }
            //
            aktualnyObiekt.transform.rotation = Quaternion.Euler(vet);
            //Zamiana granic
        }
    }
    public StrukturaBudynkuWTab ZwrócMiStruktureBudynku(short idx)
    {
        if (idx < 0 || idx >= czyBudynekZablokowany.Length)
            return null;
        else
            return czyBudynekZablokowany[idx];
    }
    private void PodmieńNaCzerwony()
    {
        if (aktualnyStanKoloru == 0 || aktualnyObiekt == null)
            return;
        for (byte i = 0; i < materialWybranegoBudynku.Length; i++)
        {
            materialWybranegoBudynku[i].PodmienKolor(0);
        }
        aktualnyStanKoloru = 0;
        PomocniczeFunkcje.mainMenu.DeaktywujPanelBudowyBudynków();
    }
    private void PodmieńNaZielony()
    {
        if (aktualnyStanKoloru == 1 || aktualnyObiekt == null)
            return;
        for (byte i = 0; i < materialWybranegoBudynku.Length; i++)
        {
            materialWybranegoBudynku[i].PodmienKolor(1);
        }
        aktualnyStanKoloru = 1;
        PomocniczeFunkcje.mainMenu.OdpalPanelBudyowyBudynków(knpcs.nazwa, posCursor);
    }
    private void PodmieńNaOrginalny()
    {
        if (aktualnyStanKoloru == 255 || aktualnyObiekt == null)
            return;
        for (byte i = 0; i < materialWybranegoBudynku.Length; i++)
        {
            materialWybranegoBudynku[i].PodmienKolor();
        }
        aktualnyStanKoloru = 255;
        PomocniczeFunkcje.mainMenu.DeaktywujPanelBudowyBudynków();
    }
    public void OdpalPanelBudowyBudynków()
    {
        
    }
    #region Obsługa Custom Edytora
#if UNITY_EDITOR
    public void SortujPoEpoceIPoziomie()
    {
        List<KonkretnyNPCStatyczny> wieże = new List<KonkretnyNPCStatyczny>();
        List<KonkretnyNPCStatyczny> murki = new List<KonkretnyNPCStatyczny>();
        List<KonkretnyNPCStatyczny> reszta = new List<KonkretnyNPCStatyczny>();
        for (ushort i = 0; i < this.wszystkieBudynki.Length; i++)
        {
            KonkretnyNPCStatyczny knpcs = wszystkieBudynki[i].GetComponent<KonkretnyNPCStatyczny>();
            if (knpcs.typBudynku == TypBudynku.Wieża)
            {
                wieże.Add(knpcs);
            }
            else if (knpcs.typBudynku == TypBudynku.Mur)
            {
                murki.Add(knpcs);
            }
            else
            {
                reszta.Add(knpcs);
            }
        }
        KonkretnyNPCStatyczny[] wieżki = wieże.ToArray();
        KonkretnyNPCStatyczny[] mury = murki.ToArray();
        KonkretnyNPCStatyczny[] resztki = reszta.ToArray();
        if (wieżki.Length > 0)
        {
            QuickSort(ref wieżki, 0, wieżki.Length - 1);
        }
        if (mury.Length > 0)
        {
            QuickSort(ref mury, 0, mury.Length - 1);
        }
        if (resztki.Length > 0)
        {
            QuickSort(ref resztki, 0, resztki.Length - 1);
        }
        //Scalenie tablic
        GameObject[] allBudynkiPoSorcie = new GameObject[wieżki.Length + mury.Length + resztki.Length];
        for (ushort i = 0, j = 0, k = 0; i < allBudynkiPoSorcie.Length; i++)
        {
            if (k == 2)
            {
                if (j < wieżki.Length)
                {
                    allBudynkiPoSorcie[i] = wieżki[j].gameObject;
                    j++;
                }
                else
                {
                    Debug.Log("Za dużo obrotów");
                    break;
                }
            }
            else if (k == 1)
            {
                if (j < mury.Length)
                {
                    allBudynkiPoSorcie[i] = mury[j].gameObject;
                    j++;
                }
                else
                {
                    k++;
                    j = 0;
                    i--;
                }
            }
            else if (k == 0)
            {
                if (j < resztki.Length)
                {
                    allBudynkiPoSorcie[i] = resztki[j].gameObject;
                    j++;
                }
                else
                {
                    k++;
                    j = 0;
                    i--;
                }
            }
        }
        this.wszystkieBudynki = allBudynkiPoSorcie;
    }
    private void QuickSort(ref KonkretnyNPCStatyczny[] tab, int lewy, int prawy)
    {
        int i = lewy;
        int j = prawy;
        int środek = (tab[Mathf.FloorToInt((lewy + prawy) / 2)].ZwrócPoziomOgólny);

        while (i < j)
        {
            while (tab[i].ZwrócPoziomOgólny < środek) i++;
            while (tab[j].ZwrócPoziomOgólny > środek) j--;

            if (i <= j)
            {
                //Przestaw
                var tmp = tab[i];
                tab[i++] = tab[j];
                tab[j--] = tmp;
            }
        }
        if (lewy < j)
        {
            QuickSort(ref tab, lewy, j);
        }
        if (i < prawy)
        {
            QuickSort(ref tab, i, prawy);
        }
    }
    public void ZaladujWszystkieKonkrenyDoTablicy()
    {
        string[] foldersToSearch = AssetDatabase.GetSubFolders("Assets/Prefaby");
        List<GameObject> allPrefabs = GetAssets<GameObject>(foldersToSearch, "t:prefab");
        List<GameObject> prefabsToWszystkieBudynki = new List<GameObject>();
        for (int i = 0; i < allPrefabs.Count; i++)
        {
            if (allPrefabs[i].TryGetComponent(out KonkretnyNPCStatyczny npc))
            {
                if (npc.obrazekDoBudynku != null)
                    prefabsToWszystkieBudynki.Add(allPrefabs[i]);
            }
        }
        this.wszystkieBudynki = prefabsToWszystkieBudynki.ToArray();
    }
    private List<T> GetAssets<T>(string[] _foldersToSearch, string _filter) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets(_filter, _foldersToSearch);
        List<T> a = new List<T>();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a.Add(AssetDatabase.LoadAssetAtPath<T>(path));
        }
        return a;
    }
#endif
    #endregion
}
public class MaterialyZKolorami
{
    private Material material;
    private float colorx;
    private float colory;
    private float colorz;

    public MaterialyZKolorami(Material _material, Color c)
    {
        this.material = _material;
        this.colorx = c.r;
        this.colory = c.g;
        this.colorz = c.b;
    }
    public void PodmienKolor(byte kolor = 255)  //Jaki kolor ma podmienic
    {
        if (kolor == 255)    //Domyślne
        {
            this.material.color = new Color(this.colorx, this.colory, this.colorz);
        }
        else if (kolor == 0) //Podmien na czerowny
        {
            this.material.color = Color.red;
        }
        else if (kolor == 1) //Podmień na zielony
        {
            this.material.color = Color.green;
        }
    }
    ~MaterialyZKolorami()
    {

    }
}
