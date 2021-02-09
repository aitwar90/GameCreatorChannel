using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour, ICzekajAz
{
    #region Zmienne przycisków
    public Button nastepnyPoziom;
    public Button powtorzPoziom;
    public PrzyciskiSkrzynekIReklam[] buttonSkrzynki;
    public Button wróćDoMenu;
    public Button rekZaWyzszaNagrode;
    public Button kup;
    public Button wróc;
    public Button zmianaJezyka;
    public Button rotacjaBudynku;
    public Button odpalPoziom;
    public Button lvlNizej;
    public Button lvlWyzej;
    public Button epokaNizej;
    public Button epokaWyzej;
    #endregion
    #region Akademia
    public Button buttonAZycie;
    public Button buttonAAtak;
    public Button buttonAObrona;
    #endregion
    #region Ekwipunek
    [Tooltip("Przyciski nagród powinny być wpięte w tablicę w kolejności zgodnej z taką jak Ekwipunek Gracza w ManagerGryScript.cs")]
    public Button[] przyciskiNagród;
    #endregion
    #region TextUI
    public Text ilośćCoinów;
    public Text ilośćFal;
    public Text lFPS;
    private Text actWybEpoka;
    private Text licznikCzasuDoFali;
    public InputField poziomWEpoce;
    #endregion
    #region Panel
    public KontenerKomponentów panelDynamiczny;
    public KontenerKomponentów panelStatyczny;
    public KontenerKomponentów panelBudynki;
    #endregion
    #region Panel budynków
    public Button stawiajBudynek;
    private GameObject uiBudynkiPanel;
    private byte wielkosćButtonu = 0;
    private byte iloscButtonow = 1;
    private RectTransform trBudynkówŁącze = null;
    private ushort[] idxWież = null;
    private ushort[] idxMurów = null;
    private ushort[] idxInne = null;
    private sbyte lastPanelEnabledBuildings = -1;
    private sbyte ostatniZaznaczonyObiektBudowania = -1;
    #endregion
    #region Obiekty ładowane
    public Slider sliderDźwięku;
    private Canvas menu;
    private Canvas uiGry;
    private Canvas optionsMenu;
    private Canvas poGraj;
    private Canvas reklamyPanel;
    private Canvas ui_down;
    private Canvas goPanel;
    private GameObject samouczekPanel;
    private Button przyciskWznów;
    private Vector3 lastPosCam = Vector3.zero;
    private sbyte wybranaNagroda = -1;
    public sbyte lastIdxJezyka = 0;
    public static MainMenu singelton = null;
    private bool odpalonyPanel = false;
    private RectTransform rectHpBar;
    private sbyte kPoziomDoZaladowania = -1;
    #endregion
    #region Getery i setery
    public sbyte UstawLubPobierzOstatniIdexJezyka
    {
        get
        {
            return lastIdxJezyka;
        }
        set
        {
            lastIdxJezyka = (sbyte)(value - 1);
            ZmieńJęzyk();
        }
    }
    public bool OdpalonyPanel
    {
        get
        {
            return odpalonyPanel;
        }
        set
        {
            odpalonyPanel = value;
        }
    }
    public bool CzyLFPSOn
    {
        set
        {
            Toggle t = this.transform.Find("Menu/OptionsMenu/GrafikaPanel/ToggleFPS").GetComponent<Toggle>();
            t.isOn = value;
        }
        get
        {
            return lFPS.gameObject.activeSelf;
        }
    }
    public bool CzyOdpaloneMenu
    {
        get
        {
            return menu.enabled;
        }
    }
    public sbyte OdpalonyPanelBudynków
    {
        get
        {
            return lastPanelEnabledBuildings;
        }
    }
    #endregion
    void Awake()
    {
        if (singelton == null)
        {
            singelton = this;
        }
        else
        {
            Destroy(this);
            return;
        }
        this.poziomWEpoce.characterValidation = InputField.CharacterValidation.Integer;
        menu = this.transform.Find("Menu/MainMenu").GetComponent<Canvas>();
        uiGry = this.transform.Find("UIGry").GetComponent<Canvas>();
        optionsMenu = this.transform.Find("Menu/OptionsMenu").GetComponent<Canvas>();
        poGraj = this.transform.Find("Menu/PoGraj").GetComponent<Canvas>();
        reklamyPanel = this.transform.Find("Menu/PanelSkrzynki").GetComponent<Canvas>();
        ui_down = uiGry.transform.Find("ui_down").GetComponent<Canvas>();
        przyciskWznów = this.transform.Find("Menu/MainMenu/ResumeButton").GetComponent<Button>();
        actWybEpoka = this.transform.Find("Menu/PoGraj/AktualnieWybEpoka").GetComponent<Text>();
        uiBudynkiPanel = uiGry.transform.Find("UI_BudynkiPanel").gameObject;
        rectHpBar = uiGry.transform.Find("PasekZyciaGłównegoBudynku/Green").GetComponent<RectTransform>();
        goPanel = uiGry.transform.Find("GameOver Panel").GetComponent<Canvas>();
        licznikCzasuDoFali = uiGry.transform.Find("UI_LicznikCzasu/img_licznik/KompTextLicznikCzasu").GetComponent<Text>();
        samouczekPanel = uiGry.transform.Find("SamouczekPanel").gameObject;
        epokaNizej.interactable = false;
        panelStatyczny.gameObject.SetActive(false);
        panelDynamiczny.gameObject.SetActive(false);
        epokaWyzej.interactable = false;
        OdpalButtonyAkademii(false);
        UstawPrzyciskObrotu(false);
        przyciskWznów.interactable = false;
        PomocniczeFunkcje.oCam = Camera.main;
        nastepnyPoziom.interactable = false;
        rekZaWyzszaNagrode.gameObject.SetActive(false);
        WłączWyłączPanel(new string[] {goPanel.transform.name, uiBudynkiPanel.transform.name, ui_down.transform.name, uiGry.transform.name, optionsMenu.transform.name,
        reklamyPanel.transform.name, poGraj.transform.name, samouczekPanel.name, "Cretidsy"}, false);
    }
    #region Obsługa paneli UI, Czy mogę przesuwać kamerę (), Pasek HP
    public void WłączWyłączPanel(string panel, bool czyWłączyć)
    {
        if (panel == menu.name)
        {
            menu.enabled = czyWłączyć;
        }
        else if (panel == uiGry.name)
        {
            uiGry.enabled = czyWłączyć;
        }
        else if (panel == optionsMenu.name)
        {
            optionsMenu.enabled = czyWłączyć;
        }
        else if (panel == poGraj.name)
        {
            poGraj.enabled = czyWłączyć;
        }
        else if (panel == reklamyPanel.name)
        {
            reklamyPanel.enabled = czyWłączyć;
        }
        else if (ui_down.name == panel)
        {
            ui_down.enabled = czyWłączyć;
            licznikCzasuDoFali.transform.parent.gameObject.SetActive(czyWłączyć);
        }
        else if (goPanel.name == panel)
        {
            goPanel.enabled = czyWłączyć;
        }
        else if (uiBudynkiPanel.name == panel)
        {
            uiBudynkiPanel.SetActive(czyWłączyć);
        }
        else if (panel == "UI_LicznikCzasu")
        {
            licznikCzasuDoFali.transform.parent.gameObject.SetActive(czyWłączyć);
        }
        else if (panel == samouczekPanel.name)
        {
            samouczekPanel.SetActive(czyWłączyć);
        }
        else if (panel == "Cretidsy") //Powinno być ostatnie (kto odpala creditsy xD)
        {
            this.transform.Find("Menu/Cretidsy").GetComponent<Canvas>().enabled = czyWłączyć;
        }
    }
    public void WłączWyłączPanel(string[] panel, bool czyWłączyć)
    {
        for (byte i = 0; i < panel.Length; i++)
        {
            if (panel[i] == menu.name)
            {
                menu.enabled = czyWłączyć;
            }
            else if (panel[i] == uiGry.name)
            {
                uiGry.enabled = czyWłączyć;
            }
            else if (panel[i] == optionsMenu.name)
            {
                optionsMenu.enabled = czyWłączyć;
            }
            else if (panel[i] == poGraj.name)
            {
                poGraj.enabled = czyWłączyć;
            }
            else if (panel[i] == reklamyPanel.name)
            {
                reklamyPanel.enabled = czyWłączyć;
            }
            else if (ui_down.name == panel[i])
            {
                ui_down.enabled = czyWłączyć;
                licznikCzasuDoFali.transform.parent.gameObject.SetActive(czyWłączyć);
            }
            else if (goPanel.name == panel[i])
            {
                goPanel.enabled = czyWłączyć;
            }
            else if (uiBudynkiPanel.name == panel[i])
            {
                uiBudynkiPanel.SetActive(czyWłączyć);
            }
            else if (panel[i] == "UI_LicznikCzasu")
            {
                licznikCzasuDoFali.transform.parent.gameObject.SetActive(czyWłączyć);
            }
            else if (panel[i] == samouczekPanel.name)
            {
                samouczekPanel.SetActive(czyWłączyć);
            }
            else if (panel[i] == "Cretidsy") //Powinno być ostatnie (kto odpala creditsy xD)
            {
                this.transform.Find("Menu/Cretidsy").GetComponent<Canvas>().enabled = czyWłączyć;
            }
        }
    }
    public void PrzełączUI(bool aktywujeMenu)
    {
        if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki != 255)
            przyciskWznów.interactable = true;
        else
            przyciskWznów.interactable = false;
        menu.enabled = aktywujeMenu;
        uiGry.enabled = !aktywujeMenu;
        menu.transform.parent.GetComponent<Image>().enabled = aktywujeMenu;
        if (aktywujeMenu)
        {
            PomocniczeFunkcje.UstawTimeScale(0);
            lastPosCam = PomocniczeFunkcje.oCam.transform.position;
            PomocniczeFunkcje.oCam.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
            PomocniczeFunkcje.muzyka.WłączWyłączClip(true, "Tło_None");
        }
        else
        {
            PomocniczeFunkcje.UstawTimeScale(1);
            PomocniczeFunkcje.oCam.transform.position = lastPosCam;
            PomocniczeFunkcje.muzyka.WłączWyłączClip(true, PomocniczeFunkcje.TagZEpoka("AmbientWGrze", PomocniczeFunkcje.managerGryScript.aktualnaEpoka));
        }
    }
    public void UstawHPGłównegoPaska(float wartoscX)
    {
        rectHpBar.localScale = new Vector3(wartoscX, 1, 1);
    }

    public void UstawTextUI(string nazwaTekstu, string tekst)
    {
        if (nazwaTekstu == "timer")
        {
            licznikCzasuDoFali.text = tekst;
        }
        else if (nazwaTekstu == "ilośćCoinów")
        {
            ilośćCoinów.text = tekst;
        }
        else if (nazwaTekstu == "ilośćFal")
        {
            ilośćFal.text = tekst;
        }
    }
    public void UstawPanelUI(string parametry, Vector2 pos, KonkretnyNPCStatyczny knpcs = null)
    {
        if (odpalonyPanel)
        {
            panelDynamiczny.gameObject.SetActive(false);
            panelStatyczny.gameObject.SetActive(false);
            odpalonyPanel = false;
        }
        if (parametry == "")
        {
            return;
        }
        string[] s = parametry.Split(';');
        if (s[0] == "STATYCZNY")
        {
            PanelStatyczny ps = (PanelStatyczny)panelStatyczny;
            ps.KNPCS = knpcs;
            RectTransform r = ps.GetComponent<RectTransform>();
            if (s[1] == "True" && ui_down.enabled)  //Odblokuj naprawe budynku
            {
                ps.naprawButton.interactable = true;
            }
            else
            {
                ps.naprawButton.interactable = false;
            }

            ps.UstawDane(new string[] { s[2], s[3], s[4], s[5], s[6], s[7] });

            r.position = pos;
            ps.gameObject.SetActive(true);
            odpalonyPanel = true;
        }
        else if (s[0] == "DYNAMICZNY")
        {
            PanelDynamiczny ps = (PanelDynamiczny)panelDynamiczny;
            RectTransform r = ps.GetComponent<RectTransform>();
            ps.UstawDane(new string[] { s[1], s[2], s[3] });
            r.position = pos;
            ps.gameObject.SetActive(true);
            odpalonyPanel = true;
        }
        else if (s[0] == "PANEL")
        {
            PanelTextuWBudynkach ps = (PanelTextuWBudynkach)panelBudynki;
            ps.UstawDane(new string[] { s[1], s[2], s[3], s[4], s[5], s[6], s[7], s[8] });
        }
    }
    public bool CzyMogePrzesuwaćKamere()
    {
        if (uiGry.enabled)
        {
            if (CzyOdpaloneMenu || goPanel.enabled || CzyAktywnyPanelZBudynkami())
            {
                return false;
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    public Vector2 ZwrocRectTransformObiektu(string nazwaSzukanegoObiektu)
    {
        Vector2 zwracanaPos = Vector2.zero;
        switch (nazwaSzukanegoObiektu)
        {
            case "kup":
                zwracanaPos = kup.GetComponent<RectTransform>().position;
                break;
            case "stawiajBudynek":
                zwracanaPos = stawiajBudynek.GetComponent<RectTransform>().position;
                break;
            case "rotacjaBudynku":
                zwracanaPos = rotacjaBudynku.GetComponent<RectTransform>().position;
                break;
            case "cudOcalenia":
                zwracanaPos = przyciskiNagród[1].GetComponent<RectTransform>().position;
                break;
            case "coinyNagroda":
                zwracanaPos = przyciskiNagród[0].GetComponent<RectTransform>().position;
                break;
            case "dodatkowaNagroda":
                zwracanaPos = przyciskiNagród[2].GetComponent<RectTransform>().position;
                break;
            case "skrócenieCzasuSkrzynki":
                zwracanaPos = przyciskiNagród[3].GetComponent<RectTransform>().position;
                break;
            case "buttonAŻycie":
                zwracanaPos = buttonAZycie.GetComponent<RectTransform>().position;
                break;
            case "buttonAAtak":
                zwracanaPos = buttonAAtak.GetComponent<RectTransform>().position;
                break;
            case "buttonAObrona":
                zwracanaPos = buttonAObrona.GetComponent<RectTransform>().position;
                break;
            case "kupnoWieża":
                zwracanaPos = ui_down.transform.Find("kupno_wieza").GetComponent<RectTransform>().position;
                break;
            case "kupnoMur":
                zwracanaPos = ui_down.transform.Find("kupno_mur").GetComponent<RectTransform>().position;
                break;
            case "kupnoInne":
                zwracanaPos = ui_down.transform.Find("kupno_inne").GetComponent<RectTransform>().position;
                break;
            case "naprawBudynek":
                PanelStatyczny ps = (PanelStatyczny)panelStatyczny;
                zwracanaPos = ps.naprawButton.GetComponent<RectTransform>().position;
                break;
            default:
                zwracanaPos = Vector2.negativeInfinity;
                break;
        }
        return zwracanaPos;
    }
    #endregion
    #region Po Scenie
    public void OdpalPoScenie(bool czyOdpalamPoScenie)
    {
        if (czyOdpalamPoScenie)
        {
            menu.enabled = false;
            poGraj.enabled = true;

            poziomWEpoce.text = PomocniczeFunkcje.odblokowanyPoziomEpoki.ToString();
            actWybEpoka.text = PomocniczeFunkcje.odblokowaneEpoki.ToString();
            PomocniczeFunkcje.muzyka.WłączWyłączClip(true, "Tło_None_PoMenu");
        }
        else
        {
            menu.enabled = true;
            poGraj.enabled = false;
            PomocniczeFunkcje.muzyka.WłączWyłączClip(true, "Tło_None");
        }
    }
    public void ResetSceny(bool ładowaćNowąScene = true)
    {
        int unSceneIdx = ObslugaScenScript.indeksAktualnejSceny;
        PomocniczeFunkcje.ResetujWszystko();
        SceneManager.UnloadSceneAsync(unSceneIdx);
        if (ładowaćNowąScene)
            StartCoroutine(CzekajAz());
    }
    public void ResetSceny(sbyte aktualnyPoziomEpoki)
    {
        int unSceneIdx = ObslugaScenScript.indeksAktualnejSceny;
        PomocniczeFunkcje.ResetujWszystko();
        SceneManager.UnloadSceneAsync(unSceneIdx);
        if (aktualnyPoziomEpoki != kPoziomDoZaladowania)
        {
            kPoziomDoZaladowania = aktualnyPoziomEpoki;
            StartCoroutine(CzekajAz());
        }
    }
    public IEnumerator CzekajAz()
    {
        yield return new WaitUntil(() => SceneManager.sceneCount == 1);
        MetodaDoOdpaleniaPoWyczekaniu();
    }
    public void MetodaDoOdpaleniaPoWyczekaniu()
    {
        if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki > 100)
        {
            OdpalSamouczek();
        }
        else
        {
            OdpalPoziom();
        }
    }
    private byte DajMiMaxPoziom(string ustawionaEpoka)
    {
        int ustEpoka = int.Parse(ustawionaEpoka);
        if (ustEpoka < 1)
        {
            return 0;
        }
        else
        {
            if (ustEpoka < PomocniczeFunkcje.odblokowaneEpoki)
            {
                return 100;
            }
            else if (ustEpoka == PomocniczeFunkcje.odblokowaneEpoki)
            {
                byte temp = (byte)(PomocniczeFunkcje.odblokowanyPoziomEpoki % 100);
                if (temp == 0)
                {
                    return 1;
                }
                else
                    return temp;
            }
            else
            {
                return 0;
            }
        }
    }
    public void OdpalPoziom()
    {
        if (ManagerSamouczekScript.byloZaladowane)
        {
            ManagerSamouczekScript.mssInstance.OpuśćSamouczek(false);
        }
        byte poziom = 0;
        if (kPoziomDoZaladowania < 0) //Jeśli scena ma zostać ładowana przy pomocy danych z poScenie   
        {
            poziom = (byte)int.Parse(poziomWEpoce.text);
            PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki = poziom;
        }
        poziom = (byte)int.Parse(actWybEpoka.text);
        Epoki e = Epoki.None;
        e += poziom;
        PomocniczeFunkcje.managerGryScript.aktualnaEpoka = e;
        if (SceneManager.sceneCount == 1)
        {
            poziom = PomocniczeFunkcje.managerGryScript.GetComponent<ObslugaScenScript>().ZwróćIndeksScenyPoEpoce(e);
            SceneManager.LoadScene(poziom, LoadSceneMode.Additive);
        }
        else
        {
            //Reset scene
            ResetSceny();
        }
        PomocniczeFunkcje.oCam.transform.position = MoveCameraScript.bazowePolozenieKameryGry;
        lastPosCam = MoveCameraScript.bazowePolozenieKameryGry;
        poGraj.enabled = false;
        PrzełączUI(false);
        kPoziomDoZaladowania = -1;
    }
    public void OdpalSamouczek()
    {
        PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki = 255;
        PomocniczeFunkcje.managerGryScript.aktualnaEpoka = Epoki.EpokaKamienia;
        if (SceneManager.sceneCount == 1)
        {
            byte poziom = PomocniczeFunkcje.managerGryScript.GetComponent<ObslugaScenScript>().ZwróćScenęSamouczka();
            if (poziom < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(poziom, LoadSceneMode.Additive);
            else
            {
                Debug.Log("Nie odnalazłem sceny dla danej opcji");
                return;
            }
        }
        else
        {
            //Reset scene
            ResetSceny();
        }
        PomocniczeFunkcje.oCam.transform.position = MoveCameraScript.bazowePolozenieKameryGry;
        lastPosCam = MoveCameraScript.bazowePolozenieKameryGry;
        poGraj.enabled = false;
        PrzełączUI(false);
        ManagerSamouczekScript.mssInstance.ŁadujDaneSamouczek();
    }
    public void UstawEpokeWyzejNizej(bool czyWyzej)
    {
        int actEpok = int.Parse(actWybEpoka.text);
        if (czyWyzej)
        {
            if (actEpok < PomocniczeFunkcje.odblokowaneEpoki)
            {
                actEpok++;
                actWybEpoka.text = actEpok.ToString();
                poziomWEpoce.text = DajMiMaxPoziom(actEpok.ToString()).ToString();
            }
        }
        else
        {
            if (actEpok > 1)
            {
                actEpok--;
                actWybEpoka.text = actEpok.ToString();
                poziomWEpoce.text = DajMiMaxPoziom(actEpok.ToString()).ToString();
            }
        }
    }
    public void LVLNizejWyzej(bool czyWyzej)
    {
        int t = int.Parse(poziomWEpoce.text);
        if (czyWyzej)
        {
            t++;
            if (t <= DajMiMaxPoziom(actWybEpoka.text))
            {
                poziomWEpoce.text = t.ToString();
            }
        }
        else
        {
            t--;
            if (t > 0)
            {
                poziomWEpoce.text = t.ToString();
            }
        }
    }
    #endregion
    #region Budynki interfejs
    public bool CzyAktywnyPanelZBudynkami()
    {
        if (uiBudynkiPanel.activeInHierarchy)
            return true;
        else
            return false;
    }
    public void PrzesuńBudynki(float wartość, bool zresetuj = false)
    {
        short wartośćPrzesunięciaY = -290;
        if (zresetuj)
        {
            trBudynkówŁącze.anchoredPosition = new Vector3(trBudynkówŁącze.anchoredPosition.x, wartośćPrzesunięciaY);
            return;
        }
        Vector2 sOff = Vector2.zero;
        sOff.y += wartość;
        byte offsetBB = 50;
        Vector3 tmp = trBudynkówŁącze.anchoredPosition = trBudynkówŁącze.anchoredPosition + sOff;   //Wartość po przesunięciu obiektu o wartość
        short t = (short)(wartośćPrzesunięciaY + ((wielkosćButtonu + offsetBB) * iloscButtonow));   //Obszar po Y wszystkich przycisków
        if (tmp.y >= wartośćPrzesunięciaY && tmp.y <= t)
        {
            trBudynkówŁącze.anchoredPosition = tmp;
        }
        else
        {
            if (tmp.y < wartośćPrzesunięciaY)
            {
                trBudynkówŁącze.anchoredPosition = new Vector3(trBudynkówŁącze.anchoredPosition.x, wartośćPrzesunięciaY);
            }
            else
            {
                trBudynkówŁącze.anchoredPosition = new Vector3(trBudynkówŁącze.anchoredPosition.x, t);
            }
        }
    }
    public void WłączWyłączPanelBudynków(int idx)
    {
        PrzesuńBudynki(0, true);
        if (idx == 0)    //Panel z wieżami
        {
            if (idxWież == null)
                return;
            WłączWyłączPanel("UI_BudynkiPanel", true);
            WłączWyłączPanel("UI_LicznikCzasu", false);
            PomocniczeFunkcje.UstawTimeScale(0);
            if (lastPanelEnabledBuildings != -1 && lastPanelEnabledBuildings != 0)
            {
                if (lastPanelEnabledBuildings == 1)
                    EnDisButtonsOfBuildingsInPanel(ref idxMurów, false);
                else if (lastPanelEnabledBuildings == 2)
                    EnDisButtonsOfBuildingsInPanel(ref idxInne, false);
            }
            EnDisButtonsOfBuildingsInPanel(ref idxWież, true);
            lastPanelEnabledBuildings = 0;
            ostatniZaznaczonyObiektBudowania = -1;
            ManagerSamouczekScript.mssInstance.WyłączVisual();
        }
        else if (idx == 1)   //Panel z murkami
        {
            if (idxMurów == null)
                return;
            WłączWyłączPanel("UI_BudynkiPanel", true);
            WłączWyłączPanel("UI_LicznikCzasu", false);
            PomocniczeFunkcje.UstawTimeScale(0);
            if (lastPanelEnabledBuildings != -1 && lastPanelEnabledBuildings != 1)
            {
                if (lastPanelEnabledBuildings == 0)
                    EnDisButtonsOfBuildingsInPanel(ref idxWież, false);
                else if (lastPanelEnabledBuildings == 2)
                    EnDisButtonsOfBuildingsInPanel(ref idxInne, false);
            }
            EnDisButtonsOfBuildingsInPanel(ref idxMurów, true);
            lastPanelEnabledBuildings = 1;
            ostatniZaznaczonyObiektBudowania = -1;
            ManagerSamouczekScript.mssInstance.WyłączVisual();
        }
        else if (idx == 2)    //Panel z innymi budynkami
        {
            if (idxInne == null)
                return;
            WłączWyłączPanel("UI_BudynkiPanel", true);
            WłączWyłączPanel("UI_LicznikCzasu", false);
            PomocniczeFunkcje.UstawTimeScale(0);
            if (lastPanelEnabledBuildings != -1 && lastPanelEnabledBuildings != 2)
            {
                if (lastPanelEnabledBuildings == 1)
                    EnDisButtonsOfBuildingsInPanel(ref idxMurów, false);
                else if (lastPanelEnabledBuildings == 0)
                    EnDisButtonsOfBuildingsInPanel(ref idxWież, false);
            }
            EnDisButtonsOfBuildingsInPanel(ref idxInne, true);
            lastPanelEnabledBuildings = 2;
            ostatniZaznaczonyObiektBudowania = -1;
            ManagerSamouczekScript.mssInstance.WyłączVisual();
        }
        else    //Wyłącz panel
        {
            if (lastPanelEnabledBuildings == 1)
                EnDisButtonsOfBuildingsInPanel(ref idxMurów, false);
            else if (lastPanelEnabledBuildings == 0)
                EnDisButtonsOfBuildingsInPanel(ref idxWież, false);
            else if (lastPanelEnabledBuildings == 2)
                EnDisButtonsOfBuildingsInPanel(ref idxInne, false);
            WłączWyłączPanel("UI_BudynkiPanel", false);
            WłączWyłączPanel("UI_LicznikCzasu", true);
            PomocniczeFunkcje.UstawTimeScale(1);
            lastPanelEnabledBuildings = -1;
            ostatniZaznaczonyObiektBudowania = -1;
            ManagerSamouczekScript.mssInstance.WyłączVisual();
        }
    }
    private void EnDisButtonsOfBuildingsInPanel(ref ushort[] tabOfBuildToChange, bool willEnable = false)
    {
        StrukturaBudynkuWTab[] tab = PomocniczeFunkcje.spawnBudynki.ZablokowaneBudynki;
        if (wielkosćButtonu == 0)
        {
            RectTransform rt = tab[tabOfBuildToChange[0]].przycisk.GetComponent<RectTransform>();
            wielkosćButtonu = (byte)(Mathf.CeilToInt(rt.sizeDelta.y * rt.localScale.y));
        }
        for (ushort i = 0; i < tabOfBuildToChange.Length; i++)
        {
            tab[tabOfBuildToChange[i]].przycisk.gameObject.SetActive(willEnable);
        }

        iloscButtonow = (byte)(tabOfBuildToChange.Length-2);
    }
    public void WygenerujIPosortujTablice() //Tworzy i sortuje tablicę budynków, które gracz może postawić
    {
        Button b = Resources.Load<Button>("UI/PrzyciskBudynku");
        trBudynkówŁącze = GameObject.Find("Canvas/UIGry/UI_BudynkiPanel/Maska/RodzicButtonów").transform.GetComponent<RectTransform>();
        StrukturaBudynkuWTab[] tab = PomocniczeFunkcje.spawnBudynki.ZablokowaneBudynki;
        List<ushort> murki = null;
        List<ushort> wieże = null;
        List<ushort> inne = null;

        for (ushort i = 0; i < tab.Length; i++)
        {
            Button tb = GameObject.Instantiate(b);
            KonkretnyNPCStatyczny knpcs = PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[tab[i].indexBudynku].GetComponent<KonkretnyNPCStatyczny>();
            if (knpcs.obrazekDoBudynku != null)
            {
                tb.image.sprite = knpcs.obrazekDoBudynku;
            }
            tb.transform.SetParent(trBudynkówŁącze.transform);
            tab[i].DajButton(ref tb);

            switch (knpcs.typBudynku)
            {
                case TypBudynku.Mur:
                    if (murki == null)
                        murki = new List<ushort>();
                    murki.Add(i);
                    break;
                case TypBudynku.Wieża:
                    if (wieże == null)
                        wieże = new List<ushort>();
                    wieże.Add(i);
                    break;
                default:
                    if (inne == null)
                        inne = new List<ushort>();
                    inne.Add(i);
                    break;
            }
            tb.gameObject.SetActive(false);
            if (murki != null)
                idxMurów = murki.ToArray();
            if (wieże != null)
                idxWież = wieże.ToArray();
            if (inne != null)
                idxInne = inne.ToArray();
        }
    }
    public void AktywujDezaktywujPrzyciskPaneliBudynku(byte i = 255, bool stan = false)
    {
        if (i == 255)
        {
            Button b = ui_down.transform.Find("kupno_wieza").GetComponent<Button>();
            b.interactable = stan;
            b = ui_down.transform.Find("kupno_mur").GetComponent<Button>();
            b.interactable = stan;
            b = ui_down.transform.Find("kupno_inne").GetComponent<Button>();
            b.interactable = stan;
        }
        else
        {
            Button button;
            switch (i)
            {
                case 0:
                    button = ui_down.transform.Find("kupno_wieza").GetComponent<Button>();
                    button.interactable = stan;
                    break;
                case 1:
                    button = ui_down.transform.Find("kupno_mur").GetComponent<Button>();
                    button.interactable = stan;
                    break;
                case 2:
                    button = ui_down.transform.Find("kupno_inne").GetComponent<Button>();
                    button.interactable = stan;
                    break;
            }
        }
    }
    #endregion
    #region Rklamy, Skrzynki i ekwipunek
    public void UżyjKlikniętegoPrzedmiotu()
    {

        if (wybranaNagroda > -1)
        {
            if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki == 255)
            {
                ManagerSamouczekScript.mssInstance.ZmiennaPomocnicza = (sbyte)(wybranaNagroda + 2);
            }
            PomocniczeFunkcje.managerGryScript.UzyciePrzedmiotu((byte)wybranaNagroda);
        }
        if (PomocniczeFunkcje.managerGryScript.ekwipunekGracza[wybranaNagroda].ilośćDanejNagrody == 0)
            wybranaNagroda = -1;
    }
    public void UstawButtonNagrody(byte idxButtonaNagrody, ushort ilość)  //Wyszukuje nazwę w opcjach dropdawna
    {
        bool czyAktywuje = (ilość <= 0) ? false : true;
        if (przyciskiNagród[idxButtonaNagrody].interactable != czyAktywuje)
        {
            przyciskiNagród[idxButtonaNagrody].interactable = czyAktywuje;
        }
        przyciskiNagród[idxButtonaNagrody].GetComponentInChildren<Text>().text = ilość.ToString();
    }
    public void KliknąłemReklame(int idx)
    {
        PomocniczeFunkcje.managerGryScript.KlikniętaReklamaButtonSkrzynki((byte)idx);
    }
    public void SkrzynkaKlik(int idx)
    {
        PomocniczeFunkcje.managerGryScript.KliknietyPrzycisk();
        buttonSkrzynki[idx].skrzynkaB.interactable = false;
    }
    public void KliknietyPrzyciskRewardZPoziomuZReklama()
    {
        rekZaWyzszaNagrode.gameObject.SetActive(false);
        PomocniczeFunkcje.managerGryScript.KliknietyButtonZwiekszeniaNagrodyPoLvlu();
    }
    public void WybierzWybranyPrzedmiot(int nagroda)
    {
        if (PomocniczeFunkcje.managerGryScript.ekwipunekGracza[nagroda].ilośćDanejNagrody > 0)
        {
            wybranaNagroda = (sbyte)nagroda;
        }
        UżyjKlikniętegoPrzedmiotu();
    }
    public void UstawDropDownEkwipunku(ref PrzedmiotScript[] ps)
    {
        for (byte i = 0; i < przyciskiNagród.Length; i++)
        {
            przyciskiNagród[i].interactable = false;
        }
        for (ushort i = 0; i < ps.Length; i++)
        {
            switch (ps[i].typPrzedmiotu)
            {
                case TypPrzedmiotu.Coiny:
                    if (ps[i].ilośćDanejNagrody > 0)
                    {
                        przyciskiNagród[0].interactable = true;
                        przyciskiNagród[0].GetComponentInChildren<Text>().text = ps[i].ilośćDanejNagrody.ToString();
                    }
                    ps[i].obrazek = przyciskiNagród[0].image;
                    break;
                case TypPrzedmiotu.CudOcalenia:
                    if (ps[i].ilośćDanejNagrody > 0)
                    {
                        przyciskiNagród[1].interactable = true;
                        przyciskiNagród[1].GetComponentInChildren<Text>().text = ps[i].ilośćDanejNagrody.ToString();
                    }
                    ps[i].obrazek = przyciskiNagród[1].image;
                    break;
                case TypPrzedmiotu.SkrócenieCzasuDoSkrzynki:
                    if (ps[i].ilośćDanejNagrody > 0)
                    {
                        przyciskiNagród[3].interactable = true;
                        przyciskiNagród[3].GetComponentInChildren<Text>().text = ps[i].ilośćDanejNagrody.ToString();
                    }
                    ps[i].obrazek = przyciskiNagród[3].image;
                    break;
                case TypPrzedmiotu.DodatkowaNagroda:
                    if (ps[i].ilośćDanejNagrody > 0)
                    {
                        przyciskiNagród[2].interactable = true;
                        przyciskiNagród[2].GetComponentInChildren<Text>().text = ps[i].ilośćDanejNagrody.ToString();
                    }
                    ps[i].obrazek = przyciskiNagród[2].image;
                    break;
            }
        }
    }
    #endregion
    #region Obsluga Opcje i Creditsy
    public void OptionsMenu(bool actButton)
    {
        menu.enabled = !actButton;
        optionsMenu.enabled = actButton;
        if (!actButton)
        {
            PomocniczeFunkcje.ZapisDanychOpcje();
        }
    }
    public void ZmieńJęzyk()
    {
        lastIdxJezyka++;
        if (lastIdxJezyka < 0 || lastIdxJezyka > 1)  //Tu należy zmienić liczbę jesli dodany zostanie nowy jezyk
            lastIdxJezyka = 0;
        PomocniczeFunkcje.managerGryScript.ZmianaJęzyka((byte)lastIdxJezyka);
    }
    public void WłączWyłączLicznikFPS()
    {
        if (lFPS.gameObject.activeSelf)
        {
            lFPS.gameObject.SetActive(false);
        }
        else
        {
            lFPS.gameObject.SetActive(true);
        }
    }
    public void UstawWartoscFPS(short val)
    {
        lFPS.text = "FPS: " + val;
    }
    public void ObsluzCreditsy(bool wł)
    {
        if (!wł)
        {
            WłączWyłączPanel("Cretidsy", false);
            WłączWyłączPanel(menu.transform.name, true);
            PomocniczeFunkcje.muzyka.WłączWyłączClip(ref PomocniczeFunkcje.muzyka.muzykaTła, true, "Tło_None");
        }
        else
        {
            WłączWyłączPanel("Cretidsy", true);
            WłączWyłączPanel(menu.transform.name, false);
            PomocniczeFunkcje.muzyka.WłączWyłączClip(ref PomocniczeFunkcje.muzyka.muzykaTła, true, "Tło_None_PoMenu");
        }
    }
    public void UstawGłośność()
    {
        PomocniczeFunkcje.muzyka.UstawGłośnośćGry(sliderDźwięku.value);
    }
    #endregion
    #region Klikniety lub odpalony Przycisk
    public void KliknijPrzyciskKupnaBudynku()
    {
        kup.interactable = false;
        PomocniczeFunkcje.spawnBudynki.OdblokujBudynek(true);
        if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki == 255)
        {
            ManagerSamouczekScript.mssInstance.ZmiennaPomocnicza = 10;
        }
    }
    public void KliknijPrzyciskPostawBudynek()
    {
        if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki == 255)
        {
            if (lastPanelEnabledBuildings > -1)
            {
                ManagerSamouczekScript.mssInstance.ZmiennaPomocnicza = 12;
            }
        }
        PomocniczeFunkcje.spawnBudynki.WybierzBudynekDoPostawienia();
        kup.interactable = false;
        stawiajBudynek.interactable = false;
        WłączWyłączPanelBudynków(-1);
    }
    public void WyłączPanelBudynków()
    {
        WłączWyłączPanelBudynków(-1);
        PomocniczeFunkcje.spawnBudynki.AktIdxBudZab = -1;
        WłączWyłączPanel(uiBudynkiPanel.transform.name, false);
    }
    public void WłWyłPanelReklam(bool czyWłPanel)
    {
        menu.enabled = !czyWłPanel;// SetActive(!czyWłPanel);
        reklamyPanel.enabled = czyWłPanel; // SetActive(czyWłPanel);
    }
    public void OdpalButtonyAkademii(bool czyOdpalac = true)
    {
        if (buttonAZycie.gameObject.activeInHierarchy != czyOdpalac)
        {
            buttonAZycie.gameObject.SetActive(czyOdpalac);
            buttonAAtak.gameObject.SetActive(czyOdpalac);
            buttonAObrona.gameObject.SetActive(czyOdpalac);
            if (ManagerGryScript.iloscCoinów < 200)
            {
                buttonAZycie.interactable = false;
                buttonAAtak.interactable = false;
                buttonAObrona.interactable = false;
            }
            else
            {
                buttonAZycie.interactable = true;
                buttonAAtak.interactable = true;
                buttonAObrona.interactable = true;
            }
        }
    }
    public void ObrótBudynku()
    {
        PomocniczeFunkcje.spawnBudynki.ObróćBudynek();
    }
    public void UstawPrzyciskObrotu(bool wartośćPrzycisku)
    {
        rotacjaBudynku.gameObject.SetActive(wartośćPrzycisku);
    }
    public void KliknalemButtonRozwoju(int indeksButtonu)   //1 - Zycie, 2 - Atak, 3 - Obrona
    {
        PomocniczeFunkcje.managerGryScript.RozwójBudynkow((byte)indeksButtonu);
    }
    public void QuitGame()
    {
        if (!ManagerSamouczekScript.byloZaladowane)
            PomocniczeFunkcje.ZapiszDane();
        Application.Quit();
    }
    #endregion
}
