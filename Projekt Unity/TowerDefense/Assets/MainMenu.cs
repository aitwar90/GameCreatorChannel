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
    public Button ostatniStawianyBudynekButton;
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
    public Text winTXT;
    public Text loseTXT;
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
    //private sbyte ostatniZaznaczonyObiektBudowania = -1;
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
    public static MainMenu singelton = null;
    private bool odpalonyPanel = false;
    private RectTransform rectHpBar;
    private sbyte kPoziomDoZaladowania = -1;
    #endregion
    #region Reklamy
    public Sprite[] otwarteObrazki;
    public Text tekstCoWygrales;
    public AnimationClip animacjaOtwarciaSkrzynki;
    #endregion
    private short[] ostatniaWartośćPolozeniaPanelu = { 0, 0, 0 }; //Położenie wież, położenie murków, położenie reszty
    private StatystykiScript statystykiScript;
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
    public bool CzyPostProcesing
    {
        set
        {
            Toggle t = this.transform.Find("Menu/OptionsMenu/GrafikaPanel/CzyPostProcessing").GetComponent<Toggle>();
            t.isOn = value;
            MoveCameraScript.mscInstance.UstawPostProcessing(value, true);
        }
        get
        {
            return this.transform.Find("Menu/OptionsMenu/GrafikaPanel/CzyPostProcessing").GetComponent<Toggle>().isOn;
        }
    }
    public bool CzyOdpaloneMenu
    {
        get
        {
            return menu.activeInHierarchy;
        }
    }
    public sbyte OdpalonyPanelBudynków
    {
        get
        {
            return lastPanelEnabledBuildings;
        }
    }
    public PanelDynamiczny GetKontenerKomponentówDynamic
    {
        get
        {
            if (panelDynamiczny.gameObject.activeInHierarchy)
            {
                return (PanelDynamiczny)panelDynamiczny;
            }
            else
                return null;
        }
    }
    public PanelStatyczny GetKontenerKomponentówStatic
    {
        get
        {
            if (panelStatyczny.gameObject.activeInHierarchy)
            {
                return (PanelStatyczny)panelStatyczny;
            }
            else
                return null;
        }
    }
    public bool CzyOdpalonyPanelReklam
    {
        get
        {
            return reklamyPanel.activeInHierarchy;
        }
    }
    #endregion
    public void Awake()
    {
        Debug.Log("Rozpoczynam działanie Akawe Canvas");
        if (singelton == null)
        {
            singelton = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }
    void Start()
    {
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
        statystykiScript = goPanel.transform.Find("Statystyki").GetComponent<StatystykiScript>();
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
        reklamyPanel.name, poGraj.name, winTXT.name, loseTXT.name, samouczekPanel.name, "Cretidsy"},
        false);
        this.tekstCoWygrales.transform.parent.gameObject.SetActive(false);
    }
    #region Obsługa paneli UI, Czy mogę przesuwać kamerę (), Pasek HP
    ///<summary>Metoda włącza lub wyłącza panel zgodny z podanymi parametrami.</summary>
    ///<param name="panel">Nazwa panelu, którego widoczność ma zostać zmodyfikowana.</param>
    ///<param name="czyWłączyć">Parametr określa co ma zostać zrobione z panelem wysłanym w parametrze "panel".</param>
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
        else if (panel == winTXT.name)
        {
            winTXT.gameObject.SetActive(czyWłączyć);
        }
        else if (panel == loseTXT.name)
        {
            loseTXT.gameObject.SetActive(czyWłączyć);
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
    ///<summary>Metoda włącza lub wyłącza panel zgodny z podanymi parametrami.</summary>
    ///<param name="panel">Tablica nazw paneli, którego widoczność ma zostać zmodyfikowana.</param>
    ///<param name="czyWłączyć">Parametry określające co ma zostać zrobione z panelami wysłanymi w parametrach "panel".</param>
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
            else if (panel[i] == winTXT.name)
            {
                winTXT.gameObject.SetActive(czyWłączyć);
            }
            else if (panel[i] == loseTXT.name)
            {
                loseTXT.gameObject.SetActive(czyWłączyć);
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
    ///<summary>Metoda przełącza UI między Menu a UIGry.</summary>
    ///<param name="aktywujeMenu">Czy ma zostać odpalone menu?</param>
    public void PrzełączUI(bool aktywujeMenu)
    {
        if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki != 255)
            przyciskWznów.interactable = true;
        else
            przyciskWznów.interactable = false;
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
            if (ui_down.activeInHierarchy)
                PomocniczeFunkcje.muzyka.WłączWyłączClip(true, PomocniczeFunkcje.TagZEpoka("AmbientWGrze", PomocniczeFunkcje.managerGryScript.aktualnaEpoka));
            else
                PomocniczeFunkcje.muzyka.WłączWyłączClip(true, "Bitwa");
        }
    }
    ///<summary>Metoda ustawia wartość paska HP głównego budynku.</summary>
    ///<param name="wartoscX">Wartość wyrażona (1-0) określająca stan HP (1 - pełne zdrowie, 0 - główny budynek zniszczony).</param>
    public void UstawHPGłównegoPaska(float wartoscX)
    {
        rectHpBar.localScale = new Vector3(wartoscX, 1, 1);
    }
    ///<summary>Metoda ustawia wartość tekstową dla zadanego parametru.</summary>
    ///<param name="nazwaTekstu">Do jakiego komponentu tekstowego ma zostać przypisana wartość tekst.</param>
    ///<param name="tekst">Przypisywany tekst.</param>
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
    ///<summary>Metoda logicznie obsługuje jaki panel ma zostać odpalony oraz jakie informacje do paneli mają zostać wysłane.</summary>
    ///<param name="parametry">Lista wartości parametrów, którę będą przypisane do odpalanego panelu.</param>
    ///<param name="pos">Pozycja, gdzie dany panel powinien zostać odpalony.</param>
    ///<param name="knpcs">Jeśli ma zostać odpalony panel klikniętego budynku postawionego w świecie to przesyła referencję danego budynku.</param>
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
            if (s[1] == "True" && ui_down.activeInHierarchy)  //Odblokuj naprawe budynku
            {
                ps.naprawButton.interactable = true;
            }
            else
            {
                ps.naprawButton.interactable = false;
            }

            ps.UstawDane(new string[] { s[2], s[3], s[4], s[5], s[6], s[7] });
            r.position = SprawdźCzyNieWychodziZaObszarEkranu(pos, r.rect.width, r.rect.height);
            ps.gameObject.SetActive(true);
            odpalonyPanel = true;
        }
        else if (s[0] == "DYNAMICZNY")
        {
            PanelDynamiczny ps = (PanelDynamiczny)panelDynamiczny;
            RectTransform r = ps.GetComponent<RectTransform>();
            ps.UstawDane(new string[] { s[1], s[2], s[3] });
            r.position = SprawdźCzyNieWychodziZaObszarEkranu(pos, r.rect.width, r.rect.height);
            ps.gameObject.SetActive(true);
            odpalonyPanel = true;
        }
        else if (s[0] == "PANEL")
        {
            PanelTextuWBudynkach ps = (PanelTextuWBudynkach)panelBudynki;
            ps.UstawDane(new string[] { s[1], s[2], s[3], s[4], s[5], s[6], s[7], s[8] });
        }
    }
    ///<summary>Funkcja zwraca nowe położenie panelu, jeśli ten wykracza poza krawędź ekranu</summary>
    ///<param name="currentKlikPos">Pozycja klikniętego punktu na ekranie.</param>
    ///<param name="szerokość">Szerokość wyświetlanego panelu.</param>
    ///<param name="wysokość">Wysokość wyświetlanego panelu.</param>
    private Vector2 SprawdźCzyNieWychodziZaObszarEkranu(Vector2 currentKlikPos, float szerokość, float wysokość)
    {
        Vector2 scR = new Vector2(Screen.width, Screen.height);
        if (currentKlikPos.y - wysokość < 0) //Dolna krawędź ekranu
        {
            currentKlikPos.y = wysokość + 5;
        }
        if (currentKlikPos.x + szerokość > scR.x)
        {
            currentKlikPos.x = scR.x - szerokość - 5;
        }
        return currentKlikPos;

    }
    ///<summary>Funkcja zwraca informację czy kamera może zostać przemieszczona.</summary>
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
    ///<summary>Funkcja zwraca informację o danej pozycji na ekranie szukanego obiektu.</summary>
    ///<param name="nazwaSzukanegoObiektu">Nazwa obiektu, którego pozycji szukasz.</param>
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
            /*
        case "buttonAŻycie":
            zwracanaPos = buttonAZycie.GetComponent<RectTransform>().position;
            break;
        case "buttonAAtak":
            zwracanaPos = buttonAAtak.GetComponent<RectTransform>().position;
            break;
        case "buttonAObrona":
            zwracanaPos = buttonAObrona.GetComponent<RectTransform>().position;
            break;
            */
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
    ///<summary>Ustaw poszczególne wartości dla statystyk.</summary>
    ///<param name="tablicaWartości">Wartości dla poszczególnych elementów (0-Badania, 1-Rozwój z akademii, 2-Budowa budynków, 3-Naprawa budynków, 4-Zyski z pokonanych wrogów, 5-Ilość nagrody za ukończony poziom).</param>
    public void UstawDaneStatystyk(ref int[] tablicaWartości)
    {
        statystykiScript.UstawWartościIOdpalMnie(ref tablicaWartości);
    }
    ///<summary>Wyłącz panel statystyk.</summary>
    public void WyłączPanelStatystyk()
    {
        statystykiScript.WyłączMnie();
    }
    public void UstawFontDlaStatystyk(ref Font f)
    {
        for(byte i = 0; i <statystykiScript.wartościDlaTekstu.Length; i++)
        {
            statystykiScript.wartościDlaTekstu[i].font = f;
        }
    }
    #endregion
    #region Po Scenie
    ///<summary>Metoda aktywuje panel PoScenie i deaktywuje Menu panel, lub odwrotnie.</summary>
    ///<param name="czyOdpalamPoScenie">Czy kliknałem Play?.</param>
    public void OdpalPoScenie(bool czyOdpalamPoScenie)
    {
        if (czyOdpalamPoScenie)
        {
            WłączWyłączPanel(menu.name, false);
            WłączWyłączPanel(poGraj.name, true);

            poziomWEpoce.text = PomocniczeFunkcje.odblokowanyPoziomEpoki.ToString();
            actWybEpoka.text = PomocniczeFunkcje.odblokowaneEpoki.ToString();
            PomocniczeFunkcje.muzyka.WłączWyłączClip(true, "Tło_None_PoMenu");
        }
        else
        {
            WłączWyłączPanel(menu.name, true);
            WłączWyłączPanel(poGraj.name, false);
            PomocniczeFunkcje.muzyka.WłączWyłączClip(true, "Tło_None");
        }
    }
    ///<summary>Metoda resetuje dane podczas przełączania sceny (EXIT - nie jest obsługiwana przy samouczku).</summary>
    ///<param name="ładowaćNowąScene">Czy mam ładować nową scenę.</param>
    public void ResetSceny(bool ładowaćNowąScene = true)
    {
        int unSceneIdx = ObslugaScenScript.indeksAktualnejSceny;
        PomocniczeFunkcje.ResetujWszystko();
        SceneManager.UnloadSceneAsync(unSceneIdx);
        if (ładowaćNowąScene)
            StartCoroutine(CzekajAz());
    }
    ///<summary>Metoda resetuje dane podczas przełączania sceny (EXIT - obsługuje tylko samouczek).</summary>
    ///<param name="aktualnyPoziomEpoki">Jesli poziom epoki jest większy niż 100 aktywuję samouczek.</param>
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
    ///<summary>Funckja czeka, aż załadowana będzie tylko scena Menu.</summary>
    public IEnumerator CzekajAz()
    {
        yield return new WaitUntil(() => SceneManager.sceneCount == 1);
        MetodaDoOdpaleniaPoWyczekaniu();
    }
    ///<summary>Metoda roztrzyga, jaka scena powinna zostać załadowana (Samouczek czy normalna).</summary>
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
    ///<summary>Funkcja zwraca informację o ustawionej epoce w panelu poGraj.</summary>
    ///<param name="ustawionaEpoka">Aktualnie ustawiona epoka w poGraj.</param>
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
    ///<summary>Metoda rozpoczyna ładowanie sceny gry.</summary>
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
        poGraj.SetActive(false);
        PrzełączUI(false);
        kPoziomDoZaladowania = -1;
    }
    ///<summary>Metoda rozpoczyna ładowanie sceny samouczka.</summary>
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
        WłączWyłączPanel(poGraj.name, false);
        PrzełączUI(false);
        ManagerSamouczekScript.mssInstance.ŁadujDaneSamouczek();
    }
    ///<summary>Metoda ustawia wizualnie wartość po kliknięciu na button poGraj modyfikujący epokę.</summary>
    ///<param name="czyWyzej">Czy kliknięty button zwięsza wybraną epokę?</param>
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
    ///<summary>Metoda ustawia wizualnie wartość po kliknięciu na button poGraj modyfikujący poziom.</summary>
    ///<param name="czyWyzej">Czy kliknięty button zwięsza wybrany poziom?</param>
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
    ///<summary>Funckja zwraca informację czy Panel z Budynkami jest aktywny.</summary>
    public bool CzyAktywnyPanelZBudynkami()
    {
        if (uiBudynkiPanel.activeInHierarchy)
            return true;
        else
            return false;
    }
    ///<summary>Metoda obsługuje przesunięcie panelu z przyciskami budynków w panelu budynków.</summary>
    ///<param name="wartość">Wartość przesunięcia panelu z przyciskami.</param>
    ///<param name="zresetuj">Czy przyciski mają wrócić do pierwotnej pozycji.</param>
    public void PrzesuńBudynki(float wartość, bool zresetuj = false)
    {
        short wartośćPrzesunięciaY = -290;
        if (zresetuj && wartość == 0)
        {
            trBudynkówŁącze.anchoredPosition = new Vector3(trBudynkówŁącze.anchoredPosition.x, wartośćPrzesunięciaY);
            return;
        }
        Vector2 sOff = Vector2.zero;
        sOff.y += wartość;
        byte offsetBB = 10;
        Vector3 tmp = trBudynkówŁącze.anchoredPosition + sOff;   //Wartość po przesunięciu obiektu o wartość
        short t = (short)(wartośćPrzesunięciaY + ((wielkosćButtonu + offsetBB) * iloscButtonow));   //Obszar po Y wszystkich przycisków
        if (tmp.y >= wartośćPrzesunięciaY && tmp.y <= t)
        {
            trBudynkówŁącze.anchoredPosition = tmp;
            switch (lastPanelEnabledBuildings)
            {
                case 0: //ostatnio otwarty był panel z wieżami
                    ostatniaWartośćPolozeniaPanelu[0] = (short)(tmp.y);
                    break;
                case 1: //ostatnio otwarty był panel z murkami
                    ostatniaWartośćPolozeniaPanelu[1] = (short)(tmp.y);
                    break;
                case 2: //ostatnio otwarty był panel z innymi
                    ostatniaWartośćPolozeniaPanelu[2] = (short)(tmp.y);
                    break;

            }
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
    ///<summary>Metoda włącza, wyłącza lub przełącza panel z budynkami.</summary>
    ///<param name="idx">Odpalany panel z budynkami (0 - wieże), (1 - mury), (2 - Inne), (inny - wyłącza panel).</param>
    public void WłączWyłączPanelBudynków(int idx)
    {
        if (idx > -1 && idx < 3)
            PrzesuńBudynki(ostatniaWartośćPolozeniaPanelu[idx], true);
        else
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
            //ostatniZaznaczonyObiektBudowania = -1;
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
            //ostatniZaznaczonyObiektBudowania = -1;
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
            //ostatniZaznaczonyObiektBudowania = -1;
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
            //ostatniZaznaczonyObiektBudowania = -1;
            ManagerSamouczekScript.mssInstance.WyłączVisual();
        }
    }
    ///<summary>Metoda włącza przyciski budynków danego panelu.</summary>
    ///<param name="tabOfBuildToChange">Tablica indeksów przycisków.</param>
    ///<param name="willEnable">Informacja czy przyciski danej tablicy mają zostać włączone lub wyłączone zgodnie z tym parametrem.</param>
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

        if (tabOfBuildToChange.Length > 1)
            iloscButtonow = (byte)(tabOfBuildToChange.Length - 2);
        else if (tabOfBuildToChange.Length == 1)
            iloscButtonow = (byte)(tabOfBuildToChange.Length - 1);
        else
            iloscButtonow = 0;
    }
    ///<summary>Metoda generuje i sortuje tablice przycisków budynków do panelu budynków.</summary>
    public void WygenerujIPosortujTablice() //Tworzy i sortuje tablicę budynków, które gracz może postawić
    {
        Button b = Resources.Load<Button>("UI/PrzyciskBudynku");
        trBudynkówŁącze = GameObject.Find("Canvas/UIGry/UI_BudynkiPanel/Maska/RodzicButtonów").transform.GetComponent<RectTransform>();
        StrukturaBudynkuWTab[] tab = PomocniczeFunkcje.spawnBudynki.ZablokowaneBudynki;
        List<ushort> murki = null;
        List<ushort> wieże = null;
        List<ushort> inne = null;
        byte poziom = PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki;
        for (ushort i = 0; i < tab.Length; i++)
        {
            KonkretnyNPCStatyczny knpcs = PomocniczeFunkcje.spawnBudynki.wszystkieBudynki[tab[i].indexBudynku].GetComponent<KonkretnyNPCStatyczny>();
            if(knpcs.poziom > poziom && poziom != 255)
                continue;
            else if(poziom == 255)
            {
                if(knpcs.poziom > 1)
                    continue;
            }
            Button tb = GameObject.Instantiate(b);
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
    ///<summary>Metoda blokuje lub odblokowywuje przyciski potrzebne do odpalenia panelu budynków.</summary>
    ///<param name="i">Parametr określający, który przycisk ma zostać zablokowany lub odblokowany.</param>
    ///<param name="stan">Czy ma zostać odblokowany czy zablokowany.</param>
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
            for (byte j = 0; j < przyciskiNagród.Length; j++)
            {
                przyciskiNagród[j].interactable = false;
            }
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
    ///<summary>Metoda rozpoczyna działanie nagrody po kliknięciu przycisku.</summary>
    public void UżyjKlikniętegoPrzedmiotu()
    {
        if (wybranaNagroda > -1)
        {
            if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki == 255)
            {
                if (ManagerSamouczekScript.mssInstance.CzyZgadzaSięIDXGłówny(12)
                || ManagerSamouczekScript.mssInstance.CzyZgadzaSięIDXGłówny(13) ||
                ManagerSamouczekScript.mssInstance.CzyZgadzaSięIDXGłówny(14) ||
                ManagerSamouczekScript.mssInstance.CzyZgadzaSięIDXGłówny(15))
                {
                    ManagerSamouczekScript.mssInstance.ZmiennaPomocnicza = (sbyte)(wybranaNagroda + 2);
                }
            }
            PomocniczeFunkcje.managerGryScript.UzyciePrzedmiotu((byte)wybranaNagroda);
        }
        if (PomocniczeFunkcje.managerGryScript.ekwipunekGracza[wybranaNagroda].ilośćDanejNagrody == 0)
            wybranaNagroda = -1;
    }
    ///<summary>Metoda ustawia przycisk nagrody.</summary>
    ///<param name="idxButtonaNagrody">Parametr określający, który przycisk ma zostać ustawiony (0-monety), (1-cud ocalenia), (2-dodatkowa nagroda), (3-skrócenie czasu skrzynki).</param>
    ///<param name="ilość">Parametr określający ilość nagrody.</param>
    public void UstawButtonNagrody(byte idxButtonaNagrody, ushort ilość)  //Wyszukuje nazwę w opcjach dropdawna
    {
        bool czyAktywuje = (ilość <= 0) ? false : true;
        if (przyciskiNagród[idxButtonaNagrody].interactable != czyAktywuje)
        {
            przyciskiNagród[idxButtonaNagrody].interactable = czyAktywuje;
        }
        przyciskiNagród[idxButtonaNagrody].GetComponentInChildren<Text>().text = ilość.ToString();
    }
    ///<summary>Metoda rozpoczyna proces odtwarzania reklamy w Panelu Skrzynki.</summary>
    ///<param name="idx">Indeks klikniętej skrzynki.</param>
    public void KliknąłemReklame(int idx)
    {
        PomocniczeFunkcje.managerGryScript.KlikniętaReklamaButtonSkrzynki((byte)idx);
    }
    ///<summary>Metoda rozpoczyna losowanie nagrody z Panelu Skrzynki.</summary>
    ///<param name="idx">Indeks klikniętej skrzynki.</param>
    public void SkrzynkaKlik(int idx)
    {
        PomocniczeFunkcje.managerGryScript.KliknietyPrzycisk();
        buttonSkrzynki[idx].skrzynkaB.interactable = false;
        //Kliknąłem skrzynkę
        StartCoroutine(PodmieńObrazekSkrzynki(buttonSkrzynki[idx].skrzynkaB.transform.parent.Find("Skrzynka_obrazek").GetComponent<Image>(), (byte)idx));
    }
    ///
    ///<summary>Funkcja zmienia wartości dla obrazka i go podmienia</summary>
    ///<param name="obrazek">Sprite jaki ma zostać podmieniony.</param>
    private IEnumerator PodmieńObrazekSkrzynki(Image obrazek, byte idx)
    {
        Animation a = obrazek.GetComponent<Animation>();
        a.clip = animacjaOtwarciaSkrzynki;
        a.wrapMode = WrapMode.Once;
        a.Play();
        yield return new WaitUntil(() => !a.isPlaying);  //Czekaj na zakończenie animacji
        obrazek.sprite = this.otwarteObrazki[1];
        this.tekstCoWygrales.text = PomocniczeFunkcje.managerGryScript.ekwipunekGracza[idx].nazwaPrzedmiotu;
        this.tekstCoWygrales.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        this.tekstCoWygrales.transform.parent.gameObject.SetActive(false);
    }
    private void ResetImagesSkrzynkiImage()
    {
        for (byte i = 0; i < buttonSkrzynki.Length; i++)
        {
            buttonSkrzynki[i].skrzynkaB.transform.parent.Find("Skrzynka_obrazek").GetComponent<Image>().sprite = this.otwarteObrazki[0];
        }
    }
    /// ///<summary>Rozpoczyna działanie otrzymania dodatkowej nagrody za skończony poziom z poziomu gry za obejrzenie reklamy</summary>
    public void KliknietyPrzyciskRewardZPoziomuZReklama()
    {
        rekZaWyzszaNagrode.gameObject.SetActive(false);
        PomocniczeFunkcje.managerGryScript.KliknietyButtonZwiekszeniaNagrodyPoLvlu();
    }
    ///<summary>Metoda rozpoczyna proces użycia danej nagrody podanej w parametrze (DaneGry).</summary>
    ///<param name="nagroda">Indeks klikniętej nagrody (0-monety), (1-cud ocalenia), (2-dodatkowa nagroda), (3-skrócenie czasu skrzynki).</param>
    public void WybierzWybranyPrzedmiot(int nagroda)
    {
        if (PomocniczeFunkcje.managerGryScript.ekwipunekGracza[nagroda].ilośćDanejNagrody > 0)
        {
            wybranaNagroda = (sbyte)nagroda;
        }
        UżyjKlikniętegoPrzedmiotu();
    }
    ///<summary>Metoda ustawia przyciski nagród (Dane Gry) przy starcie poziomu.</summary>
    ///<param name="ps">Referencja tablicy możliwych nagród gracza.</param>
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
    ///<summary>Metoda włącza i wyłącza panel opcji.</summary>
    ///<param name="actButton">Czy odpalać opcje?</param>
    public void OptionsMenu(bool actButton)
    {
        WłączWyłączPanel(menu.name, !actButton);
        WłączWyłączPanel(optionsMenu.name, actButton);
        if (!actButton)
        {
            PomocniczeFunkcje.ZapisDanychOpcje();
        }
    }
    ///<summary>Metoda rozpoczyna zmianę jezyka interfejsu.</summary>
    public void ZmieńJęzyk()
    {
        lastIdxJezyka++;
        //1 - Polski, 2 - Angielski, 3 - Rosyjski, 4 - Ukraiński
        if (lastIdxJezyka < 0 || lastIdxJezyka > 3)  //Tu należy zmienić liczbę jesli dodany zostanie nowy jezyk
            lastIdxJezyka = 0;
        PomocniczeFunkcje.managerGryScript.ZmianaJęzyka((byte)lastIdxJezyka);
    }
    ///<summary>Metoda aktywuje lub deaktywuje licznik FPS.</summary>
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
    ///<summary>Metoda aktualizuje licznik FPS.</summary>
    ///<param name="val">Wartość jaka zostanie wyświetlona w liczniku FPS</param>
    public void UstawWartoscFPS(short val)
    {
        lFPS.text = "FPS: " + val;
    }
    ///<summary>Metoda włącza lub wyłącza panel Creditsy.</summary>
    ///<param name="wł">Włączyć creditsy?</param>
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
    ///<summary>Metoda ustawia głośność dźwięków aplikacji.</summary>
    public void UstawGłośność()
    {
        PomocniczeFunkcje.muzyka.UstawGłośnośćGry(sliderDźwięku.value);
    }
    #endregion
    #region Klikniety lub odpalony Przycisk
    ///<summary>Metoda rozpoczyna proces kupna budynku.</summary>
    public void KliknijPrzyciskKupnaBudynku()
    {
        kup.interactable = false;
        PomocniczeFunkcje.spawnBudynki.OdblokujBudynek(true);
        if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki == 255)
        {
            if (ManagerSamouczekScript.mssInstance.CzyZgadzaSięIDXGłówny(8))
            {
                ManagerSamouczekScript.mssInstance.ZmiennaPomocnicza = 10;
            }
        }
    }
    ///<summary>Metoda rozpoczyna proces ustawiania budynku.</summary>
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
    ///<summary>Metoda wyłącza Panel budynków.</summary>
    public void WyłączPanelBudynków()
    {
        WłączWyłączPanelBudynków(-1);
        PomocniczeFunkcje.spawnBudynki.AktIdxBudZab = -1;
        WłączWyłączPanel(uiBudynkiPanel.transform.name, false);
    }
    ///<summary>Metoda włącza lub wyłącza panel Reklam.</summary>
    ///<param name="czyWłPanel">Włączyć panel reklam?</param>
    public void WłWyłPanelReklam(bool czyWłPanel)
    {
        WłączWyłączPanel(menu.name, !czyWłPanel);
        WłączWyłączPanel(reklamyPanel.name, czyWłPanel);
        if (!czyWłPanel) //Resetuj obrazki
        {
            ResetImagesSkrzynkiImage();
        }
    }
    ///<summary>Metoda włącza lub wyłącza przyciski służące do ulepszania budynków.</summary>
    ///<param name="czyOdpalac">Włączyć przyciski?</param>
    public void OdpalButtonyAkademii(bool czyOdpalac = true)
    {
        if (buttonAZycie.gameObject.activeInHierarchy != czyOdpalac)
        {
            buttonAZycie.gameObject.SetActive(czyOdpalac);
            buttonAAtak.gameObject.SetActive(czyOdpalac);
            buttonAObrona.gameObject.SetActive(czyOdpalac);
            if (ManagerGryScript.iloscCoinów < PomocniczeFunkcje.managerGryScript.kosztRozwojuAkademii)
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
    ///<summary>Metoda rozpoczyna proces obrotu przygotowanego do postawienia budynku.</summary>
    public void ObrótBudynku()
    {
        PomocniczeFunkcje.spawnBudynki.ObróćBudynek();
    }
    ///<summary>Metoda aktywuje lub deaktywuje przycisk obrotu budynku.</summary>
    ///<param name="wartośćPrzycisku">Wartość przypisania do Set.Active()</param>
    public void UstawPrzyciskObrotu(bool wartośćPrzycisku)
    {
        rotacjaBudynku.gameObject.SetActive(wartośćPrzycisku);
    }
    ///<summary>Metoda rozpoczyna proces wzmacniania budynków przy pomocy akademii.</summary>
    ///<param name="indeksButtonu">Indeks, które ulepszenie jest wybrane: 1 - Zycie, 2 - Atak, 3 - Obrona</param>
    public void KliknalemButtonRozwoju(int indeksButtonu)   //1 - Zycie, 2 - Atak, 3 - Obrona
    {
        PomocniczeFunkcje.managerGryScript.RozwójBudynkow((byte)indeksButtonu);
    }
    ///<summary>Wyłącz aplikację.</summary>
    public void QuitGame()
    {
        if (!ManagerSamouczekScript.byloZaladowane)
            PomocniczeFunkcje.ZapiszDane();
        Application.Quit();
    }
    public void WlWylToogleOdwrocenieKamery()
    {
        Toggle t = this.transform.Find("Menu/OptionsMenu/GrafikaPanel/CzyOdwrócićPrzesuwanie").GetComponent<Toggle>();
        MoveCameraScript.odwrócPrzesuwanie = t.isOn;
    }
    public void SetToogleOdwrocenieKamery(bool value)
    {
        Toggle t = this.transform.Find("Menu/OptionsMenu/GrafikaPanel/CzyOdwrócićPrzesuwanie").GetComponent<Toggle>();
        t.isOn = value;
    }
    #endregion
}
