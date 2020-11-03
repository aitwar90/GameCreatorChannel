﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class ManagerGryScript : MonoBehaviour
{
    [Header("Podstawowe informacje dla gracza")]
    #region Zmienne publiczne
    [Tooltip("Aktualna ilość monet")]
    public static ushort iloscCoinów = 10000;
    [Tooltip("Aktualna epoka w której gra gracz")]
    public Epoki aktualnaEpoka;
    public byte aktualnyPoziomEpoki = 1;

    [Header("Informacje o graczu")]
    [Tooltip("Czas pomięczy kolejnymi falami hordy")]
    public byte czasWMinutachMiędzyFalami = 10;
    public static short iloscAktywnychWrogów = 0;

    [Tooltip("Zaznaczony NPC")]
    public NPCClass zaznaczonyObiekt = null;
    public delegate void WywołajResetujŚcieżki(KonkretnyNPCStatyczny knpcs = null);
    public WywołajResetujŚcieżki wywołajResetŚcieżek;
    public GameObject[] bazy = new GameObject[1];
    [Tooltip("Tablica nagród jakie gracz może otrzymać")]
    public PrzedmiotScript[] ekwipunekGracza = null;
    [Tooltip("Nagrody jakie gracz poisada")]
    public EkwipunekScript ekwipunek;
    public Skrzynka[] skrzynki;
    [Tooltip("Asset o rozszerzeniu csv z tłumaczeniem")]
    public TextAsset plikJezykowy;
    #endregion

    #region Prywatne zmienne
    KonkretnyNPCStatyczny knpcsBazy = null;
    private byte idxOfManagerGryScript = 0;
    private bool czyScenaZostałaZaładowana = false;
    private bool toNieOstatniaFala = true;
    private ObsługaReklam or;
    public Skrzynka ZwróćSkrzynkeOIndeksie(byte idx)
    {
        return skrzynki[idx];
    }
    public bool CzyScenaZostałaZaładowana
    {
        get
        {
            return czyScenaZostałaZaładowana;
        }
        set
        {
            czyScenaZostałaZaładowana = value;
        }
    }
    public bool CzyReklamaZaładowana
    {
        get
        {
            return or.ZaładowanaReklamaJest;
        }
    }
    #endregion  
    void Awake()
    {
        PomocniczeFunkcje.managerGryScript = this;
        PomocniczeFunkcje.spawnBudynki = FindObjectOfType(typeof(SpawnBudynki)) as SpawnBudynki;
        PomocniczeFunkcje.mainMenu = FindObjectOfType(typeof(MainMenu)) as MainMenu;
        or = FindObjectOfType(typeof(ObsługaReklam)) as ObsługaReklam;
        SpawnerHord.actualHPBars = 0;
        skrzynki = new Skrzynka[PomocniczeFunkcje.mainMenu.buttonSkrzynki.Length];
        for (byte i = 0; i < skrzynki.Length; i++)
        {
            skrzynki[i] = new Skrzynka(ref PomocniczeFunkcje.mainMenu.buttonSkrzynki[i]);
        }
        PomocniczeFunkcje.ŁadujDane();
        UtworzSzablonPlikuJezykowego();
        PomocniczeFunkcje.mainMenu.UstawDropDownEkwipunku(ref ekwipunek);
    }
    private void ŁadowanieDanych()
    {
        PomocniczeFunkcje.spawnerHord = FindObjectOfType(typeof(SpawnerHord)) as SpawnerHord;
        PomocniczeFunkcje.spawnerHord.UstawHorde(aktualnaEpoka, aktualnyPoziomEpoki);
        Terrain terr = FindObjectOfType(typeof(Terrain)) as Terrain;
        PomocniczeFunkcje.tablicaWież = new List<InformacjeDlaPolWież>[20, 20];
        PomocniczeFunkcje.aktualneGranicaTab = (ushort)((terr.terrainData.size.x - 40) / 2.0f);
        PomocniczeFunkcje.distXZ = (terr.terrainData.size.x - (PomocniczeFunkcje.aktualneGranicaTab * 2)) / 20f;
        /*
        for(byte i = 0; i < 20; i++)
        {
            for(byte j = 0; j < 20; j++)
            {
                GameObject go = new GameObject("X = "+i+" Z = "+j);
                go.transform.position = new Vector3(i*PomocniczeFunkcje.distXZ + PomocniczeFunkcje.aktualneGranicaTab, 1.0f, j*PomocniczeFunkcje.distXZ + PomocniczeFunkcje.aktualneGranicaTab);
                go.AddComponent<TestOnEnableGround>();
                TestOnEnableGround toeg = go.GetComponent<TestOnEnableGround>();
                toeg.X = i;
                toeg.Z = j;
            }
        }
        */
        PomocniczeFunkcje.ZapiszDane();
    }
    public void GenerujBaze()
    {
        if (aktualnyPoziomEpoki > PomocniczeFunkcje.odblokowanyPoziomEpoki)
        {
            Debug.Log("Poziom epoki nie został odblokowany");
            return;
        }
        else if ((byte)aktualnaEpoka > PomocniczeFunkcje.odblokowanyPoziomEpoki)
        {
            Debug.Log("Epoka nie została odblokowana");
            return;
        }
        sbyte idxEpokiBazyWTablicy = (sbyte)((sbyte)aktualnaEpoka - 1);
        if (idxEpokiBazyWTablicy < 0 || idxEpokiBazyWTablicy >= bazy.Length)
        {
            Debug.Log("idxEpokaBazyWTablicy = " + idxEpokiBazyWTablicy);
            return;
        }
        else
        {
            ŁadowanieDanych();
            GameObject baza = GameObject.Instantiate(bazy[idxEpokiBazyWTablicy], new Vector3(50.0f, 1.5f, 50.0f), Quaternion.identity);
            knpcsBazy = baza.GetComponent<KonkretnyNPCStatyczny>();
            PomocniczeFunkcje.DodajDoDrzewaPozycji(knpcsBazy, ref PomocniczeFunkcje.korzeńDrzewaPozycji);
            baza.transform.SetParent(PomocniczeFunkcje.spawnBudynki.RodzicBudynków);
            PomocniczeFunkcje.celWrogów = knpcsBazy;
            StartCoroutine("WyzwólKolejnąFalę");
        }
    }
    void FixedUpdate()
    {
        /*
        Fragment kodu, który ma za zadanie zaznaczyć obiekt
        */
#if UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt);
        }
#endif
#if UNITY_ANDROID
        if (Input.mousePresent)
        {
            if (Input.GetMouseButtonDown(0))
            {
                zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt);
            }
        }
        else
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                zaznaczonyObiekt = PomocniczeFunkcje.OkreślKlikniętyNPC(ref zaznaczonyObiekt);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            sbyte t = 120;
            for (byte i = 0; i < skrzynki.Length; i++)
            {
                if (skrzynki[i].ReuseTimer && !skrzynki[i].button.interactable)
                {
                    skrzynki[i].OdejmnijCzas();
                    break;
                }
                else    //
                {
                    if (i < t && !skrzynki[i].button.interactable)
                    {
                        t = (sbyte)i;
                    }
                    if (t != 120 && i == (skrzynki.Length - 1))
                    {
                        skrzynki[t].RozpocznijOdliczanie();
                    }
                }
            }
        }
