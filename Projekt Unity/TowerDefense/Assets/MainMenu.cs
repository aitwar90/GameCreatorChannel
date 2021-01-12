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
    private byte wielkosćButtonu = 100;
    private byte offsetBB = 1;
    private byte iloscButtonow = 1;
    private RectTransform trBudynkówŁącze = null;
    private ushort[] idxWież = null;
    private ushort[] idxMurów = null;
    private ushort[] idxInne = null;
    private sbyte lastPanelEnabledBuildings = -1;
    #endregion
    #region Obiekty ładowane
    public Slider sliderDźwięku;
    private GameObject menu;
    private GameObject uiGry;
    private GameObject optionsMenu;
    private GameObject poGraj;
    private GameObject reklamyPanel;
    private GameObject ui_down;
    private GameObject goPanel;
    private GameObject samouczekPanel;
    private Button przyciskWznów;
    private Vector3 lastPosCam = Vector3.zero;
    private sbyte wybranaNagroda = -1;
    public sbyte lastIdxJezyka = 0;
    private static MainMenu singelton = null;
    private bool odpalonyPanel = false;
    private RectTransform rectHpBar;
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
            return menu.activeInHierarchy;
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
        menu = this.transform.Find("Menu/MainMenu").gameObject;
        uiGry = this.transform.Find("UIGry").gameObject;
        optionsMenu = this.transform.Find("Menu/OptionsMenu").gameObject;
        poGraj = this.transform.Find("Menu/PoGraj").gameObject;
        reklamyPanel = this.transform.Find("Menu/PanelSkrzynki").gameObject;
        ui_down = uiGry.transform.Find("ui_down").gameObject;
        przyciskWznów = this.transform.Find("Menu/MainMenu/ResumeButton").GetComponent<Button>();
        actWybEpoka = this.transform.Find("Menu/PoGraj/AktualnieWybEpoka").GetComponent<Text>();
        uiBudynkiPanel = uiGry.transform.Find("UI_BudynkiPanel").gameObject;
        rectHpBar = uiGry.transform.Find("PasekZyciaGłównegoBudynku/Green").GetComponent<RectTransform>();
        goPanel = uiGry.transform.Find("GameOver Panel").gameObject;
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
        WłączWyłączPanel(new string[] {goPanel.name, uiBudynkiPanel.name, ui_down.name, uiGry.name, optionsMenu.name,
        reklamyPanel.name, poGraj.name, samouczekPanel.name, "Cretidsy"}, false);
    }

    #region Obsługa paneli UI, Czy mogę przesuwać kamerę (), Pasek HP
    public void WłączWyłączPanel(string panel, bool czyWłączyć)
    {
        if (panel == menu.name)
        {
            menu.SetActive(czyWłączyć);
        }
        else if (panel == uiGry.name)
        {
            uiGry.SetActive(czyWłączyć);
        }
        else if (panel == optionsMenu.name)
        {
            optionsMenu.SetActive(czyWłączyć);
        }
        else if (panel == poGraj.name)
        {
            poGraj.SetActive(czyWłączyć);
        }
        else if (panel == reklamyPanel.name)
        {
            reklamyPanel.SetActive(czyWłączyć);
        }
        else if (ui_down.name == panel)
        {
            ui_down.SetActive(czyWłączyć);
            licznikCzasuDoFali.transform.parent.gameObject.SetActive(czyWłączyć);
        }
        else if (goPanel.name == panel)
        {
            goPanel.SetActive(czyWłączyć);
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
            this.transform.Find("Menu/Cretidsy").gameObject.SetActive(czyWłączyć);
        }
    }
    public void WłączWyłączPanel(string[] panel, bool czyWłączyć)
    {
        for (byte i = 0; i < panel.Length; i++)
        {
            if (panel[i] == menu.name)
            {
                menu.SetActive(czyWłączyć);
            }
            else if (panel[i] == uiGry.name)
            {
                uiGry.SetActive(czyWłączyć);
            }
            else if (panel[i] == optionsMenu.name)
            {
                optionsMenu.SetActive(czyWłączyć);
            }
            else if (panel[i] == poGraj.name)
            {
                poGraj.SetActive(czyWłączyć);
            }
            else if (panel[i] == reklamyPanel.name)
            {
                reklamyPanel.SetActive(czyWłączyć);
            }
            else if (ui_down.name == panel[i])
            {
                ui_down.SetActive(czyWłączyć);
                licznikCzasuDoFali.transform.parent.gameObject.SetActive(czyWłączyć);
            }
            else if (goPanel.name == panel[i])
            {
                goPanel.SetActive(czyWłączyć);
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
                this.transform.Find("Menu/Cretidsy").gameObject.SetActive(czyWłączyć);
            }
        }
    }
    public void PrzełączUI(bool aktywujeMenu)
    {
        przyciskWznów.interactable = true;
        menu.SetActive(aktywujeMenu);
        uiGry.SetActive(!aktywujeMenu);
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
        string[] s = parametry.Split('_');
        if (s[0] == "STATYCZNY")
        {
            PanelStatyczny ps = (PanelStatyczny)panelStatyczny;
            ps.KNPCS = knpcs;
            RectTransform r = ps.GetComponent<RectTransform>();
            if (s[1] == "True")  //Odblokuj naprawe budynku
            {
                ps.naprawButton.interactable = true;
            }
            else
            {
                ps.naprawButton.interactable = false;
            }

            ps.UstawDane(new string[] { s[2], s[3], s[4], s[5], s[6] });

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
            ps.UstawDane(new string[] { s[1], s[2], s[3], s[4], s[5], s[6], s[7] });
        }
    }
    public bool CzyMogePrzesuwaćKamere()
    {
        if (uiGry.activeInHierarchy)
        {
            if (CzyOdpaloneMenu || goPanel.activeInHierarchy || CzyAktywnyPanelZBudynkami())
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
    #endregion
    #region Po Scenie
    public void OdpalPoScenie(bool czyOdpalamPoScenie)
    {
        if (czyOdpalamPoScenie)
        {
            menu.SetActive(false);
            poGraj.SetActive(true);

            poziomWEpoce.text = PomocniczeFunkcje.odblokowanyPoziomEpoki.ToString();
            actWybEpoka.text = PomocniczeFunkcje.odblokowaneEpoki.ToString();
            PomocniczeFunkcje.muzyka.WłączWyłączClip(true, "Tło_None_PoMenu");
        }
        else
        {
            menu.SetActive(true);
            poGraj.SetActive(false);
            PomocniczeFunkcje.muzyka.WłączWyłączClip(true, "Tło_None");
        }
    }
    public void ResetSceny()
    {
        int unSceneIdx = ObslugaScenScript.indeksAktualnejSceny;
        PomocniczeFunkcje.ResetujWszystko();
        SceneManager.UnloadSceneAsync(unSceneIdx);
        StartCoroutine(CzekajAz());
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
        byte poziom = (byte)int.Parse(poziomWEpoce.text);
        PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki = poziom;
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
        poGraj.SetActive(false);
        PrzełączUI(false);
    }
    public void OdpalSamouczek()
    {
        PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki = 255;
        PomocniczeFunkcje.managerGryScript.aktualnaEpoka = Epoki.EpokaKamienia;
        if (SceneManager.sceneCount == 1)
        {
            byte poziom = PomocniczeFunkcje.managerGryScript.GetComponent<ObslugaScenScript>().ZwróćScenęSamouczka();
            if(poziom < SceneManager.sceneCountInBuildSettings)
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
        poGraj.SetActive(false);
        PrzełączUI(false);
        samouczekPanel.GetComponent<ManagerSamouczekScript>().ŁadujDaneSamouczek();
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
        if (zresetuj)
        {
            trBudynkówŁącze.anchoredPosition = new Vector3(trBudynkówŁącze.anchoredPosition.x, -100);
            return;
        }
        Vector2 sOff = Vector2.zero;
        sOff.y += wartość;
        Vector3 tmp = trBudynkówŁącze.anchoredPosition = trBudynkówŁącze.anchoredPosition + sOff;
        short t = (short)(-100 - ((wielkosćButtonu + offsetBB) * iloscButtonow));
        if (tmp.y <= -100 && tmp.y >= t)
        {
            trBudynkówŁącze.anchoredPosition = tmp;
        }
        else
        {
            if (tmp.y > -100)
            {
                trBudynkówŁącze.anchoredPosition = new Vector3(trBudynkówŁącze.anchoredPosition.x, -100);
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
            iloscButtonow = 1;
        }
    }
    private void EnDisButtonsOfBuildingsInPanel(ref ushort[] tabOfBuildToChange, bool willEnable = false)
    {
        StrukturaBudynkuWTab[] tab = PomocniczeFunkcje.spawnBudynki.ZablokowaneBudynki;
        for (ushort i = 0; i < tabOfBuildToChange.Length; i++)
        {
            tab[tabOfBuildToChange[i]].przycisk.gameObject.SetActive(willEnable);
        }
        iloscButtonow = (byte)tabOfBuildToChange.Length;
    }
    public void WygenerujIPosortujTablice() //Tworzy i sortuje tablicę budynków, które gracz może postawić
    {
        Button b = Resources.Load<Button>("UI/PrzyciskBudynku");
        wielkosćButtonu = (byte)b.GetComponent<RectTransform>().sizeDelta.y;
        trBudynkówŁącze = GameObject.Find("Canvas/UIGry/UI_BudynkiPanel/RodzicButtonów").transform.GetComponent<RectTransform>();
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
    #endregion
    #region Rklamy, Skrzynki i ekwipunek
    public void UżyjKlikniętegoPrzedmiotu()
    {

        if (wybranaNagroda > -1)
        {
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
        menu.SetActive(!actButton);
        optionsMenu.SetActive(actButton);
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
            WłączWyłączPanel(menu.name, true);
            PomocniczeFunkcje.muzyka.WłączWyłączClip(ref PomocniczeFunkcje.muzyka.muzykaTła, true, "Tło_None");
        }
        else
        {
            WłączWyłączPanel("Cretidsy", true);
            WłączWyłączPanel(menu.name, false);
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
        PomocniczeFunkcje.spawnBudynki.OdblokujBudynek(lastPanelEnabledBuildings, true);
    }
    public void KliknijPrzyciskPostawBudynek()
    {
        PomocniczeFunkcje.spawnBudynki.WybierzBudynekDoPostawienia();
        kup.interactable = false;
        stawiajBudynek.interactable = false;
        WłączWyłączPanelBudynków(-1);
    }
    public void WyłączPanelBudynków()
    {
        WłączWyłączPanelBudynków(-1);
        PomocniczeFunkcje.spawnBudynki.AktIdxBudZab = -1;
        WłączWyłączPanel(uiBudynkiPanel.name, false);
    }
    public void WłWyłPanelReklam(bool czyWłPanel)
    {
        menu.SetActive(!czyWłPanel);
        reklamyPanel.SetActive(czyWłPanel);
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
        PomocniczeFunkcje.ZapiszDane();
        Application.Quit();
    }
    #endregion
}