#endif
        if (zaznaczonyObiekt != null)
        {
            PomocniczeFunkcje.spawnBudynki.teksAktualnegoObiektu.text = "Altualnie zaznaczony obiekt: " + zaznaczonyObiekt.nazwa;
        }
    }
    void Update()
    {
        switch (idxOfManagerGryScript)
        {
            case 255:
                for (byte i = 0; i < 4; i++)
                {
                    skrzynki[i].SprawdźCzyReuseMinęło();
                }
                idxOfManagerGryScript++;
                break;
            default:
                idxOfManagerGryScript++;
                break;

        }
        if (czyScenaZostałaZaładowana)
        {
            if (!toNieOstatniaFala && iloscAktywnychWrogów <= 0)
            {
                //Lvl skończony wszystkie fale zostały pokonane
                KoniecPoziomuZakończony(true);
            }
            else if (knpcsBazy.AktualneŻycie <= 0)
            {
                KoniecPoziomuZakończony(false);
            }
        }
        else
        {
            SprawdźCzyScenaZostałaZaładowana();
        }
    }
    private void SprawdźCzyScenaZostałaZaładowana()
    {
        Scene s = SceneManager.GetSceneByBuildIndex((byte)aktualnaEpoka);
        if (s.isLoaded)
        {
            czyScenaZostałaZaładowana = true;
            GenerujBaze();
        }
    }
    private IEnumerator WyzwólKolejnąFalę()
    {
        yield return new WaitForSeconds(czasWMinutachMiędzyFalami * 60);
        toNieOstatniaFala = PomocniczeFunkcje.spawnerHord.GenerujSpawn(aktualnaEpoka);
        if (toNieOstatniaFala)
        {
            StartCoroutine("WyzwólKolejnąFalę");
        }
    }
    private void KoniecPoziomuZakończony(bool sukces = true)
    {
        if (sukces)
        {
            if (aktualnyPoziomEpoki == PomocniczeFunkcje.odblokowanyPoziomEpoki)
            {
                if (aktualnyPoziomEpoki == 100 && (byte)aktualnaEpoka < PomocniczeFunkcje.odblokowanyPoziomEpoki)
                {
                    PomocniczeFunkcje.odblokowaneEpoki++;
                }
                PomocniczeFunkcje.odblokowanyPoziomEpoki++;
            }
            PomocniczeFunkcje.ZapiszDane();
            PomocniczeFunkcje.mainMenu.nastepnyPoziom.gameObject.SetActive(true);
            PomocniczeFunkcje.mainMenu.powtorzPoziom.gameObject.SetActive(true);
            PomocniczeFunkcje.mainMenu.rekZaWyzszaNagrode.gameObject.SetActive(CzyReklamaZaładowana);
            PomocniczeFunkcje.mainMenu.UstawPrzyciskObrotu(false);
            OdblokujKolejnaSkrzynke();
        }
        else
        {
            PomocniczeFunkcje.mainMenu.powtorzPoziom.gameObject.SetActive(true);
        }
        toNieOstatniaFala = true;
    }
    public void PrzejdźNaNastepnyPoziom(bool czyNowyPoziom = true)
    {
        if (czyNowyPoziom)
        {
            if (aktualnyPoziomEpoki >= 100)
            {
                aktualnyPoziomEpoki = 0;
                byte f = (byte)aktualnaEpoka;
                aktualnaEpoka = Epoki.None;
                f++;
                aktualnaEpoka += f;
            }
            else
            {
                aktualnyPoziomEpoki++;
            }
        }
        PomocniczeFunkcje.mainMenu.nastepnyPoziom.gameObject.SetActive(false);
        PomocniczeFunkcje.mainMenu.powtorzPoziom.gameObject.SetActive(false);
        PomocniczeFunkcje.mainMenu.ResetSceny();
    }
    public void CudOcalenia()
    {
        PomocniczeFunkcje.celWrogów.AktualneŻycie = (short)(PomocniczeFunkcje.celWrogów.maksymalneŻycie / 2.0f);
        KonkretnyNPCDynamiczny[] knpcd = FindObjectsOfType(typeof(KonkretnyNPCDynamiczny)) as KonkretnyNPCDynamiczny[];
        for (ushort i = 0; i < knpcd.Length; i++)
        {
            knpcd[i].AktualneŻycie = 0;
            knpcd[i].NieŻyję = true;
        }
        KonkretnyNPCStatyczny[] knpcs = FindObjectsOfType(typeof(KonkretnyNPCStatyczny)) as KonkretnyNPCStatyczny[];
        for (ushort i = 0; i < knpcs.Length; i++)
        {
            if (knpcs[i].AktualneŻycie > 0)
                knpcs[i].AktualneŻycie = knpcs[i].maksymalneŻycie;
        }
        PomocniczeFunkcje.mainMenu.powtorzPoziom.gameObject.SetActive(false);
        PomocniczeFunkcje.mainMenu.UstawPrzyciskObrotu(false);
    }
    public void KliknietyPrzycisk(byte idx)
    {
        if (ekwipunek == null)
            ekwipunek = new EkwipunekScript(null);
        byte idxPrzedmiotuLosowanego = ekwipunek.LosujNagrode();
        bool c = true;
        if (ekwipunek.przedmioty != null && ekwipunek.przedmioty.Length > 0)
        {
            if (PomocniczeFunkcje.mainMenu.SprawdźCzyNazwaPasujeItemDropDown(ekwipunekGracza[idxPrzedmiotuLosowanego].nazwaPrzedmiotu))
            {
                for (byte i = 0; i < ekwipunek.przedmioty.Length; i++)
                {
                    if (ekwipunek.przedmioty[i].nazwaPrzedmiotu == ekwipunekGracza[idxPrzedmiotuLosowanego].nazwaPrzedmiotu)
                    {
                        PomocniczeFunkcje.mainMenu.AktualizujInfoOIlosci(i, ekwipunek.przedmioty[i].nazwaPrzedmiotu, ekwipunek.przedmioty[i].ilośćDanejNagrody);
                        c = false;
                        break;
                    }
                }
            }
        }
        if (c)
        {
            PomocniczeFunkcje.mainMenu.UstawDropDownEkwipunku(ref ekwipunek);
        }
    }
    public void SkróćCzasSkrzynki(sbyte idxS = -1)
    {
        if (idxS == -1)
        {
            for (byte i = 0; i < skrzynki.Length; i++)
            {
                if (skrzynki[i].ReuseTimer && !skrzynki[i].button.interactable)
                {
                    skrzynki[i].OdejmnijCzas();
                    break;
                }
            }
        }
        else if (idxS > -1)
        {
            if (skrzynki[idxS].ReuseTimer && !skrzynki[idxS].button.interactable)
            {
                skrzynki[idxS].OdejmnijCzas();
            }
        }
    }
    private void OdblokujKolejnaSkrzynke()
    {
        for (byte i = 0; i < skrzynki.Length; i++)
        {
            if (!skrzynki[i].ReuseTimer && !skrzynki[i].button.interactable)
            {
                skrzynki[i].RozpocznijOdliczanie();
                break;
            }
        }
    }
    public void KliknietyButtonZwiekszeniaNagrodyPoLvlu()
    {
        ushort c = (ushort)(((byte)aktualnaEpoka) * 10 + aktualnyPoziomEpoki);
        or.OtwórzReklame(1, c);
    }
    public void UzyciePrzedmiotu(byte idxOfItem)
    {
        if (ekwipunek != null && ekwipunek.przedmioty != null && ekwipunek.przedmioty.Length > 0)
        {
            if (ekwipunek.przedmioty.Length > idxOfItem)
            {
                if (ekwipunek.przedmioty[idxOfItem].ilośćDanejNagrody > 0)
                {
                    ekwipunek.przedmioty[idxOfItem].AktywujPrzedmiot();
                    bool c = true;
                    if (ekwipunek.przedmioty[idxOfItem].ilośćDanejNagrody == 0)
                    {
                        c = false;
                        List<PrzedmiotScript> ps = new List<PrzedmiotScript>();
                        for (byte i = 0; i < ekwipunek.przedmioty.Length; i++)
                        {
                            if (i == idxOfItem)
                            {
                                continue;
                            }
                            else
                            {
                                ps.Add(ekwipunek.przedmioty[i]);
                            }
                        }
                        ekwipunek.przedmioty = ps.ToArray();
                        PomocniczeFunkcje.mainMenu.UstawDropDownEkwipunku(ref ekwipunek);
                    }
                    if (c)
                    {
                        PomocniczeFunkcje.mainMenu.AktualizujInfoOIlosci(idxOfItem, ekwipunek.przedmioty[idxOfItem].nazwaPrzedmiotu, ekwipunek.przedmioty[idxOfItem].ilośćDanejNagrody);
                    }
                }
            }
        }
    }
    public void KlikniętaReklamaButtonSkrzynki(byte idx)
    {
        or.OtwórzReklame(2, idx);
    }
    public void ZmianaJęzyka(byte idx)
    {
        //if (plikJezykowy != null)
        //{
            UnityEngine.UI.Text[] wszystkieFrazy = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text[];
            string fs = plikJezykowy.text;
            fs = fs.Replace("\n", "");
            fs = fs.Replace("\r", "");
            string[] fLines = fs.Split(';');
            idx++;
            for (ushort i = 0; i < fLines.Length; i++)
            {
                string[] pFrazy = fLines[i].Split(',');
                if (idx >= pFrazy.Length)
                {
                    continue;
                }
                if (pFrazy[idx] != "")
                {
                    for (ushort j = 0; j < wszystkieFrazy.Length; j++)
                    {
                        if (wszystkieFrazy[j].transform.name != "Text")
                        {
                            if (pFrazy[0] == wszystkieFrazy[j].transform.name)
                            {
                                wszystkieFrazy[j].text = pFrazy[idx];
                                break;
                            }
                        }
                        else
                        {
                            if (pFrazy[0] == wszystkieFrazy[j].transform.parent.name)
                            {
                                wszystkieFrazy[j].text = pFrazy[idx];
                                break;
                            }
                        }
                    }
                    if (PomocniczeFunkcje.spawnBudynki != null)
                    {
                        for (ushort j = 0; j < PomocniczeFunkcje.spawnBudynki.wszystkieBudynki.Length; j++)
                        {
                            if (pFrazy[0] == PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[j].name)
                            {
                                PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[j].GetComponent<KonkretnyNPCStatyczny>().nazwa = pFrazy[idx];
                                break;
                            }
                        }
                        PomocniczeFunkcje.spawnBudynki.ZróbListęDropdownBudynków();
                    }
                    if(PomocniczeFunkcje.managerGryScript != null)
                    {
                        for(ushort j = 0; j < PomocniczeFunkcje.managerGryScript.ekwipunekGracza.Length; j++)
                        {
                            if(PomocniczeFunkcje.managerGryScript.ekwipunekGracza[j].name == pFrazy[0])
                            {
                                PomocniczeFunkcje.managerGryScript.ekwipunekGracza[j].nazwaPrzedmiotu = pFrazy[idx];
                            }
                        }
                        PomocniczeFunkcje.mainMenu.UstawDropDownEkwipunku(ref ekwipunek);
                    }
                }
            }
        //}
    }
    private void UtworzSzablonPlikuJezykowego()
    {
        string sciezka = "Assets/Resources/jezyki.txt";
        if (plikJezykowy == null)
        {
            if (File.Exists(sciezka))
            {
                File.Delete(sciezka);
            }
            StreamWriter writer = File.CreateText(sciezka);
            UnityEngine.UI.Text[] wszystkieFrazy = FindObjectsOfType<UnityEngine.UI.Text>() as UnityEngine.UI.Text[];
            for (ushort i = 0; i < wszystkieFrazy.Length; i++)
            {
                string zapisywanaFraza = "";
                if (wszystkieFrazy[i].transform.name != "Text")
                {
                    zapisywanaFraza = zapisywanaFraza + wszystkieFrazy[i].transform.name + ",";
                }
                else
                {
                    zapisywanaFraza = zapisywanaFraza + wszystkieFrazy[i].transform.parent.name + ",";
                }
                zapisywanaFraza = zapisywanaFraza + wszystkieFrazy[i].text + ";";
                writer.WriteLine(zapisywanaFraza);
            }
            TextMesh[] allTe = FindObjectsOfType<TextMesh>() as TextMesh[];
            for (ushort i = 0; i < allTe.Length; i++)
            {
                string zapisywanaFraza = "";
                if (allTe[i].transform.name != "Text")
                {
                    zapisywanaFraza = zapisywanaFraza + allTe[i].transform.name + ",";
                }
                else
                {
                    zapisywanaFraza = zapisywanaFraza + allTe[i].transform.parent.name + ",";
                }
                zapisywanaFraza = zapisywanaFraza + allTe[i].text + ";";
                writer.WriteLine(zapisywanaFraza);
            }
            if (PomocniczeFunkcje.spawnBudynki != null)
            {
                for (ushort i = 0; i < PomocniczeFunkcje.spawnBudynki.wszystkieBudynki.Length; i++)
                {
                    string zapisywanaFraza = "";
                    KonkretnyNPCStatyczny knpcs = PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[i].GetComponent<KonkretnyNPCStatyczny>();
                    zapisywanaFraza = zapisywanaFraza + knpcs.gameObject.name + ",";
                    zapisywanaFraza = zapisywanaFraza + knpcs.nazwa + ";";
                    writer.WriteLine(zapisywanaFraza);
                }
            }
            else
            {
                Debug.Log("Spawn Budynki jest null");
            }
            if(PomocniczeFunkcje.managerGryScript != null)
            {
                for(ushort i = 0; i < ekwipunekGracza.Length; i++)
                {
                    string zapisywanaFraza = "";
                    zapisywanaFraza = zapisywanaFraza + ekwipunekGracza[i].gameObject.name + ",";
                    zapisywanaFraza = zapisywanaFraza + ekwipunekGracza[i].nazwaPrzedmiotu + ";";
                    writer.WriteLine(zapisywanaFraza);
                }
            }
            writer.Close();
        }
        //Hehe
    }
}
